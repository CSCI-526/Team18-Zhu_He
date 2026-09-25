using System.Collections.Generic;
using UnityEngine;

// stores a convex shape as a bunch of flat polygon faces
// handles cutting it with a plane, getting the volume, building a mesh out of it
// this isn't a component, just plain data, so you never attach it to a GameObject
public class ConvexShape
{
    public readonly List<Vector3[]> Faces;
    const float Eps = 1e-4f;

    public ConvexShape(List<Vector3[]> faces) { Faces = faces; }

    // makes a box centered on the origin
    public static ConvexShape Box(Vector3 size)
    {
        Vector3 h = size * 0.5f;
        Vector3 V(float x, float y, float z) => new Vector3(x * h.x, y * h.y, z * h.z);
        return new ConvexShape(new List<Vector3[]>
        {
            new[] { V( 1,-1,-1), V( 1, 1,-1), V( 1, 1, 1), V( 1,-1, 1) },
            new[] { V(-1,-1,-1), V(-1,-1, 1), V(-1, 1, 1), V(-1, 1,-1) },
            new[] { V(-1, 1,-1), V(-1, 1, 1), V( 1, 1, 1), V( 1, 1,-1) },
            new[] { V(-1,-1,-1), V( 1,-1,-1), V( 1,-1, 1), V(-1,-1, 1) },
            new[] { V(-1,-1, 1), V( 1,-1, 1), V( 1, 1, 1), V(-1, 1, 1) },
            new[] { V(-1,-1,-1), V(-1, 1,-1), V( 1, 1,-1), V( 1,-1,-1) },
        });
    }

    public ConvexShape Transformed(Matrix4x4 m)
    {
        var faces = new List<Vector3[]>(Faces.Count);
        foreach (var f in Faces)
        {
            var nf = new Vector3[f.Length];
            for (int i = 0; i < f.Length; i++) nf[i] = m.MultiplyPoint3x4(f[i]);
            faces.Add(nf);
        }
        return new ConvexShape(faces);
    }

    public ConvexShape Offset(Vector3 delta) => Transformed(Matrix4x4.Translate(delta));

    public Vector3 Centroid()
    {
        Vector3 sum = Vector3.zero; int n = 0;
        foreach (var f in Faces) foreach (var p in f) { sum += p; n++; }
        return n > 0 ? sum / n : Vector3.zero;
    }

    public Bounds GetBounds()
    {
        var b = new Bounds(Faces[0][0], Vector3.zero);
        foreach (var f in Faces) foreach (var p in f) b.Encapsulate(p);
        return b;
    }

    public float Volume()
    {
        Vector3 c = Centroid();
        float v = 0f;
        foreach (var f in Faces)
            for (int i = 1; i < f.Length - 1; i++)
                v += Mathf.Abs(Vector3.Dot(f[0] - c, Vector3.Cross(f[i] - c, f[i + 1] - c))) / 6f;
        return v;
    }

    // cuts the shape with the plane dot(normal, p) = d
    // returns false if the plane doesn't actually pass through the shape
    public bool Split(Vector3 normal, float d, out ConvexShape front, out ConvexShape back)
    {
        front = back = null;
        var frontFaces = new List<Vector3[]>();
        var backFaces = new List<Vector3[]>();
        var capPoints = new List<Vector3>();

        foreach (var f in Faces)
        {
            int count = f.Length;
            var s = new float[count];
            bool allOnPlane = true;
            for (int i = 0; i < count; i++)
            {
                s[i] = Vector3.Dot(normal, f[i]) - d;
                if (Mathf.Abs(s[i]) > Eps) allOnPlane = false;
            }
            if (allOnPlane) continue;

            var fp = new List<Vector3>();
            var bp = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                Vector3 a = f[i], b = f[j];
                float sa = s[i], sb = s[j];

                if (sa > Eps) fp.Add(a);
                else if (sa < -Eps) bp.Add(a);
                else { fp.Add(a); bp.Add(a); capPoints.Add(a); }

                if ((sa > Eps && sb < -Eps) || (sa < -Eps && sb > Eps))
                {
                    Vector3 p = Vector3.Lerp(a, b, sa / (sa - sb));
                    fp.Add(p); bp.Add(p); capPoints.Add(p);
                }
            }
            if (fp.Count >= 3) frontFaces.Add(fp.ToArray());
            if (bp.Count >= 3) backFaces.Add(bp.ToArray());
        }

        // get rid of duplicate points from the cut
        var pts = new List<Vector3>();
        foreach (var p in capPoints)
        {
            bool dup = false;
            foreach (var q in pts) if ((p - q).sqrMagnitude < 1e-8f) { dup = true; break; }
            if (!dup) pts.Add(p);
        }
        if (pts.Count < 3 || frontFaces.Count < 3 || backFaces.Count < 3) return false;

        // sort them by angle so they actually form a polygon
        Vector3 c = Vector3.zero;
        foreach (var p in pts) c += p;
        c /= pts.Count;
        Vector3 u = Mathf.Abs(normal.x) < 0.9f ? Vector3.right : Vector3.up;
        u = (u - normal * Vector3.Dot(u, normal)).normalized;
        Vector3 w = Vector3.Cross(normal, u);
        pts.Sort((p, q) =>
            Mathf.Atan2(Vector3.Dot(p - c, w), Vector3.Dot(p - c, u))
                .CompareTo(Mathf.Atan2(Vector3.Dot(q - c, w), Vector3.Dot(q - c, u))));

        frontFaces.Add(pts.ToArray());
        backFaces.Add(pts.ToArray());
        front = new ConvexShape(frontFaces);
        back = new ConvexShape(backFaces);
        return true;
    }

    // builds a flat shaded mesh with every face pointing outward
    public Mesh BuildMesh()
    {
        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var tris = new List<int>();
        Vector3 c = Centroid();

        foreach (var face in Faces)
        {
            Vector3[] f = face;
            Vector3 n = Newell(f);
            Vector3 fc = Vector3.zero;
            foreach (var p in f) fc += p;
            fc /= f.Length;
            if (Vector3.Dot(n, fc - c) < 0f)
            {
                f = (Vector3[])f.Clone();
                System.Array.Reverse(f);
                n = -n;
            }
            int start = verts.Count;
            foreach (var p in f) { verts.Add(p); normals.Add(n); }
            for (int i = 1; i < f.Length - 1; i++)
            {
                tris.Add(start); tris.Add(start + i); tris.Add(start + i + 1);
            }
        }

        var mesh = new Mesh { name = "CutPiece" };
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    static Vector3 Newell(Vector3[] f)
    {
        Vector3 n = Vector3.zero;
        for (int i = 0; i < f.Length; i++)
        {
            Vector3 a = f[i], b = f[(i + 1) % f.Length];
            n.x += (a.y - b.y) * (a.z + b.z);
            n.y += (a.z - b.z) * (a.x + b.x);
            n.z += (a.x - b.x) * (a.y + b.y);
        }
        return n.sqrMagnitude > 1e-12f ? n.normalized : Vector3.up;
    }
}

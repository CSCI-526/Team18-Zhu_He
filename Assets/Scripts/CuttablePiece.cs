using UnityEngine;

// put this on the Player (leftover pieces get it added automatically too)
// builds the mesh and collider, and sets the Rigidbody mass based on volume
// keep the transform rotation at 0,0,0 and scale at 1,1,1, use Start Size to change size
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CuttablePiece : MonoBehaviour
{
    [Tooltip("starting size, 2 x 2 x 2 is the starting cube")]
    public Vector3 startSize = new Vector3(2f, 2f, 2f);

    [Tooltip("mass per unit of volume, with 1 a 2x2x2 cube has mass 8")]
    public float density = 1f;

    public ConvexShape LocalShape { get; private set; }
    public float Volume { get; private set; }
    public Vector3 Size => LocalShape != null ? LocalShape.GetBounds().size : startSize;

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Rigidbody body;

    void Awake()
    {
        CacheComponents();
        if (LocalShape == null) SetLocalShape(ConvexShape.Box(startSize));
    }

    void CacheComponents()
    {
        if (meshFilter != null) return;
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        body = GetComponent<Rigidbody>();
    }

    public void SetLocalShape(ConvexShape shape)
    {
        CacheComponents();
        LocalShape = shape;
        Mesh mesh = shape.BuildMesh();

        Mesh old = meshFilter.sharedMesh;
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = null;
        meshCollider.convex = true;
        meshCollider.sharedMesh = mesh;
        if (old != null && old.name == "CutPiece") Destroy(old);

        Volume = shape.Volume();
        body.mass = Mathf.Max(0.05f, Volume * density);
    }

    // current shape in world coordinates
    public ConvexShape WorldShape() => LocalShape.Transformed(transform.localToWorldMatrix);

    // swaps in a world space shape and moves the object to its new center
    public void SetWorldShape(ConvexShape world)
    {
        CacheComponents();
        Vector3 center = world.GetBounds().center;
        transform.SetPositionAndRotation(center, Quaternion.identity);
        body.position = center;
        body.rotation = Quaternion.identity;
        SetLocalShape(world.Offset(-center));
        Physics.SyncTransforms();
    }

    // just draws the starting size as a gizmo in edit mode since the mesh only builds at runtime
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = new Color(0.18f, 0.36f, 0.83f, 1f);
        Gizmos.DrawWireCube(transform.position, startSize);
    }
}

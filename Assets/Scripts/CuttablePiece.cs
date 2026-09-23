using UnityEngine;

/// <summary>
/// Put this on the Player (and it is added automatically to leftover pieces).
/// It builds the shape's mesh and collider, and sets the Rigidbody mass from the volume.
/// Keep the Transform rotation at 0, 0, 0 and scale at 1, 1, 1; use Start Size to set the size.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CuttablePiece : MonoBehaviour
{
    [Tooltip("Starting size. 2 x 2 x 2 is the starting cube.")]
    public Vector3 startSize = new Vector3(2f, 2f, 2f);

    [Tooltip("Mass per unit of volume. With 1, a 2 x 2 x 2 cube has mass 8.")]
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

    /// <summary>The current shape in world coordinates.</summary>
    public ConvexShape WorldShape() => LocalShape.Transformed(transform.localToWorldMatrix);

    /// <summary>Replaces this piece's shape with a world-space shape and moves the object to its center.</summary>
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

    // Shows the starting size in the Scene view, since the mesh is only built in Play mode.
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = new Color(0.18f, 0.36f, 0.83f, 1f);
        Gizmos.DrawWireCube(transform.position, startSize);
    }
}

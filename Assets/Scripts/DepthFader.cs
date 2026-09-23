using UnityEngine;

/// <summary>
/// In 2D, fades every Fadeable object that does not overlap the player's depth,
/// so the player can tell what is actually in their path. In 3D nothing fades.
/// Links: Camera Rig, Player.
/// </summary>
public class DepthFader : MonoBehaviour
{
    public CameraRig cameraRig;
    public CuttablePiece player;
    [Range(0f, 1f)] public float fadedAlpha = 0.2f;

    Collider playerCollider;

    void Awake() { playerCollider = player.GetComponent<Collider>(); }

    void LateUpdate()
    {
        if (!playerCollider.enabled) return; // player is hidden while choosing a piece

        Vector3 axis = cameraRig.DepthAxis;
        int k = Mathf.Abs(axis.x) > 0.5f ? 0 : 2; // 0 = x, 2 = z
        Bounds pb = playerCollider.bounds;
        float pMin = pb.min[k], pMax = pb.max[k];
        const float eps = 0.001f;

        foreach (Fadeable f in Fadeable.All)
        {
            Bounds b = f.Bounds;
            bool separate = b.max[k] <= pMin + eps || b.min[k] >= pMax - eps;
            f.SetFaded(cameraRig.Is2D && separate, fadedAlpha);
        }
    }
}

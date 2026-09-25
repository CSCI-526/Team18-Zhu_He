using UnityEngine;

public class DepthFader : MonoBehaviour
{
    public CameraRig camScript;
    public CuttablePiece player;
    public float fadeAlpha = 0.2f;

    private Collider playerCol;

    void Awake()
    {
        playerCol = player.GetComponent<Collider>();
    }

    void LateUpdate()
    {
        if (!playerCol.enabled) return;

        Vector3 depthDir = camScript.GetDepthDir();
        int axis = 2;
        if (Mathf.Abs(depthDir.x) > 0.5f) axis = 0;

        Bounds playerBounds = playerCol.bounds;
        float playerMin = playerBounds.min[axis];
        float playerMax = playerBounds.max[axis];
        float gap = 0.001f;

        foreach (Fadeable obj in Fadeable.allFadeables)
        {
            Bounds bounds = obj.GetBounds();
            bool notOverlapping = false;
            if (bounds.max[axis] <= playerMin + gap || bounds.min[axis] >= playerMax - gap)
            {
                notOverlapping = true;
            }
            obj.SetFade(camScript.is2D && notOverlapping, fadeAlpha);
        }
    }
}

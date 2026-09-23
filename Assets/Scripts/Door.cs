using UnityEngine;

/// <summary>
/// Put this on the door object (a cube with a Box Collider).
/// A Pressure Plate opens and closes it. It will not close on top of the player or a piece.
/// No links needed; the Pressure Plate links to this.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    Collider col;
    Renderer rend;
    Bounds closedBounds;
    readonly Collider[] buffer = new Collider[16];

    void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        closedBounds = col.bounds;
    }

    public void SetOpen(bool open)
    {
        if (open == IsOpen) return;
        if (!open && SomethingInside()) return; // wait until the doorway is clear
        IsOpen = open;
        col.enabled = !open;
        if (rend != null) rend.enabled = !open;
    }

    bool SomethingInside()
    {
        int n = Physics.OverlapBoxNonAlloc(closedBounds.center, closedBounds.extents * 0.98f, buffer,
                                           Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            Rigidbody rb = buffer[i].attachedRigidbody;
            if (rb != null && !rb.isKinematic) return true;
        }
        return false;
    }
}

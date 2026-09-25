using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen;

    private Collider col;
    private Renderer rend;
    private Bounds closedBounds;

    void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        closedBounds = col.bounds;
    }

    public void SetDoor(bool open)
    {
        if (open != isOpen)
        {
            bool somethingInside = false;
            if (!open)
            {
                Collider[] cols = Physics.OverlapBox(closedBounds.center, closedBounds.extents * 0.98f, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < cols.Length; i++)
                {
                    Rigidbody rb = cols[i].attachedRigidbody;
                    if (rb != null && !rb.isKinematic)
                    {
                        somethingInside = true;
                    }
                }
            }

            if (!somethingInside)
            {
                isOpen = open;
                col.enabled = !open;
                if (rend != null)
                {
                    rend.enabled = !open;
                }
            }
        }
    }
}

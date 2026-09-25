using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Door door;
    public float massNeeded = 1.5f;
    public float checkHeight = 0.4f;

    public float currentMass;
    public bool isPressed;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        Bounds bounds;
        if (rend != null) bounds = rend.bounds;
        else bounds = GetComponent<Renderer>().bounds;

        float boxBottom = bounds.max.y - 0.05f;
        Vector3 boxCenter = new Vector3(bounds.center.x, boxBottom + checkHeight * 0.5f, bounds.center.z);
        Vector3 boxHalf = new Vector3(bounds.extents.x, checkHeight * 0.5f, bounds.extents.z);

        Collider[] cols = Physics.OverlapBox(boxCenter, boxHalf, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

        float totalMass = 0f;
        for (int i = 0; i < cols.Length; i++)
        {
            Rigidbody rb = cols[i].attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                totalMass += rb.mass;
            }
        }
        currentMass = totalMass;
        isPressed = totalMass >= massNeeded;
        if (door != null)
        {
            door.SetDoor(isPressed);
        }
    }

    void OnDrawGizmosSelected()
    {
        Bounds bounds;
        if (rend != null) bounds = rend.bounds;
        else bounds = GetComponent<Renderer>().bounds;

        float boxBottom = bounds.max.y - 0.05f;
        Vector3 boxCenter = new Vector3(bounds.center.x, boxBottom + checkHeight * 0.5f, bounds.center.z);
        Vector3 boxHalf = new Vector3(bounds.extents.x, checkHeight * 0.5f, bounds.extents.z);

        Gizmos.color = new Color(0.89f, 0.7f, 0.24f, 0.6f);
        Gizmos.DrawWireCube(boxCenter, boxHalf * 2f);
    }
}

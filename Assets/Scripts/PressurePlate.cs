using System.Collections.Generic;
using UnityEngine;

// put this on the plate object. adds up the mass of everything resting on top of it
// and keeps the linked door open while that mass is at least Required Mass
[RequireComponent(typeof(Renderer))]
public class PressurePlate : MonoBehaviour
{
    public Door door;
    public float requiredMass = 1.5f;
    [Tooltip("how far above the plate's top surface objects get detected")]
    public float detectHeight = 0.4f;

    public float CurrentMass { get; private set; }
    public bool IsPressed { get; private set; }

    Renderer rend;
    readonly Collider[] buffer = new Collider[32];
    readonly HashSet<Rigidbody> counted = new HashSet<Rigidbody>();

    void Awake() { rend = GetComponent<Renderer>(); }

    void FixedUpdate()
    {
        GetZone(out Vector3 center, out Vector3 halfSize);
        int count = Physics.OverlapBoxNonAlloc(center, halfSize, buffer, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

        counted.Clear();
        float mass = 0f;
        for (int i = 0; i < count; i++)
        {
            Rigidbody rb = buffer[i].attachedRigidbody;
            if (rb != null && !rb.isKinematic && counted.Add(rb)) mass += rb.mass;
        }
        CurrentMass = mass;
        IsPressed = mass >= requiredMass;
        if (door != null) door.SetOpen(IsPressed);
    }

    void GetZone(out Vector3 center, out Vector3 halfSize)
    {
        Bounds b = (rend != null ? rend : GetComponent<Renderer>()).bounds;
        float bottom = b.max.y - 0.05f;
        center = new Vector3(b.center.x, bottom + detectHeight * 0.5f, b.center.z);
        halfSize = new Vector3(b.extents.x, detectHeight * 0.5f, b.extents.z);
    }

    void OnDrawGizmosSelected()
    {
        GetZone(out Vector3 c, out Vector3 h);
        Gizmos.color = new Color(0.89f, 0.7f, 0.24f, 0.6f);
        Gizmos.DrawWireCube(c, h * 2f);
    }
}

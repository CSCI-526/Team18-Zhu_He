using UnityEngine;

/// <summary>
/// Put this on the exit object. Its collider must be a trigger (this is set automatically when you add the script).
/// When the player touches it, the next level loads.
/// Links: Level Manager.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitZone : MonoBehaviour
{
    public LevelManager levelManager;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rb.GetComponent<PlayerController>() != null) levelManager.CompleteLevel();
    }
}

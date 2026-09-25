using UnityEngine;

// put this on the exit object, collider needs Is Trigger checked (gets set automatically when added)
// when the player touches it, the next level loads
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

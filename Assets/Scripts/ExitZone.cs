using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public LevelManager levelManager;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rb.GetComponent<PlayerController>() != null)
        {
            levelManager.NextLevel();
        }
    }
}

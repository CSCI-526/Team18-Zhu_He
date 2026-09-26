using TMPro;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public LevelManager levelManager;
    public string labelText = "EXIT";
    public float labelSize = 14f;

    private GameObject label;
    private Vector3 labelPos;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        Bounds bounds = GetComponent<Collider>().bounds;
        labelPos = new Vector3(bounds.center.x, bounds.max.y + 1.2f, bounds.center.z);

        label = new GameObject("Exit Label");
        label.transform.position = labelPos;

        TextMeshPro text = label.AddComponent<TextMeshPro>();
        text.text = labelText;
        text.fontSize = labelSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.1f, 0.6f, 0.4f, 1f);
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.white;
        text.rectTransform.sizeDelta = new Vector2(8f, 3f);
    }

    void LateUpdate()
    {
        if (label == null) return;

        label.transform.position = labelPos + Vector3.up * Mathf.Sin(Time.unscaledTime * 3f) * 0.2f;
        if (Camera.main != null)
        {
            label.transform.rotation = Camera.main.transform.rotation;
        }
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

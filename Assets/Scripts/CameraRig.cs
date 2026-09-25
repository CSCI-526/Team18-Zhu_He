using UnityEngine;

// this goes on the Main Camera
// 2D mode: orthographic view, Q/E rotates it in 90 degree steps
// 3D mode: perspective orbit cam, Q/E or hold right mouse to orbit, scroll to zoom
// Tab switches between the two
public class CameraRig : MonoBehaviour
{
    public Camera cam;
    public Transform target;

    [Header("2D View")]
    public float orthoSize = 6.5f;
    public float distance2D = 40f;

    [Header("3D View")]
    public float distance3D = 15f;
    public float minDistance3D = 7f;
    public float maxDistance3D = 28f;
    public float pitch3D = 25f;
    public float keyOrbitSpeed = 120f;
    public float mouseOrbitSpeed = 0.25f;

    [Header("Follow")]
    public float followSharpness = 8f;
    public float rotateSharpness = 10f;
    public float heightOffset = 0.8f;

    public bool Is2D { get; private set; } = true;
    public bool AllowInput { get; set; } = true;

    float yaw, yawTarget, pitch;
    Vector3 focus;
    Vector3 lastMouse;

    // player can't move while the view is still turning
    public bool IsRotating => Is2D && Mathf.Abs(Mathf.DeltaAngle(yaw, yawTarget)) > 1f;
    // screen "right" direction, snapped to a world axis
    public Vector3 ScreenRight => Snap(Quaternion.Euler(0f, yawTarget, 0f) * Vector3.right);
    // depth axis, basically the direction the camera is looking that you can't see
    public Vector3 DepthAxis => Snap(Quaternion.Euler(0f, yawTarget, 0f) * Vector3.forward);
    public Vector3 FlatRight => Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
    public Vector3 FlatForward => Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

    public string ViewName
    {
        get
        {
            if (!Is2D) return "3D free camera";
            int k = ((Mathf.RoundToInt(yawTarget / 90f) % 4) + 4) % 4;
            return new[] { "2D front view", "2D left side view", "2D back view", "2D right side view" }[k];
        }
    }

    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
        pitch = pitch3D;
        if (target != null) focus = target.position;
        Apply();
    }

    void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime; // needs to keep going even when Cut Mode pauses time
        if (AllowInput) HandleInput(dt);
        yaw = Mathf.LerpAngle(yaw, yawTarget, 1f - Mathf.Exp(-rotateSharpness * dt));
        if (Mathf.Abs(Mathf.DeltaAngle(yaw, yawTarget)) < 0.05f) yaw = yawTarget;
        if (target != null && Time.timeScale > 0f)
            focus = Vector3.Lerp(focus, target.position, 1f - Mathf.Exp(-followSharpness * dt));
        Apply();
    }

    void HandleInput(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ToggleView();

        if (Is2D)
        {
            if (Input.GetKeyDown(KeyCode.Q)) yawTarget -= 90f;
            if (Input.GetKeyDown(KeyCode.E)) yawTarget += 90f;
        }
        else
        {
            float turn = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
            if (turn != 0f) { yawTarget += turn * keyOrbitSpeed * dt; yaw = yawTarget; }

            if (Input.GetMouseButtonDown(1)) lastMouse = Input.mousePosition;
            if (Input.GetMouseButton(1))
            {
                Vector3 delta = Input.mousePosition - lastMouse;
                yawTarget += delta.x * mouseOrbitSpeed;
                yaw = yawTarget;
                pitch = Mathf.Clamp(pitch - delta.y * mouseOrbitSpeed, 5f, 75f);
                lastMouse = Input.mousePosition;
            }
            distance3D = Mathf.Clamp(distance3D - Input.mouseScrollDelta.y, minDistance3D, maxDistance3D);
        }
    }

    public void ToggleView()
    {
        Is2D = !Is2D;
        if (Is2D) yawTarget = Mathf.Round(yaw / 90f) * 90f; // snap to closest side
    }

    void Apply()
    {
        Vector3 f = focus + Vector3.up * heightOffset;
        Quaternion rot = Quaternion.Euler(Is2D ? 0f : pitch, yaw, 0f);
        float dist = Is2D ? distance2D : distance3D;
        cam.orthographic = Is2D;
        cam.orthographicSize = orthoSize;
        cam.transform.SetPositionAndRotation(f - rot * Vector3.forward * dist, rot);
    }

    static Vector3 Snap(Vector3 v) => new Vector3(Mathf.Round(v.x), 0f, Mathf.Round(v.z));
}

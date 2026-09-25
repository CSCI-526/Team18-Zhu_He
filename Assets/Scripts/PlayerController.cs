using UnityEngine;

// moves the player. in 2D it only moves left/right along the screen (which rotates with the view)
// in 3D it moves with WASD relative to the camera. space jumps
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public CameraRig cameraRig;
    public float moveSpeed = 6f;
    public float jumpVelocity = 8.5f;

    [HideInInspector] public bool inputEnabled = true;

    Rigidbody body;
    bool grounded;
    bool jumpQueued;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation; // the shape itself never rotates
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        if (inputEnabled && Input.GetKeyDown(KeyCode.Space)) jumpQueued = true;
    }

    void FixedUpdate()
    {
        Vector3 move = Vector3.zero;
        if (inputEnabled)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (cameraRig.Is2D)
            {
                if (!cameraRig.IsRotating) move = cameraRig.ScreenRight * h;
            }
            else
            {
                move = cameraRig.FlatForward * v + cameraRig.FlatRight * h;
                if (move.sqrMagnitude > 1f) move.Normalize();
            }
        }

        Vector3 vel = GetVelocity();
        vel.x = move.x * moveSpeed;
        vel.z = move.z * moveSpeed;
        if (jumpQueued && grounded) vel.y = jumpVelocity;
        jumpQueued = false;
        grounded = false; // gets set again next physics step by OnCollisionStay
        SetVelocity(vel);
    }

    void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f) { grounded = true; return; }
        }
    }

    public void StopMoving() => SetVelocity(Vector3.zero);

    Vector3 GetVelocity() => body.linearVelocity;

    void SetVelocity(Vector3 v) => body.linearVelocity = v;
}

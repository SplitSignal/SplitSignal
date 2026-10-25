using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 3f;
    public float maxLookAngle = 85f;
    public float gravity = -9.81f;

    public float climbSpeed = 2f;

    private CharacterController controller;
    private float verticalVelocity;
    private float yaw;
    private float pitch;
    private bool climbing = false;
    private float climbTargetY;
    private Vector3 ledgeDirection;
    private float ledgeDistance;

    void Awake()
    {
        if (!TryGetComponent(out controller)) controller = gameObject.AddComponent<CharacterController>();

        // Start looking whichever way the object is already facing in the scene
        yaw = transform.eulerAngles.y;
        pitch = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
    }

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        // Esc frees the cursor, clicking back into the game captures it again
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Input.GetMouseButtonDown(0)) LockCursor(true);

        // Mouse / trackpad looks around: left/right turns, up/down tilts the view
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // WASD movement along the ground, so looking up or down doesn't change where you walk
        Quaternion facing = Quaternion.Euler(0f, yaw, 0f);
        Vector3 forward = facing * Vector3.forward;
        Vector3 right = facing * Vector3.right;

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += forward;
        if (Input.GetKey(KeyCode.S)) move -= forward;
        if (Input.GetKey(KeyCode.A)) move -= right;
        if (Input.GetKey(KeyCode.D)) move += right;
        move = move.normalized * moveSpeed;

        if (climbing)
        {
            // WASD releases the ice axe and lets the player fall
            if (Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D))
            {
                climbing = false;
                verticalVelocity = 0f;
                Debug.Log("Falling!");
            }
            else
            {
                // Stay attached to the mountain.
                // No gravity and no automatic climbing.
                verticalVelocity = 0f;
                return;
            }
        }

        // Keep the player on the ground
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        // Move() stops at colliders and slides along walls instead of passing through them
        controller.Move(move * Time.deltaTime);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void BeginClimb(RaycastHit hit)
    {
        // Calculate the top of the mountain
        float feetOffset = controller.center.y - controller.height * 0.5f;
        climbTargetY = hit.collider.bounds.max.y + 0.15f - feetOffset;

        // Don't climb above the mountain
        float remaining = climbTargetY - transform.position.y;
        if (remaining <= 0f) return;

        // Remember the direction of the mountain
        Vector3 towardWall = hit.point - transform.position;
        towardWall.y = 0f;

        ledgeDirection = towardWall.normalized;
        ledgeDistance = towardWall.magnitude + controller.radius + 0.15f;

        // Hold position against the mountain
        climbing = true;
        verticalVelocity = 0f;

        // Move up exactly 1 unit per successful hit
        float climbAmount = Mathf.Min(1f, remaining);
        controller.Move(Vector3.up * climbAmount);

        Debug.Log("Climbed 1 unit!");

        // Once at the top, move onto the ledge
        if (transform.position.y >= climbTargetY - 0.01f)
        {
            climbing = false;
            verticalVelocity = 0f;

            controller.Move(ledgeDirection * ledgeDistance);

            Debug.Log("Reached the top!");
        }
    }
}


using UnityEngine;

public class IceAxe : MonoBehaviour
{
    public PlayerMovement player;
    public Camera playerCamera;

    public float reachDistance = 3f;

    // Where the axe appears on the screen
    public Vector3 holdPosition = new Vector3(0.4f, -0.35f, 0.7f);
    public Vector3 holdRotation = new Vector3(0f, -80f, -25f);

    // How the axe rotates when swung
    public Vector3 swingRotation = new Vector3(-75f, 0f, 30f);
    public float swingDuration = 0.3f;

    private bool holdingAxe = false;
    private float swingTimer = 0f;
    private Collider[] axeColliders;
    private Rigidbody rb;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = player.GetComponentInChildren<Camera>();

        axeColliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Right-click to drop axe
        if (Input.GetMouseButtonDown(1) && holdingAxe)
        {
            DropAxe();
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (playerCamera == null || player == null) return;

        Ray ray;

        if (Cursor.lockState == CursorLockMode.Locked)
            ray = new Ray(playerCamera.transform.position,
                          playerCamera.transform.forward);
        else
            ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (!holdingAxe)
        {
            // First click: pick up the axe
            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
            {
                if (hit.transform == transform ||
                    hit.transform.IsChildOf(transform))
                {
                    PickUp();
                }
            }
        }
        else
        {
            // Every later click: swing the axe
            swingTimer = swingDuration;

            // Only climb when facing a mountain
            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
            {
                if (hit.collider.CompareTag("Mountain"))
                {
                    player.BeginClimb(hit);
                    Debug.Log("Climbing");
                }
                else
                {
                    Debug.Log("Cannot climb this object");
                }
            }
        }
    }

    void LateUpdate()
    {
        if (!holdingAxe) return;

        Quaternion restingRotation = Quaternion.Euler(holdRotation);

        if (swingTimer > 0f)
        {
            float progress = 1f - Mathf.Clamp01(
                swingTimer / swingDuration);

            float amount = Mathf.Sin(progress * Mathf.PI);

            // Swing up/down relative to the camera,
            // regardless of how the axe model is rotated
            transform.localRotation =
                Quaternion.AngleAxis(
                    swingRotation.x * amount,
                    Vector3.right
                ) * restingRotation;

            swingTimer -= Time.deltaTime;
        }
        else
        {
            transform.localRotation = restingRotation;
        }
    }

    void PickUp()
    {
        holdingAxe = true;

        // Disable collisions while holding it
        foreach (Collider col in axeColliders)
            col.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Attach the axe to the player's camera
        transform.SetParent(playerCamera.transform);
        transform.localPosition = holdPosition;
        transform.localRotation = Quaternion.Euler(holdRotation);

        Debug.Log("Picked up Ice Axe");
    }

    void DropAxe()
    {
        holdingAxe = false;
        swingTimer = 0f;
        transform.SetParent(null, true);

        foreach (Collider col in axeColliders)
            col.enabled = true;

        Debug.Log("Dropped Ice Axe");
    }
}

using UnityEngine;

public class IceAxe : MonoBehaviour
{
    public PlayerMovement player;
    public Camera playerCamera;

    public float reachDistance = 3f;

    [Header("Objects to click while holding the axe")]
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;
    public GameObject objectToReveal;

    private bool clicked1;
    private bool clicked2;
    private bool clicked3;
    private bool objectiveCompleted;

    [Header("Axe position")]
    public Vector3 holdPosition = new Vector3(0.4f, -0.35f, 0.7f);
    public Vector3 holdRotation = new Vector3(0f, -80f, -25f);

    [Header("Axe swing")]
    public Vector3 swingRotation = new Vector3(-75f, 0f, 30f);
    public float swingDuration = 0.3f;

    private bool holdingAxe = false;
    private float swingTimer = 0f;
    private Collider[] axeColliders;
    private Rigidbody rb;

    void Start()
    {
        if (playerCamera == null && player != null)
            playerCamera = player.GetComponentInChildren<Camera>();

        axeColliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();

        if (objectToReveal != null)
            objectToReveal.SetActive(false);
    }

    void Update()
    {
        // Right-click to drop axe.
        if (Input.GetMouseButtonDown(1) && holdingAxe)
        {
            DropAxe();
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (playerCamera == null || player == null) return;

        Ray ray;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );
        }
        else
        {
            ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        }

        if (!holdingAxe)
        {
            // First click: pick up the axe.
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
            // Later clicks: swing and check the target.
            swingTimer = swingDuration;

            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
            {
                CheckObjective(hit.collider.transform);

                if (hit.collider.CompareTag("Mountain"))
                {
                    player.BeginClimb(hit);
                    Debug.Log("Climbing");
                }
            }
        }
    }

    void CheckObjective(Transform hitTransform)
    {
        if (objectiveCompleted) return;

        if (!clicked1 && MatchesTarget(hitTransform, object1))
        {
            clicked1 = true;
            object1.SetActive(false);
        }

        if (!clicked2 && MatchesTarget(hitTransform, object2))
        {
            clicked2 = true;
            object2.SetActive(false);
        }

        if (!clicked3 && MatchesTarget(hitTransform, object3))
        {
            clicked3 = true;
            object3.SetActive(false);
        }

        if (clicked1 && clicked2 && clicked3)
        {
            objectiveCompleted = true;

            if (objectToReveal != null)
                objectToReveal.SetActive(true);

            Debug.Log("All three objects clicked!");
        }
    }

    bool MatchesTarget(Transform hitTransform, GameObject target)
    {
        if (target == null) return false;

        return hitTransform == target.transform ||
               hitTransform.IsChildOf(target.transform);
    }

    void LateUpdate()
    {
        if (!holdingAxe) return;

        Quaternion restingRotation = Quaternion.Euler(holdRotation);

        if (swingTimer > 0f)
        {
            float progress = 1f - Mathf.Clamp01(
                swingTimer / swingDuration
            );

            float amount = Mathf.Sin(progress * Mathf.PI);

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

        foreach (Collider col in axeColliders)
            col.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

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
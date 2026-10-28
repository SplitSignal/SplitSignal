using UnityEngine;

// A HitButton launches this. While flying it slowly falls, and when it hits something
// the view goes back to the player, the player can move again, and the button resets.
public class Paraglider : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 100f;
    public float fallSpeed = 1.5f;

    public PlayerMovement player;
    public Camera skyCamera;

    private Camera playerCamera;
    private HitButton launchedBy;
    private Renderer[] renderers;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool flying;
    [SerializeField] private GameObject landingObject;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        renderers = GetComponentsInChildren<Renderer>();
        playerCamera = player.GetComponentInChildren<Camera>();
    }

    void Start()
    {
        // Hidden, and the player's camera in use, until a button launches it
        if (!flying)
        {
            UseSkyCamera(false);
            gameObject.SetActive(false);
        }
    }

    public void Launch(HitButton button)
    {
        launchedBy = button;
        flying = true;
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(startPosition, startRotation);

        // The player stays frozen while you fly
        player.enabled = false;
        UseSkyCamera(true);
    }

    void Update()
    {
        if (!flying) return;

        // WASD movement, relative to which way the paraglider is facing, while slowly falling
        Vector3 move = Vector3.zero;
        move += transform.forward;
        //STUPID UNITY ROTATION - Apparently you really should check it
        // if (Input.GetKey(KeyCode.W)) move += transform.forward;
        // if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        // if (Input.GetKey(KeyCode.A)) move -= transform.right;
        // if (Input.GetKey(KeyCode.D)) move += transform.right;
        Vector3 step = (move.normalized * moveSpeed + Vector3.down * fallSpeed) * Time.deltaTime;

        // Arrow keys turn (rotate) the paraglider left/right
        float turn = 0f;
        if (Input.GetKey(KeyCode.D)) turn += 1f;
        if (Input.GetKey(KeyCode.A)) turn -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) turn += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) turn -= 1f;
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);

        // Check the path ahead so it stops at whatever it hits instead of passing through
        Bounds bounds = GetBounds();
        if (Physics.BoxCast(bounds.center, bounds.extents, step.normalized, out RaycastHit hit,
                Quaternion.identity, step.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            transform.position += step.normalized * hit.distance;

            if (hit.collider.gameObject == landingObject)
            {
                Debug.Log("Land");
            }
            else
            {
                Debug.Log("Crashed");
            }

            Land();
        }
        else
        {
            transform.position += step;
        }
    }

    void Land()
    {
        flying = false;
        UseSkyCamera(false);
        player.enabled = true;
        if (launchedBy != null) launchedBy.Deactivate();
    }

    void UseSkyCamera(bool sky)
    {
        skyCamera.enabled = sky;
        playerCamera.enabled = !sky;

        // Unity only wants one AudioListener on at a time
        if (skyCamera.TryGetComponent(out AudioListener skyListener)) skyListener.enabled = sky;
        if (playerCamera.TryGetComponent(out AudioListener playerListener)) playerListener.enabled = !sky;
    }

    Bounds GetBounds()
    {
        if (renderers.Length == 0) return new Bounds(transform.position, Vector3.one * 0.5f);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        return bounds;
    }
}

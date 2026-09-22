using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 100f;

    void Update()
    {
        // WASD movement, relative to which way the player is facing
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        transform.position += move.normalized * moveSpeed * Time.deltaTime;

        // Arrow keys turn (rotate) the player left/right
        float turn = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) turn += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) turn -= 1f;
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
    }
}

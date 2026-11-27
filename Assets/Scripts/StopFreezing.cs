using UnityEngine;

public class StopFreezing : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private bool playerOn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) SetPlayerOn(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) SetPlayerOn(false);
    }

    private void SetPlayerOn(bool on)
    {
        if (playerOn == on) return;
        playerOn = on;

        APIClient.UpdateReq("icePond", on ? "true" : "false", "Web");
        Debug.Log($"Ice pond: {(on ? "melting stopped" : "melting resumed")}");
    }

    private void OnDisable()
    {
        if (playerOn && APIClient.Instance != null) SetPlayerOn(false);
    }
}
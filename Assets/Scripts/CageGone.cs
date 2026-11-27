using UnityEngine;

public class CageGone : MonoBehaviour
{
    private void OnEnable()
    {
        APIClient.Cage += OpenCage;
    }

    private void OnDisable()
    {
        APIClient.Cage -= OpenCage;
    }

    private void OpenCage(UpdateClient msg)
    {
        Debug.Log($"Cage event received: {msg.Object} / {msg.Status}");

        // Optional: only react to the "open" status the web sends
        if (msg.Status != "open") return;

        gameObject.SetActive(false);   // or Destroy(gameObject);
    }
}
using UnityEngine;

public class Escaped : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            APIClient.UpdateReq("game", "complete", "Web");
            Debug.Log("Game complete!");
        }
    }
}
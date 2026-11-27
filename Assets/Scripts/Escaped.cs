using UnityEngine;

public class Escaped : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Game complete!");
        }
    }
}
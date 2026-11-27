using UnityEngine;

public class SendClue : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject objectToShow;

    [Header("Message sent to the website")]
    [SerializeField] private string clueObject = "code";
    [SerializeField] private string clueStatus = "atbash cipher";

    private bool used;

    private void Start()
    {
        if (objectToShow != null) objectToShow.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) => TryPress(other.gameObject);
    private void OnCollisionEnter(Collision collision) => TryPress(collision.gameObject);

    private void TryPress(GameObject other)
    {
        if (used || !other.CompareTag(playerTag)) return;
        used = true;

        if (objectToShow != null) objectToShow.SetActive(true);
        APIClient.UpdateReq(clueObject, clueStatus, "Web");
        gameObject.SetActive(false);

        Debug.Log($"Clue sent: {clueObject} / {clueStatus}");
    }
}
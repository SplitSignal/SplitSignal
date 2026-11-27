using UnityEngine;

public class RaiseActivate : MonoBehaviour
{
    public float raiseAmount = 1f;
    [SerializeField] private GameObject objectToShow;   // the flag

    private bool raised;
    private RaiseActivate raiseCheck;
    public bool IsRaised => raised;                     // ParaglideActive checks this

    private void Start()
    {
        if (objectToShow != null) objectToShow.SetActive(false);
    }

    private void OnEnable() { APIClient.Paraglider += OnParaglider; }
    private void OnDisable() { APIClient.Paraglider -= OnParaglider; }

    private void OnParaglider(UpdateClient msg)
    {
        if (msg.Status != "active") return;
        Raise();
    }

    public void Raise()
    {
        if (raised) return;
        raised = true;
        transform.position += Vector3.up * raiseAmount;
        if (objectToShow != null) objectToShow.SetActive(true);
    }
}
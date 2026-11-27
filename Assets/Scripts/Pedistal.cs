using UnityEngine;

public class Pedistal : MonoBehaviour
{
    public GameObject floor;
    private bool unlocked;

    private void OnEnable() { APIClient.Pedestal += Unlock; }
    private void OnDisable() { APIClient.Pedestal -= Unlock; }

    private void Unlock(UpdateClient msg)
    {
        unlocked = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (unlocked && other.CompareTag("Player"))
        {
            floor.SetActive(false);
            Invoke(nameof(Showfloor), 10f);
        }
    }

    private void Showfloor()
    {
        floor.SetActive(true);
    }
}
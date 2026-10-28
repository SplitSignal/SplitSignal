using UnityEngine;

public class RaiseActivate : MonoBehaviour
{
    public float raiseAmount = 1f;

    public void Raise()
    {
        transform.position += Vector3.up * raiseAmount;
    }
}

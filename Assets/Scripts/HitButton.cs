using System;
using UnityEngine;

[Serializable]
public class HitButtonInteraction
{
    public string type = "interact";
    public HitButtonPayload payload;
}

[Serializable]
public class HitButtonPayload
{
    public string objectId;
    public string action;
}

public class HitButton : MonoBehaviour
{
    [SerializeField] private string objectId = "red_button";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Renderer[] objectsToChange;
    [SerializeField] private Color activatedColor = Color.red;
    [SerializeField] private bool activateOnlyOnce = true;

    private bool isActivated;

    public bool IsActivated => isActivated;
    public event Action<HitButtonInteraction> Activated;

    private void OnCollisionEnter(Collision collision)
    {
        TryActivate(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryActivate(other.gameObject);
    }

    public void ActivateFromServer()
    {
        Activate();
    }

    private void TryActivate(GameObject other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
        {
            return;
        }

        Activate();
    }

    private void Activate()
    {
        if (activateOnlyOnce && isActivated)
        {
            return;
        }

        isActivated = true;

        if (objectsToChange != null)
        {
            foreach (Renderer target in objectsToChange)
            {
                if (target != null)
                {
                    target.material.color = activatedColor;
                }
            }
        }

        Activated?.Invoke(new HitButtonInteraction
        {
            payload = new HitButtonPayload
            {
                objectId = objectId,
                action = "hit"
            }
        });
    }
}

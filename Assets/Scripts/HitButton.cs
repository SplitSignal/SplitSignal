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
    [SerializeField] private Paraglider paraglider;
    [SerializeField] private RaiseActivate objectToRaise;

    private bool isActivated;
    private Color[] originalColors;

    public string ObjectId => objectId;
    public bool IsActivated => isActivated;

    // Fires when the player changes this button, with the message to send to the server
    public event Action<HitButtonInteraction> Interacted;

    private void Awake()
    {
        // Remember the starting colors so Deactivate() can put them back
        if (objectsToChange != null)
        {
            originalColors = new Color[objectsToChange.Length];
            for (int i = 0; i < objectsToChange.Length; i++)
            {
                if (objectsToChange[i] != null)
                {
                    originalColors[i] = objectsToChange[i].material.color;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryActivate(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryActivate(other.gameObject);
    }

    // The server also sends our own changes back to us, so skip it if we're already in that state.
    // Changes from the server aren't sent back to it.
    public void ActivateFromServer()
    {
        if (!isActivated)
        {
            Activate(true);
        }
    }

    public void DeactivateFromServer()
    {
        if (isActivated)
        {
            Deactivate(true);
        }
    }

    public void Deactivate()
    {
        Deactivate(false);
    }

    private void TryActivate(GameObject other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
        {
            return;
        }

        Activate(false);
    }

    private void Activate(bool fromServer)
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

        if (paraglider != null)
        {
            paraglider.Launch(this);
        }

        if (objectToRaise != null)
        {
            objectToRaise.Raise();
        }

        if (!fromServer)
        {
            SendInteraction("hit");
        }
    }

    private void Deactivate(bool fromServer)
    {
        isActivated = false;

        if (objectsToChange != null)
        {
            for (int i = 0; i < objectsToChange.Length; i++)
            {
                if (objectsToChange[i] != null)
                {
                    objectsToChange[i].material.color = originalColors[i];
                }
            }
        }

        if (!fromServer)
        {
            SendInteraction("deactivate");
        }
    }

    private void SendInteraction(string action)
    {
        Interacted?.Invoke(new HitButtonInteraction
        {
            payload = new HitButtonPayload
            {
                objectId = objectId,
                action = action
            }
        });
    }
}

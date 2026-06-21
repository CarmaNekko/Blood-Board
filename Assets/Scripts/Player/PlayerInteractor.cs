using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text promptText;

    private readonly List<InteractableBase> nearbyInteractables = new List<InteractableBase>();

    public MagicShooter Shooter { get; private set; }

    private void Awake()
    {
        Shooter = GetComponent<MagicShooter>();
        if (Shooter == null)
        {
            Shooter = GetComponentInChildren<MagicShooter>();
        }

        UpdatePrompt();
    }

    private void Update()
    {
        if (PowerUpShopUI.IsOpen)
        {
            return;
        }

        CleanupInvalidInteractables();

        InteractableBase currentInteractable = GetCurrentInteractable();
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }

        UpdatePrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableBase interactable = other.GetComponentInParent<InteractableBase>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableBase interactable = other.GetComponentInParent<InteractableBase>();
        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
        }
    }

    private InteractableBase GetCurrentInteractable()
    {
        InteractableBase bestInteractable = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < nearbyInteractables.Count; i++)
        {
            InteractableBase interactable = nearbyInteractables[i];
            if (interactable == null || !interactable.CanInteract)
            {
                continue;
            }

            float distance = (interactable.transform.position - transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestInteractable = interactable;
            }
        }

        return bestInteractable;
    }

    private void CleanupInvalidInteractables()
    {
        for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
        {
            if (nearbyInteractables[i] == null || !nearbyInteractables[i].CanInteract)
            {
                nearbyInteractables.RemoveAt(i);
            }
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
        {
            return;
        }

        InteractableBase currentInteractable = GetCurrentInteractable();
        promptText.gameObject.SetActive(currentInteractable != null);
        if (currentInteractable != null)
        {
            promptText.text = $"{interactKey}: {currentInteractable.InteractionPrompt}";
        }
    }
}

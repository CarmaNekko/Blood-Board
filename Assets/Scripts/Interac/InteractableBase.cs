using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Interactuar";
    [SerializeField] private bool canInteract = true;

    public string InteractionPrompt => interactionPrompt;
    public bool CanInteract => canInteract && isActiveAndEnabled;

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract || interactor == null)
        {
            return;
        }

        OnInteract(interactor);
    }

    protected abstract void OnInteract(PlayerInteractor interactor);

    protected void SetInteractionEnabled(bool isEnabled)
    {
        canInteract = isEnabled;
    }
}

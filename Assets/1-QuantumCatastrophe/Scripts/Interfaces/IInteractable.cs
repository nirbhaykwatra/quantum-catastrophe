using UnityEngine;

public readonly struct InteractionContext
{
    public readonly GameObject Interactor;

    public InteractionContext(GameObject interactor)
    {
        Interactor = interactor;
    }
}

public interface IInteractable
{
    void Interact(in InteractionContext context);
}

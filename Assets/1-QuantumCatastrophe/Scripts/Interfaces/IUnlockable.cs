using UnityEngine;

public interface IUnlockable
{
    bool IsLocked { get; }
    void TryUnlock(GameObject interactor);
    bool IsUnlocked();
    void Lock();
}

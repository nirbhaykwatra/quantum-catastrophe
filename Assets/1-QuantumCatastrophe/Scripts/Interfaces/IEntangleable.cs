using UnityEngine;

public interface IEntangleable
{
    void OnEntanglementSelected();    // visual feedback — highlight
    void OnEntanglementDeselected();  // remove highlight
    void OnEntangle(IEntangleable other, int order); // link established
    void OnEntanglementBroken();
    
    Vector2 GetVelocity();
}
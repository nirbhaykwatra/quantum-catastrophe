public interface IEntangleable
{
    void OnEntanglementSelected();    // visual feedback — highlight
    void OnEntanglementDeselected();  // remove highlight
    void OnEntangle(IEntangleable other); // link established
    void OnEntanglementBroken();
}
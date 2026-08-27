using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public Vector3 LastCheckpoint;
    public int Health;
    public bool EnableDash;
    public bool EnableAirDash;
    public bool EnableWallJump;
    public bool EnableDoubleJump;
    public bool EnableEntanglementMode;
    public bool EnableTunnelingBarriers;
    public bool EnableSuperposition;
}

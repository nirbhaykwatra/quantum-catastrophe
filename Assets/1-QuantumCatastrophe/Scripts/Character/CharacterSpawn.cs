using System;
using GameEvents;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterSpawn : MonoBehaviour
{
    [SerializeField] private Vector3 SpawnAtLevelStart = new Vector3(0, 0, 0);
    [SerializeField] private bool SpawnFromCheckpoint = true;
    [SerializeField] private BoolEventAsset OnRespawn;
    private Vector3 m_spawnPosition;
    [ReadOnly]
    [ShowInInspector]
    public Vector3 LastCheckpoint { get; set; }
    
    private CharacterHealth m_health;
    private Rigidbody2D m_rigidbody;
    private PlayerController m_playerController;

    private GlobalEventBus _globalEventBus;
    private EventBinding<OnRespawnPlayer> _respawnPlayer;
    
    private void Awake()
    {
        m_health = GetComponent<CharacterHealth>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_playerController = GetComponent<PlayerController>();

        _globalEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<GlobalEventBus>();

        if (PlayerPrefs.HasKey("LastCheckpointX"))
        {
            LastCheckpoint = new Vector3(PlayerPrefs.GetFloat("LastCheckpointX"), PlayerPrefs.GetFloat("LastCheckpointY"), PlayerPrefs.GetFloat("LastCheckpointZ"));
        }
        else
        {
            LastCheckpoint = SpawnAtLevelStart;
        }
        
        Debug.Log($"LastCheckpoint: {LastCheckpoint}");
        if (SpawnFromCheckpoint)
        {
            m_spawnPosition = LastCheckpoint;
            transform.SetPositionAndRotation(m_spawnPosition, Quaternion.identity);
        }
    }

    private void OnEnable()
    {
        _respawnPlayer = new EventBinding<OnRespawnPlayer>(HandlePlayerRespawn);
        _globalEventBus.Register(_respawnPlayer);
    }

    private void OnDisable()
    {
        _globalEventBus.Deregister(_respawnPlayer);
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("Health"))
        {
            m_health.SetHealth(PlayerPrefs.GetInt("Health"));
        }
        else
        {
            m_health.SetHealth(m_health.MaxHealthValue);
        }
    }

    public void SetSpawnPoint(Vector3 position)
    {
        LastCheckpoint = position;
        PlayerPrefs.SetFloat("LastCheckpointX", LastCheckpoint.x);
        PlayerPrefs.SetFloat("LastCheckpointY", LastCheckpoint.y);
        PlayerPrefs.SetFloat("LastCheckpointZ", LastCheckpoint.z);
        PlayerPrefs.Save();
    }
    
    public void TeleportPlayer(Vector3 position)
    {
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }
    
    public void TeleportToLastCheckpoint()
    {
        TeleportPlayer(LastCheckpoint);
    }

    public void Respawn(float inputDisableTimer)
    {
        OnRespawn.Invoke(false);
        m_rigidbody.linearVelocity = Vector2.zero;
        m_playerController.DisableMovement(inputDisableTimer);
    }

    private void HandlePlayerRespawn()
    {
        m_rigidbody.linearVelocity = Vector2.zero;
        TeleportToLastCheckpoint();
        m_playerController.DisableMovement(1f);
    }
}

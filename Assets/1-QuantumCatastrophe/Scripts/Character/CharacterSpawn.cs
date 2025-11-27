using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterSpawn : MonoBehaviour
{
    [SerializeField] private bool SpawnFromCheckpoint = true;
    private Vector3 m_spawnPosition;
    [ReadOnly]
    [ShowInInspector]
    public Vector3 LastCheckpoint { get; set; }
    
    private CharacterHealth m_health;
    
    private void Awake()
    {
        m_health = GetComponent<CharacterHealth>();
        
        LastCheckpoint = new Vector3(PlayerPrefs.GetFloat("LastCheckpointX"), PlayerPrefs.GetFloat("LastCheckpointY"), PlayerPrefs.GetFloat("LastCheckpointZ"));
        Debug.Log($"LastCheckpoint: {LastCheckpoint}");
        if (SpawnFromCheckpoint)
        {
            m_spawnPosition = LastCheckpoint;
            transform.SetPositionAndRotation(m_spawnPosition, Quaternion.identity);
        }
        m_health.SetHealth(PlayerPrefs.GetInt("Health"));
    }
    
    public void SetSpawnPoint(Vector3 position)
    {
        LastCheckpoint = position;
        PlayerPrefs.SetFloat("LastCheckpointX", LastCheckpoint.x);
        PlayerPrefs.SetFloat("LastCheckpointY", LastCheckpoint.y);
        PlayerPrefs.SetFloat("LastCheckpointZ", LastCheckpoint.z);
        PlayerPrefs.Save();
    }
}

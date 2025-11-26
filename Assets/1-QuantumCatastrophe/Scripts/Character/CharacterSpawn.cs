using UnityEngine;

public class CharacterSpawn : MonoBehaviour
{
    [SerializeField] private bool SpawnFromCheckpoint = true;
    private Vector3 m_spawnPosition;
    public Vector3 LastCheckpoint { get; set; }
    
    private CharacterHealth m_health;
    
    private void Awake()
    {
        m_health = GetComponent<CharacterHealth>();
        
        LastCheckpoint = new Vector3(PlayerPrefs.GetFloat("LastCheckpointX"), PlayerPrefs.GetFloat("LastCheckpointY"), PlayerPrefs.GetFloat("LastCheckpointZ"));
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
        PlayerPrefs.SetFloat("LastCheckpointX", position.x);
        PlayerPrefs.SetFloat("LastCheckpointY", position.y);
        PlayerPrefs.SetFloat("LastCheckpointZ", position.z);
        PlayerPrefs.Save();
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "QC/Levels/Level Data")]
public class LevelData : ScriptableObject
{
    public string LevelName;
    public Sprite Thumbnail;
    public bool IsLocked;
    public Color ThumbnailTint;
}
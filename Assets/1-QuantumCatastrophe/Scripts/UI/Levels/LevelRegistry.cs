using UnityEngine;

[CreateAssetMenu(fileName = "LevelRegistry", menuName = "QC/Levels/Level Registry", order = 0)]
public class LevelRegistry : ScriptableObject
{
    public LevelData[] Levels;
}
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}

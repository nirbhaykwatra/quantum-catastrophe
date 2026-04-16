using TMPro;
using UnityEngine;

public class Loading : MonoBehaviour
{
    private float timer;
    private TextMeshProUGUI m_textMeshProUGUI;

    private int loadingPhase = 0;
    string[] loadingPhases = {"Loading", "Loading.", "Loading..", "Loading..."};
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.5f)
        {
            if (m_textMeshProUGUI != null)
            {
                m_textMeshProUGUI.text = loadingPhases[loadingPhase];
                loadingPhase = (loadingPhase + 1) % loadingPhases.Length;
                timer = 0;
            }
        }
    }
}

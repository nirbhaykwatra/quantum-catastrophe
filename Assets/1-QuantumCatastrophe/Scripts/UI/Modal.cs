using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Modal : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_text;

    public void SetText(string text)
    {
        m_text.text = text;
    }
}

using System;
using System.Collections;
using GameEvents;
using UnityEngine;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour
{
    [SerializeField] private float m_fadeTime = 1f;
    [SerializeField] private float m_fadeDuration = 2f;
    [SerializeField] private BoolEventAsset OnFadeInCompleted;
    [SerializeField] private BoolEventAsset OnFadeOutCompleted;
    [SerializeField] private BoolEventAsset OnRespawn;
    [SerializeField] private IntEventAsset OnDeath;
    private Image m_panel;
    
    private Color m_opaque = new Color(0f , 0f , 0f, 1f );
    private Color m_transparent = new Color(0f , 0f , 0f, 0f );
    
    private void Awake()
    {
        m_panel = GetComponent<Image>();
        m_panel.color = m_transparent;
    }

    private void OnEnable()
    {
        OnRespawn.AddListener(Fade);
    }
    
    private void OnDisable()
    {
        OnRespawn.RemoveListener(Fade);
    }
    
    public void Fade(bool death)
    {
        Debug.Log($"Fade in with death: {death}");
        StartCoroutine(AnimateFade(death));
    }

    private IEnumerator AnimateFade(bool death)
    {
        float timer = 0f;
        while (timer < m_fadeTime)
        {
            timer += Time.deltaTime;
            float alpha = timer / m_fadeTime;
            m_panel.color = Color.Lerp(m_transparent, m_opaque, alpha);
            yield return null;
        }
        OnFadeInCompleted.Invoke(death);
        m_panel.color = m_opaque;

        if (!death)
        {
            Debug.Log($"Fade out without death");
            yield return new WaitForSeconds(m_fadeDuration);
            float respawnTimer = 0f;
            while (respawnTimer < m_fadeTime)
            {
                respawnTimer += Time.deltaTime;
                float alpha = respawnTimer / m_fadeTime;
                m_panel.color = Color.Lerp(m_opaque, m_transparent, alpha);
                yield return null;
            }
            OnFadeOutCompleted.Invoke(death);
            m_panel.color = m_transparent;
        }
        else
        {
            Debug.Log($"Fade out with death");
            OnDeath.Invoke(0);
        }
    }
}

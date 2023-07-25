using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private EntityHealth m_Health;

    [SerializeField] private Canvas m_Canvas;
    [SerializeField] private Slider m_Slider;
    [SerializeField] private Image m_FillImage;

    [SerializeField] private Color m_Color1 = Color.green;
    [SerializeField] private float m_Threshold1 = 0.7f;

    [SerializeField] private Color m_Color2 = Color.yellow;
    [SerializeField] private float m_Threshold2 = 0.4f;

    [SerializeField] private Color m_Color3 = Color.red;

    private Action<int> m_OnHealthChangedAction;

    public float fillRatio
    {
        get => m_Slider.value;
        set => m_Slider.value = value;
    }

    private void Start()
    {
        float _ratio = (float)m_Health.health / m_Health.maxHealth;

        if (_ratio >= m_Threshold1)
            m_FillImage.color = m_Color1;
        else if (_ratio >= m_Threshold2)
            m_FillImage.color = m_Color2;
        else
            m_FillImage.color = m_Color3;

        m_OnHealthChangedAction = _current =>
        {
            float _ratio = (float)_current / m_Health.maxHealth;

            if (_ratio >= m_Threshold1)
                m_FillImage.color = m_Color1;
            else if (_ratio >= m_Threshold2)
                m_FillImage.color = m_Color2;
            else
                m_FillImage.color = m_Color3;
        };
        m_Health.onHealthChanged_OnClient += m_OnHealthChangedAction;
    }

    private void OnDestroy()
    {
        if (m_OnHealthChangedAction != null)
        {
            m_Health.onHealthChanged_OnClient -= m_OnHealthChangedAction;
            m_OnHealthChangedAction = null;
        }
    }

    private void OnEnable()
    {
        m_Canvas.enabled = true;
    }

    private void OnDisable()
    {
        m_Canvas.enabled = false;
    }
}
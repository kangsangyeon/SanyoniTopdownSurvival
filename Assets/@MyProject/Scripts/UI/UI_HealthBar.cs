using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private Canvas m_Canvas;
    [SerializeField] private Slider m_Slider;

    public float fillRatio
    {
        get => m_Slider.value;
        set => m_Slider.value = value;
    }

    private void OnEnable() => m_Canvas.enabled = true;
    private void OnDisable() => m_Canvas.enabled = false;
}
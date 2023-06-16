using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider m_Slider;

    public float fillRatio
    {
        get => m_Slider.value;
        set => m_Slider.value = value;
    }
}
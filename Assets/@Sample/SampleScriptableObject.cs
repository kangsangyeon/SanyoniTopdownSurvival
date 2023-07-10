using UnityEngine;

[CreateAssetMenu(menuName = "Sample/SampleScriptableObject")]
public class SampleScriptableObject : ScriptableObject
{
    public int publicInt;
    public string publicString;

    [SerializeField] private int m_PrivateSerializedInt;
    public int privateSerializedInt => m_PrivateSerializedInt;

    [SerializeField] private string m_PrivateSerializedString;
    public string privateSerializedString => m_PrivateSerializedString;
}
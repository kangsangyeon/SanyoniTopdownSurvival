using UnityEngine;

public class SampleClass
{
    public int publicInt;
    public string publicString;

    [SerializeField] private int m_PrivateSerializedInt;
    public int privateSerializedInt => m_PrivateSerializedInt;
    public void SetPrivateSerializedInt(int _value) => m_PrivateSerializedInt = _value;

    [SerializeField] private string m_PrivateSerializedString;
    public string privateSerializedString => m_PrivateSerializedString;
    public void SetPrivateSerializedString(string _value) => m_PrivateSerializedString = _value;
}
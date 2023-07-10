using FishNet.Object;
using UnityEngine;

public class Sample_SendVariousObjectThroughNetwork : NetworkBehaviour
{
    [SerializeField] private SampleScriptableObject m_SO;

    [Server]
    public void Server_SendMessageString()
    {
        ObserversRpc_ReceiveMessageString("hello world!");
    }

    [ObserversRpc]
    private void ObserversRpc_ReceiveMessageString(string _msg)
    {
        Debug.Log(_msg);
    }

    [Server]
    public void Server_SendMessageSO()
    {
        ObserversRpc_ReceiveMessageSO(m_SO);
    }

    [ObserversRpc]
    private void ObserversRpc_ReceiveMessageSO(SampleScriptableObject _so)
    {
        Debug.Log("message received!");

        Debug.Log(_so.publicInt);
        Debug.Log(_so.publicString);
        Debug.Log(_so.privateSerializedInt);
        Debug.Log(_so.privateSerializedString);
    }

    [Server]
    public void Server_SendMessageStruct()
    {
        var _sampleStruct = new SampleStruct() { publicInt = 123, publicString = "SampleStruct: public string" };
        _sampleStruct.SetPrivateSerializedInt(456);
        _sampleStruct.SetPrivateSerializedString("SampleStruct: private private string");
        ObserversRpc_ReceiveMessageStruct(_sampleStruct);
    }

    [ObserversRpc]
    private void ObserversRpc_ReceiveMessageStruct(SampleStruct _struct)
    {
        Debug.Log(_struct.publicInt);
        Debug.Log(_struct.publicString);
        Debug.Log(_struct.privateSerializedInt);
        Debug.Log(_struct.privateSerializedString);
    }

    [Server]
    public void Server_SendMessageClass()
    {
        var _sampleClass = new SampleClass() { publicInt = 123, publicString = "SampleClass: public string" };
        _sampleClass.SetPrivateSerializedInt(456);
        _sampleClass.SetPrivateSerializedString("SampleClass: private private string");
        ObserversRpc_ReceiveMessageClass(_sampleClass);
    }

    [ObserversRpc]
    private void ObserversRpc_ReceiveMessageClass(SampleClass _class)
    {
        Debug.Log(_class.publicInt);
        Debug.Log(_class.publicString);
        Debug.Log(_class.privateSerializedInt);
        Debug.Log(_class.privateSerializedString);
    }

    [Server]
    public void Server_SendMessageInterface()
    {
        var _sampleInterfaceImpl = new SampleInterfaceImpl() { intVal = 123, stringVal = "SampleInterfaceImpl string" };
        ObserversRpc_ReceiveMessageInterface(_sampleInterfaceImpl);
    }

    [ObserversRpc]
    private void ObserversRpc_ReceiveMessageInterface(SampleInterfaceImpl _impl)
    {
        Debug.Log(_impl.intVal);
        Debug.Log(_impl.stringVal);
    }
}
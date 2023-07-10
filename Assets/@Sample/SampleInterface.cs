public interface SampleInterface
{
    int intValue { get; }
    string stringValue { get; }
}

public class SampleInterfaceImpl : SampleInterface
{
    public int intVal;
    public string stringVal;

    public int intValue => intVal;
    public string stringValue => stringVal;
}
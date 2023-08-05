namespace CustomObsolete
{
    public interface ICustomObsoleteAttribute
    {
        string Message { get; }
        bool IsError { get; }
    }
}

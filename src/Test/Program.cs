using Test;

internal class Program
{
    [MyObsolete(IsError = true, Message = "MYOBS")]
    private static string Method()
    {
        return "s";
    }

    private static void Main(string[] args)
    {
        Console.WriteLine(Method());
    }
}
using Test;

internal class Program
{
    [MyObsolete]
    private static string Method()
    {
        return "s";
    }

    private static void Main(string[] args)
    {
        Console.WriteLine(Method());
    }
}
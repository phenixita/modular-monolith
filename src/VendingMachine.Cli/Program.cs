internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        var app = new CliApp();
        return app.RunAsync(args);
    }
}

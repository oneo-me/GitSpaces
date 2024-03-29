namespace GitSpaces.Logging;

public static class Log
{
    public static void Debug(string? message)
    {
        System.Diagnostics.Debug.WriteLine(message);
    }
}

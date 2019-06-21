using System;
using OriginalConsole = System.Console;

public static class Console
{
    internal static object threadLock = new object();

    public static string ReadPassword()
    {
        string pass = "";
        do
        {
            ConsoleKeyInfo key = OriginalConsole.ReadKey(true);
            // Backspace Should Not Work
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass += key.KeyChar;
                lock (threadLock)
                    OriginalConsole.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                    lock (threadLock)
                        OriginalConsole.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
        } while (true);
        lock (threadLock)
            OriginalConsole.WriteLine("");
        return pass;
    }

    public static void WriteLine(string msg = "")
    {
        lock (threadLock)
            OriginalConsole.WriteLine(msg);
    }

    public static string ReadLine() => OriginalConsole.ReadLine();

    public static string ReadOptionalLine(string defaultValue)
    {
        var value = OriginalConsole.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            EraseLine();
            WriteLine(defaultValue);
            return defaultValue;
        }

        return value;
    }

    public static ConsoleKeyInfo ReadKey() => OriginalConsole.ReadKey();

    public static ConsoleProgressBar ProgressBar(long total)
    {
        return new ConsoleProgressBar(total);
    }

    public static void EraseLine()
    {
        lock (threadLock)
        {
            OriginalConsole.SetCursorPosition(0, OriginalConsole.CursorTop - 1);
            int currentLineCursor = OriginalConsole.CursorTop;
            OriginalConsole.SetCursorPosition(0, OriginalConsole.CursorTop);
            OriginalConsole.Write(new string(' ', OriginalConsole.WindowWidth));
            OriginalConsole.SetCursorPosition(0, currentLineCursor);
        }
    }
}

using System;
using System.Threading.Tasks;
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

public class ConsoleProgressBar : IDisposable
{
    private readonly long total;
    private readonly Task task;
    private long progress = 0;
    //private int previousPercentage = 0;
    private int percentage = 0;
    private int location;
    DateTime startTime = DateTime.Now;
    private bool disposed;
    private long previousProgress;

    internal ConsoleProgressBar(long total)
    {
        this.total = total;
        location = OriginalConsole.CursorTop;
        lock (Console.threadLock)
            Console.WriteLine(string.Empty);

        task = Task.Factory.StartNew(() =>
        {
            while (!disposed)
            {
                Render();
                Task.Delay(500).Wait();
            }
            Render();
        });

    }

    public void Dispose()
    {
        disposed = true;
    }

    public void SetProgress(long progress)
    {
        this.progress = progress;
    }

    public void IncrementProgress(long progressIncrement = 1)
    {
        this.progress += progressIncrement;
    }

    private void Render()
    {
        lock (Console.threadLock)
        {
            OriginalConsole.CursorVisible = false;
            var currentLocation = OriginalConsole.CursorTop;
            OriginalConsole.SetCursorPosition(0, location);

            previousProgress = (int)Lerp(previousProgress, progress, 0.5f);
            percentage = (int)((previousProgress * 100) / total);

            if (total == progress)
            {
                previousProgress = progress;
                percentage = 100;
                if (!disposed)
                    Dispose();
            }

            //TimeSpan timeRemaining = TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks * (total - (progress + 1)) / (progress + 1));

            OriginalConsole.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * previousProgress; i++)
            {
                OriginalConsole.BackgroundColor = ConsoleColor.Green;
                OriginalConsole.CursorLeft = position++;
                OriginalConsole.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i < 31; i++)
            {
                OriginalConsole.BackgroundColor = ConsoleColor.DarkGray;
                OriginalConsole.CursorLeft = position++;
                OriginalConsole.Write(" ");
            }

            //draw totals
            OriginalConsole.CursorLeft = 35;
            OriginalConsole.BackgroundColor = ConsoleColor.Black;

            OriginalConsole.WriteLine($"{percentage}%");
            //+ $"  ({timeRemaining.Hours.ToString("00")}:{timeRemaining.Minutes.ToString("00")}:{timeRemaining.Seconds.ToString("00")})");


            OriginalConsole.SetCursorPosition(0, currentLocation);
            OriginalConsole.CursorVisible = true;
        }
    }

    private float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }
}

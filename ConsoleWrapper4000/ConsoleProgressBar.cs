using System;
using System.Threading.Tasks;
using OriginalConsole = System.Console;

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
                Render(false);
                Task.Delay(500).Wait();
            }
            Render(true);
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

    private void Render(bool finalRender)
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


            var timeTaken = DateTime.Now.Subtract(startTime);
            var timeRemaining = TimeSpan.FromTicks(
                (timeTaken.Ticks / (progress + 1)) * (total - progress + 1)
            );

            OriginalConsole.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * previousProgress; i++)
            {
                OriginalConsole.BackgroundColor = finalRender ? ConsoleColor.DarkGreen : ConsoleColor.Green;
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

            if (finalRender)
                OriginalConsole.WriteLine($"{percentage}%"
                    + $" ({timeTaken.Hours.ToString("00")}:{timeTaken.Minutes.ToString("00")}:{timeTaken.Seconds.ToString("00")})");
            else
                OriginalConsole.WriteLine($"{percentage}%"
                  + $" ({timeRemaining.Hours.ToString("00")}:{timeRemaining.Minutes.ToString("00")}:{timeRemaining.Seconds.ToString("00")})");

            OriginalConsole.SetCursorPosition(0, currentLocation);
            OriginalConsole.CursorVisible = true;
        }
    }

    private float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }
}

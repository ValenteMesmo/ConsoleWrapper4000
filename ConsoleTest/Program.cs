using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var progress = Console.ProgressBar(10))

                for (int i = 0; i < 10; i++)
                {
                    if (i == 9)
                        Thread.Sleep(30000);
                    else
                        Thread.Sleep(1000);
                    progress.IncrementProgress();
                }

            Console.ReadKey();
        }
    }
}

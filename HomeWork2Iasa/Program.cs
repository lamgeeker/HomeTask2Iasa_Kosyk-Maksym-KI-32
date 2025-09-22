/*static void Calc(int count)
{
    Random rnd = new Random();
    int inCycle = 0;
    for(int i = 0; i < count; i++)
    {
        double x = rnd.NextDouble();
        double y = rnd.NextDouble();
        if(x *x + y *y <= 1)
        {
            inCycle++;
        }

    }
     
    double pi = 4 * (inCycle * 1.0  / count);
    Console.WriteLine(pi);
}

DateTime t1 = DateTime.Now;
Calc(1000000);
DateTime t2 = DateTime.Now;
TimeSpan t3 = t2 - t1;
Console.WriteLine(t3.TotalMilliseconds + "ms");
*/

using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main()
    {
        int totalPoints = 1_000_000;
        int[] threadCounts = { 1, 2, 4, 8, 16, 32, 64 };

        foreach (int threads in threadCounts)
        {
            double pi = RunMonteCarlo(totalPoints, threads, out long elapsedMs);
            Console.WriteLine($"Threads: {threads}, Pi ≈ {pi}, Time = {elapsedMs} ms");
        }
    }

    static double RunMonteCarlo(int totalPoints, int numThreads, out long elapsedMs)
    {
        int pointsPerThread = totalPoints / numThreads;
        int[] hits = new int[numThreads];
        Thread[] workers = new Thread[numThreads];
        Random globalRand = new Random();

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < numThreads; i++)
        {
            int localIndex = i;
            int seed;

            lock (globalRand) 
                seed = globalRand.Next();

            workers[i] = new Thread(() =>
            {
                Random rnd = new Random(seed);
                int inside = 0;
                for (int j = 0; j < pointsPerThread; j++)
                {
                    double x = rnd.NextDouble();
                    double y = rnd.NextDouble();
                    if (x * x + y * y <= 1.0) inside++;
                }
                hits[localIndex] = inside;
            });
            workers[i].Start();
        }

        for (int i = 0; i < numThreads; i++)
            workers[i].Join();

        sw.Stop();
        elapsedMs = sw.ElapsedMilliseconds;

        int totalHits = 0;
        for (int i = 0; i < numThreads; i++)
            totalHits += hits[i];

        return 4.0 * totalHits / totalPoints;
    }
}




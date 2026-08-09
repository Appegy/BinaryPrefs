using System;
using System.Diagnostics;

namespace Appegy.Storage
{
    internal static class DurationProbe
    {
        private const int WarmupIterations = 10;
        private const int MeasuredIterations = 50;

        public static (double p50, double p99) MillisecondsPerCall(Action action)
        {
            for (var i = 0; i < WarmupIterations; i++)
            {
                action();
            }

            var samples = new double[MeasuredIterations];
            for (var i = 0; i < MeasuredIterations; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                samples[i] = stopwatch.Elapsed.TotalMilliseconds;
            }

            Array.Sort(samples);
            return (samples[MeasuredIterations / 2], samples[(int)(MeasuredIterations * 0.99)]);
        }
    }
}

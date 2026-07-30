using System;
using UnityEngine.Profiling;

namespace Appegy.Storage
{
    internal static class AllocationProbe
    {
        public const double AllowedBytesPerCall = 1d;

        private const int WarmupIterations = 10_000;
        private const int MeasuredIterations = 200_000;
        private const int Repeats = 3;

        public static double BytesPerCall(Action action)
        {
            for (var i = 0; i < WarmupIterations; i++)
            {
                action();
            }

            var highest = 0d;
            for (var repeat = 0; repeat < Repeats; repeat++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var heapBefore = Profiler.GetMonoUsedSizeLong();
                for (var i = 0; i < MeasuredIterations; i++)
                {
                    action();
                }
                var heapAfter = Profiler.GetMonoUsedSizeLong();

                highest = Math.Max(highest, (heapAfter - heapBefore) / (double)MeasuredIterations);
            }
            return highest;
        }
    }
}

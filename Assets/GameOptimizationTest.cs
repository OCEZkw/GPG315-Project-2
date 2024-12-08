using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OccaSoftware.PerformanceProfiler.Runtime;

public class GameOptimizationTest : MonoBehaviour
{
    public EnhancedProfiler profiler;

    void Start()
    {
        // Most comprehensive setup
        var benchmarkConfig = new EnhancedProfiler.BenchmarkConfig
        {
            benchmarkName = "Full Game Performance Test",
            benchmarkDuration = 30f, // 3-minute comprehensive test
            recordFrameTimestamps = true,
            generateDetailedReport = true
        };

        // Multiple custom metrics
        profiler.RegisterCustomMetric("FPS",
            () => 1.0f / Time.unscaledDeltaTime,
            45f,   // Warning if below 45 FPS
            30f    // Critical if below 30 FPS
        );

        profiler.RegisterCustomMetric("GPU Utilization",
            () => profiler.CalculateGPUComputeUtilization(),
            50f,   // Warning threshold
            75f    // Critical threshold
        );

        // Start the benchmark
        profiler.StartBenchmark(benchmarkConfig);
    }
}

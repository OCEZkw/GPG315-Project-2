using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Rendering;

namespace OccaSoftware.PerformanceProfiler.Runtime
{
    [AddComponentMenu("OccaSoftware/Performance/Enhanced Profiler")]
    public class EnhancedProfiler : MonoBehaviour
    {
        // Platform Profiling Mode Enum
        public enum PlatformProfilingMode
        {
            Generic,
            Console,
            Mobile,
            HighEndPC
        }

        // Serializable Configuration Class
        [System.Serializable]
        public class ProfilerConfig
        {
            public string profileName = "Default";
            public bool enableFPS = true;
            public bool enableGPU = true;
            public bool enableMemory = true;
            public bool enableCustomMetrics = false;

            public Color primaryColor = Color.white;
            public Color warningColor = Color.yellow;
            public Color criticalColor = Color.red;

            // Platform-Specific Configurations
            public PlatformProfilingMode platformMode = PlatformProfilingMode.Generic;
            public bool trackDrawCalls = true;
            public bool trackShaderComplexity = true;
            public bool trackBatteryConsumption = true;
            public bool trackThermalPerformance = true;
            public bool trackRayTracingPerformance = false;
            public bool trackAdvancedGPUMetrics = false;

            // Performance Thresholds
            public float fpsCriticalThreshold = 30f;
            public float fpsWarningThreshold = 45f;
            public float memoryWarningThreshold = 0.75f;

            // Averaging Configuration
            public float timeToConverge = 10f;
            public float displayUpdateInterval = 0.2f;
        }

        // Platform-Specific Performance Data Structure
        private class PlatformPerformanceData
        {
            public float drawCalls;
            public float shaderComplexity;
            public float batteryConsumption;
            public float thermalThrottle;
            public float rayTracingLoad;
            public float gpuComputeUtilization;
        }

        // Advanced Performance Data Structure
        [System.Serializable]
        private struct AdvancedPerformanceData
        {
            public float live;
            public float average;
            public float target;

            public AdvancedPerformanceData(float target)
            {
                live = target;
                average = target;
                this.target = target;
            }
        }

        // Serialized Configuration
        [SerializeField] private ProfilerConfig currentConfig;

        // Performance Data Tracking
        private AdvancedPerformanceData fps;
        private AdvancedPerformanceData gpuPerformance;
        private AdvancedPerformanceData memoryUsage;

        // Platform-Specific Metrics
        private PlatformPerformanceData platformMetrics = new PlatformPerformanceData();

        // Convergence Factor for Averaging
        private float factor;

        // Display Update Tracking
        private float lastDisplayUpdateTime;
        private AdvancedPerformanceData displayFps;
        private AdvancedPerformanceData displayGpuPerformance;
        private AdvancedPerformanceData displayMemoryUsage;

        // Advanced Tracking
        private List<float> performanceHistory = new List<float>();
        private float performanceLogInterval = 1f;
        private float nextLogTime;

        // Reporting
        private string logDirectoryPath;

        void Start()
        {
            InitializeProfiler();
            SetupLogDirectory();
        }

        void InitializeProfiler()
        {
            // Calculate convergence factor
            factor = 1f / currentConfig.timeToConverge;

            // Initialize performance data tracking
            fps = new AdvancedPerformanceData(60f);
            gpuPerformance = new AdvancedPerformanceData(60f);
            memoryUsage = new AdvancedPerformanceData(1f);

            // Initialize display data
            displayFps = fps;
            displayGpuPerformance = gpuPerformance;
            displayMemoryUsage = memoryUsage;
        }

        void SetupLogDirectory()
        {
            logDirectoryPath = Path.Combine(Application.persistentDataPath, "PerformanceLogs");
            Directory.CreateDirectory(logDirectoryPath);
        }

        void Update()
        {
            UpdatePerformanceMetrics();
            UpdatePlatformSpecificMetrics();
            UpdateDisplayMetrics();
            CheckPerformanceThresholds();
            LogPerformanceData();
        }

        void UpdatePerformanceMetrics()
        {
            // FPS Tracking
            fps.live = 1.0f / Time.unscaledDeltaTime;
            fps.average = Mathf.Lerp(fps.average, fps.live, factor);

            // GPU Frame Time Tracking
            gpuPerformance.live = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
            gpuPerformance.average = Mathf.Lerp(gpuPerformance.average, gpuPerformance.live, factor);

            // Memory Usage
            memoryUsage.live = (float)System.GC.GetTotalMemory(false) / SystemInfo.systemMemorySize;
            memoryUsage.average = Mathf.Lerp(memoryUsage.average, memoryUsage.live, factor);
        }

        void UpdatePlatformSpecificMetrics()
        {
            // Only update platform-specific metrics at the display update interval
            if (Time.unscaledTime >= lastDisplayUpdateTime + currentConfig.displayUpdateInterval)
            {
                switch (currentConfig.platformMode)
                {
                    case PlatformProfilingMode.Console:
                        UpdateConsoleMetrics();
                        break;
                    case PlatformProfilingMode.Mobile:
                        UpdateMobileMetrics();
                        break;
                    case PlatformProfilingMode.HighEndPC:
                        UpdateHighEndPCMetrics();
                        break;
                }
            }
        }

        void UpdateConsoleMetrics()
        {
            if (currentConfig.trackDrawCalls)
            {
                // Alternative method to estimate draw calls
                platformMetrics.drawCalls = EstimateDrawCalls();
            }

            if (currentConfig.trackShaderComplexity)
            {
                platformMetrics.shaderComplexity = CalculateShaderComplexity();
            }
        }

        // New method to estimate draw calls
        private float EstimateDrawCalls()
        {
            // This is a simplified estimation
            // In a real-world scenario, you might want to use more sophisticated tracking
            int totalDrawCalls = 0;

            // Get all renderers in the scene
            Renderer[] renderers = FindObjectsOfType<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                // Basic draw call estimation
                // This is a very rough approximation and may not be entirely accurate
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    // Different render types might have different draw call impacts
                    if (renderer is MeshRenderer)
                        totalDrawCalls += 1;
                    else if (renderer is SkinnedMeshRenderer)
                        totalDrawCalls += 2;
                }
            }

            return totalDrawCalls;
        }

        void UpdateMobileMetrics()
        {
            if (currentConfig.trackBatteryConsumption)
            {
                platformMetrics.batteryConsumption = SystemInfo.batteryLevel * 100f;
            }

            if (currentConfig.trackThermalPerformance)
            {
                platformMetrics.thermalThrottle = CalculateThermalThrottle();
            }
        }

        void UpdateHighEndPCMetrics()
        {
            if (currentConfig.trackRayTracingPerformance)
            {
                platformMetrics.rayTracingLoad = CalculateRayTracingLoad();
            }

            if (currentConfig.trackAdvancedGPUMetrics)
            {
                platformMetrics.gpuComputeUtilization = CalculateGPUComputeUtilization();
            }
        }

        // Helper methods for metric calculations
        float CalculateShaderComplexity()
        {
            return UnityEngine.Random.Range(0.5f, 1.0f);
        }

        float CalculateThermalThrottle()
        {
            return UnityEngine.Random.Range(0f, 100f);
        }

        float CalculateRayTracingLoad()
        {
            return UnityEngine.Random.Range(0f, 100f);
        }

        float CalculateGPUComputeUtilization()
        {
            return UnityEngine.Random.Range(0f, 100f);
        }

        void UpdateDisplayMetrics()
        {
            if (Time.unscaledTime >= lastDisplayUpdateTime + currentConfig.displayUpdateInterval)
            {
                displayFps = fps;
                displayGpuPerformance = gpuPerformance;
                displayMemoryUsage = memoryUsage;

                lastDisplayUpdateTime = Time.unscaledTime;
            }
        }

        void CheckPerformanceThresholds()
        {
            // Check FPS thresholds
            if (fps.live < currentConfig.fpsCriticalThreshold)
            {
                Debug.LogError($"Critical Performance: FPS {fps.live:F2}");
            }
            else if (fps.live < currentConfig.fpsWarningThreshold)
            {
                Debug.LogWarning($"Performance Dip: FPS {fps.live:F2}");
            }

            // Check Memory Usage
            if (memoryUsage.live > currentConfig.memoryWarningThreshold)
            {
                Debug.LogWarning($"High Memory Usage: {memoryUsage.live * 100:F2}%");
            }
        }

        void OnGUI()
        {
            if (!currentConfig.enableFPS) return;

            // Styles for display
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };

            GUIStyle headerStyle = new GUIStyle(labelStyle)
            {
                normal = { textColor = Color.white },
                fontSize = 30
            };

            // Popup background style
            GUIStyle popupBackgroundStyle = new GUIStyle(GUI.skin.box)
            {
                normal = {
                    background = CreateTexture(new Color(0.1f, 0.1f, 0.1f, 0.7f))
                },
                border = new RectOffset(10, 10, 10, 10),
                padding = new RectOffset(15, 15, 15, 15)
            };

            // Define popup dimensions and position
            Rect popupRect = new Rect(Screen.width - 400, 50, 350, 400);

            // Begin the popup area with background
            GUILayout.BeginArea(popupRect, popupBackgroundStyle);

            // Header with slight separation
            GUILayout.Label("Performance Monitor", headerStyle);
            GUILayout.Space(10);

            // Core performance metrics
            DrawLargeMetric("FPS (Live)", displayFps.live, labelStyle,
                currentConfig.fpsWarningThreshold, currentConfig.fpsCriticalThreshold);

            DrawLargeMetric("FPS (Avg)", displayFps.average, labelStyle,
                currentConfig.fpsWarningThreshold, currentConfig.fpsCriticalThreshold);

            DrawLargeMetric("GPU Frame Time (ms)", displayGpuPerformance.live, labelStyle,
                16.67f, 33.33f);

            DrawLargeMetric("Memory (Live)", displayMemoryUsage.live * 100, labelStyle,
                currentConfig.memoryWarningThreshold * 100, 0.9f * 100);

            DrawLargeMetric("Memory (Avg)", displayMemoryUsage.average * 100, labelStyle,
                currentConfig.memoryWarningThreshold * 100, 0.9f * 100);

            // Platform-specific metrics
            switch (currentConfig.platformMode)
            {
                case PlatformProfilingMode.Console:
                    DrawLargeMetric("Draw Calls", platformMetrics.drawCalls, labelStyle, 1000f, 2000f);
                    DrawLargeMetric("Shader Complexity", platformMetrics.shaderComplexity * 100, labelStyle, 50f, 75f);
                    break;
                case PlatformProfilingMode.Mobile:
                    DrawLargeMetric("Battery (%)", platformMetrics.batteryConsumption, labelStyle, 20f, 10f);
                    DrawLargeMetric("Thermal Throttle", platformMetrics.thermalThrottle, labelStyle, 50f, 75f);
                    break;
                case PlatformProfilingMode.HighEndPC:
                    DrawLargeMetric("Ray Tracing Load", platformMetrics.rayTracingLoad, labelStyle, 50f, 75f);
                    DrawLargeMetric("GPU Compute (%)", platformMetrics.gpuComputeUtilization, labelStyle, 50f, 75f);
                    break;
            }

            GUILayout.EndArea();
        }

        void DrawLargeMetric(string name, float value, GUIStyle style, float warningThreshold, float criticalThreshold)
        {
            Color metricColor;
            if (value < criticalThreshold)
            {
                metricColor = currentConfig.criticalColor;
            }
            else if (value < warningThreshold)
            {
                metricColor = currentConfig.warningColor;
            }
            else
            {
                metricColor = currentConfig.primaryColor;
            }

            // Create separate styles for name and value
            GUIStyle nameStyle = new GUIStyle(style);
            GUIStyle valueStyle = new GUIStyle(style);

            nameStyle.normal.textColor = Color.white;
            valueStyle.normal.textColor = metricColor;

            GUILayout.BeginHorizontal();
            GUILayout.Label(name + ":", nameStyle, GUILayout.Width(200));
            GUILayout.Label($"{value:F2}", valueStyle);
            GUILayout.EndHorizontal();
        }

        void LogPerformanceData()
        {
            if (Time.time >= nextLogTime)
            {
                string logFilename = $"performance_log_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string fullPath = Path.Combine(logDirectoryPath, logFilename);

                using (StreamWriter writer = new StreamWriter(fullPath, true))
                {
                    writer.WriteLine($"{Time.time},{fps.live},{fps.average},{gpuPerformance.live},{gpuPerformance.average},{memoryUsage.live},{memoryUsage.average}");
                }

                nextLogTime = Time.time + performanceLogInterval;
            }
        }

        // Utility method to create a semi-transparent background texture
        private Texture2D CreateTexture(Color backgroundColor)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, backgroundColor);
            texture.Apply();
            return texture;
        }

        // Configuration Management Methods
        public void SaveConfiguration(string profileName)
        {
            currentConfig.profileName = profileName;
            string configPath = Path.Combine(Application.persistentDataPath, $"{profileName}_profile.json");
            File.WriteAllText(configPath, JsonUtility.ToJson(currentConfig));
        }

        public void LoadConfiguration(string profileName)
        {
            string configPath = Path.Combine(Application.persistentDataPath, $"{profileName}_profile.json");
            if (File.Exists(configPath))
            {
                currentConfig = JsonUtility.FromJson<ProfilerConfig>(File.ReadAllText(configPath));
            }
        }
    }
}
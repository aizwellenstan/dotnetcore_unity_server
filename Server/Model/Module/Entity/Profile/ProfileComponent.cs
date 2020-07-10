using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ETHotfix;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Diagnostics;

namespace ETModel
{
    public class ProfileComponent : Component
    {
        private TimeSpan processStartTime;

        public double totalCpuUsage { get; private set; }

        public double lastCpuUsage { get; private set; }

        public long workSetMemoryUsage { get; private set; }

        public long privateWorkSetMemoryUsage { get; private set; }

        public int processId { get; private set; }

        private TimeSpan oldCPUTime = new TimeSpan(0);
        private DateTime lastMonitorTime = DateTime.UtcNow;
        private DateTime StartTime = DateTime.UtcNow;
        private Process process = Process.GetCurrentProcess();

        private bool isShowMessage = false;
        private DateTime lastShowMessageAt = DateTime.UtcNow;
        private int showFreqAtMilisec = 1000;

        private TimerComponent timerComponent;

        private float frameCount = 0;

        private double updateTimer = 0;

        ProfilerUtility.NetworkProfiler networkProfiler = new ProfilerUtility.NetworkProfiler();

        public void Awake()
        {
            processStartTime = process.TotalProcessorTime;
            processId = process.Id;

            timerComponent = Game.Scene.GetComponent<TimerComponent>();

            //ShowMessage(60000);
        }

        public void Update()
        {
            try
            {
                TimeSpan newCPUTime = process.TotalProcessorTime - processStartTime;
                lastCpuUsage = (newCPUTime - oldCPUTime).TotalSeconds / (Environment.ProcessorCount * DateTime.UtcNow.Subtract(lastMonitorTime).TotalSeconds);
                lastMonitorTime = DateTime.UtcNow;
                totalCpuUsage = newCPUTime.TotalSeconds / (Environment.ProcessorCount * DateTime.UtcNow.Subtract(StartTime).TotalSeconds);
                oldCPUTime = newCPUTime;
                workSetMemoryUsage = process.WorkingSet64;
                privateWorkSetMemoryUsage = process.PrivateMemorySize64;

                UpdateMessage();
            }
            catch (Exception e)
            {
                Log.Error($"Profile update failed! Reason:{e.Message}, TraceStack:{e.StackTrace}");
            }
        }

        public string GetCpuUsageLast()
        {
            return $"{lastCpuUsage * 100:0.0}";
        }

        public string GetCpuUsageTotal()
        {
            return $"{totalCpuUsage * 100:0.0}";
        }

        public string GetWorkSetMemoryUsage()
        {
            const int KB = 1024;
            const int MB = 1024 * 1024;

            if (workSetMemoryUsage < KB)
                return $"{workSetMemoryUsage:0.00} byte";
            if (workSetMemoryUsage < MB)
                return $"{workSetMemoryUsage / KB:0.00} KB";

            return $"{workSetMemoryUsage / MB:0.00} MB";
        }

        public string GetPrivateWorkSetMemoryUsage()
        {
            const int KB = 1024;
            const int MB = 1024 * 1024;

            if (privateWorkSetMemoryUsage < KB)
                return $"{privateWorkSetMemoryUsage:0.00} byte";
            if (privateWorkSetMemoryUsage < MB)
                return $"{privateWorkSetMemoryUsage / KB:0.00} KB";

            return $"{privateWorkSetMemoryUsage / MB:0.00} MB";
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();
        }

        public void ShowMessage(int milisec)
        {
            showFreqAtMilisec = milisec;
            isShowMessage = true;
        }

        public void HideMessage()
        {
            networkProfiler.Reset();
            isShowMessage = false;
        }

        public void UpdateMessage()
        {
            frameCount += 1.0f;
            updateTimer += timerComponent.deltaTime;

            if (!isShowMessage)
                return;
            var now = DateTime.UtcNow;
            if (now.Subtract(lastShowMessageAt).TotalMilliseconds >= showFreqAtMilisec)
            {
                var fps = (int)(frameCount / updateTimer);
                frameCount = 0;
                updateTimer = 0;
                string networkInfo = networkProfiler.Show(fps);
                string showMsg = $"CpuUsageLast:{GetCpuUsageLast()}%, CpuUsageTotal:{GetCpuUsageTotal()}%, WorkSetMemoryUsage:{GetWorkSetMemoryUsage()}, NetworkInfo:{networkInfo}";
                //Console.WriteLine(showMsg);
                Log.Trace(showMsg);
                lastShowMessageAt = DateTime.UtcNow;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using Lab_5.Properties;

namespace Lab_5.Models
{
    public class ProcessWrapper : INotifyPropertyChanged
    {
        #region Fields

        private static readonly object Lock = new object();

        private static readonly Dictionary<int, ProcessData> ProcessCache = new Dictionary<int, ProcessData>();

        private static readonly ulong Ram = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;

        #endregion

        #region Props

        public string Name { get; private set; }
        public int Pid { get; private set; }

        public bool IsActive { get; private set; }

        public float CpuPercent { get; private set; }

        public double RamPercent { get; private set; }

        public int RamVolume { get; private set; }

        public long NumThreads { get; set; }

        public string UserName { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public DateTime LaunchTime { get; private set; }

        #endregion

        #region Ctor

        private ProcessWrapper()
        {
        }

        internal static async Task<ProcessWrapper> ConstructProcessWrapper(Process p)
        {
            try
            {
                //TODO: do all job with processes in parallel 
                ProcessWrapper instance = ProcessCache.ContainsKey(p.Id)
                    ? UseCache(p.Id)
                    : BuildNew(p);
                
                Stopwatch sw = Stopwatch.StartNew();
                await instance.SetMutableData(p);
                records.Add(sw.ElapsedMilliseconds);
                return instance;
            }
            catch (Win32Exception)
            {
                return null;
            }
        }

        private static ProcessWrapper BuildNew(Process p)
        {
            ProcessWrapper res = new ProcessWrapper();
            res.Pid = p.Id;
            res.Name = p.ProcessName;
            res.IsActive = p.Responding;
            res.UserName = GetUserName(res.Pid);
            res.FilePath = p.MainModule.FileName;
            res.FileName = res.FilePath.Substring(res.FilePath.LastIndexOf('\\') + 1);
            lock (Lock)
            {
                ProcessCache.Add(res.Pid,
                    new ProcessData(res.Name, res.IsActive, res.UserName, res.FilePath, res.FileName));
            }
            return res;
        }

        static List<long> records = new List<long>();
        private static ProcessWrapper UseCache(int pId)
        {
            //Stopwatch sw = Stopwatch.StartNew();
            ProcessWrapper res = new ProcessWrapper();
            ProcessData pd = ProcessCache[pId];
            res.Pid = pId;
            res.Name = pd.Name;
            res.IsActive = pd.IsActive;
            res.FileName = pd.FileName;
            res.FilePath = pd.FilePath;
            res.UserName = pd.UserName;
            //records.Add(sw.ElapsedMilliseconds);
            return res;
        }

        private async Task SetMutableData(Process p)
        {
            CpuPercent = await CalcCpuPercentAsync(p.ProcessName);
            LaunchTime = p.StartTime;
            NumThreads = p.Threads.Count;
            long usedBytes = p.WorkingSet64;
            RamVolume = Convert.ToInt32(usedBytes / 1e6);
            RamPercent = CalcRamPercent(usedBytes);
        }

        #endregion

        #region HelperMethods

        private static async Task<float> CalcCpuPercentAsync(string processName)
        {
            PerformanceCounter pc = new PerformanceCounter("Process", "% Processor Time", processName, true);
            pc.NextValue();
            await Task.Delay(500);
            return pc.NextValue();
        }

        private static double CalcRamPercent(long ramUsage)
        {
            double precisePercent = Convert.ToDouble(ramUsage * 100) / Ram;
            return Convert.ToInt32(precisePercent * 10) / 10.0;
        }

        private static string GetUserName(int processId)
        {
            ObjectQuery objQuery =
                new ObjectQuery("Select * From Win32_Process where ProcessId='" + processId + "'");

            ManagementObjectSearcher mos = new ManagementObjectSearcher(objQuery);
            string processOwner = null;

            foreach (ManagementObject mo in mos.Get())
            {
                string[] s = new string[2];

                mo.InvokeMethod("GetOwner", s);

                processOwner = s[0];
                break;
            }

            return processOwner;
        }

        #endregion

        // public override bool Equals(object obj)
        // {
        //     if (obj is ProcessWrapper wrapper)
        //         return Pid == wrapper.Pid;
        //     return false;
        // }

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
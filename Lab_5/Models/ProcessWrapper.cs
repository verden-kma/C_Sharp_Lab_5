using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Lab_5.Properties;

namespace Lab_5.Models
{

        public class ProcessWrapper : INotifyPropertyChanged
        {
            #region Fields

            private bool _isActive;
            private double _cpuPercentage;
            private double _ramPercentage;
            private uint _ramVolume;
            private uint _nThreads;

            #endregion

            #region Props
            
            internal string Name { get; }
            internal uint Pid { get; }

            internal bool IsActive
            {
                get => _isActive;
                set
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }

            internal double CpuPercent
            {
                get => _cpuPercentage;
                set
                {
                    _cpuPercentage = value;
                    OnPropertyChanged();
                }
            }

            internal double RamPercent
            {
                get => _ramPercentage;
                set
                {
                    _ramPercentage = value;
                    OnPropertyChanged();
                }
            }

            internal uint RamVolume
            {
                get => _ramVolume;
                set
                {
                    _ramVolume = value;
                    OnPropertyChanged();
                }
            }

            internal uint NumThreads
            {
                get => _nThreads;
                set
                {
                    _nThreads = value;
                    OnPropertyChanged();
                }
            }

            internal string UserName { get; }
            internal string FileName { get; }
            internal string FilePath { get; }
            internal DateTime LaunchTime { get; }
        
            #endregion

            #region Ctor

            public ProcessWrapper(string name, uint pid, bool isActive, double cpuPercent, double ramPercent, 
                uint ramVolume, uint numThreads, string userName, string fileName, string filePath, DateTime launchTime)
            {
                Name = name;
                Pid = pid;
                IsActive = isActive;
                CpuPercent = cpuPercent;
                RamPercent = ramPercent;
                RamVolume = ramVolume;
                NumThreads = numThreads;
                UserName = userName;
                FileName = fileName;
                FilePath = filePath;
                LaunchTime = launchTime;
            }

            #endregion


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
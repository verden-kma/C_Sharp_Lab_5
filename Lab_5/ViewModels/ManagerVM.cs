using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Lab_5.Models;
using Lab_5.Tools.MVVM;

namespace Lab_5.ViewModels
{
    public class ManagerVM
    {
        #region Fields

        private bool _sortIsSet;
        private string _sortTarget;
        private static readonly object Lock = new object();

        #endregion

        #region Props

        public ProcessWrapper SelectedProcess { get; set; }

        #endregion

        #region Commands

        private RelayCommand<object> _modulesCommand;
        private RelayCommand<object> _threadsCommand;
        private RelayCommand<object> _terminateCommand;
        private RelayCommand<object> _folderCommand;
        private RelayCommand<object> _sortCommand;

        #region ModuleCmd

        public RelayCommand<object> ModulesCommand
        {
            get
            {
                return _modulesCommand ??
                       (_modulesCommand = new RelayCommand<object>(ModulesCmdImpl, o => CanListModules()));
            }
        }

        private void ModulesCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanListModules()
        {
            return SelectedProcess != null;
        }

        #endregion

        #region ThreadCmd

        public RelayCommand<object> ThreadsCommand
        {
            get
            {
                return _threadsCommand ??
                       (_threadsCommand = new RelayCommand<object>(ThreadsCmdImpl, o => CanListThreads()));
            }
        }

        private void ThreadsCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanListThreads()
        {
            return SelectedProcess != null;
        }

        #endregion

        #region TerninateCmd

        public RelayCommand<object> TerminateCommand
        {
            get
            {
                return _terminateCommand ??
                       (_terminateCommand = new RelayCommand<object>(TerminateCmdImpl, o => CanTerminate()));
            }
        }

        private void TerminateCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanTerminate()
        {
            return SelectedProcess != null;
        }

        #endregion

        #region FolderCmd

        public RelayCommand<object> FolderCommand
        {
            get
            {
                return _folderCommand ??
                       (_folderCommand = new RelayCommand<object>(FolderCmdImpl, o => CanOpenFolder()));
            }
        }

        private void FolderCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanOpenFolder()
        {
            return SelectedProcess != null;
        }

        #endregion

        #region SortCmd

        public RelayCommand<object> SortCommand
        {
            get { return _sortCommand ?? (_sortCommand = new RelayCommand<object>(SortCmdImpl, o => CanSort())); }
        }

        private void SortCmdImpl(object obj)
        {
            _sortIsSet = true;
            _sortTarget = obj.ToString();
        }

        private bool CanSort()
        {
            return ProcessesData.Count != 0;
        }

        #endregion

        #endregion

        public ObservableCollection<ProcessWrapper> ProcessesData { get; }

        internal ManagerVM()
        {
            ProcessesData = new ObservableCollection<ProcessWrapper>();
            BindingOperations.EnableCollectionSynchronization(ProcessesData, Lock);
        }

        internal void LaunchRefresh()
        {
            new Task(DelegateLogic).Start();
        }

        private async void DelegateLogic()
        {
            while (true)
            {
                Stopwatch watch = Stopwatch.StartNew();

                Process[] processes = Process.GetProcesses();
                List<Task<ProcessWrapper>> futuresList = processes.Select(ProcessWrapper.ConstructProcessWrapper).ToList();
                ProcessWrapper[] updatedProcesses = await Task.WhenAll(futuresList);
                updatedProcesses = (from process in updatedProcesses where process != null select process).ToArray();

                if (_sortIsSet) Sort(ref updatedProcesses, _sortTarget);
                
                int processId = -1;
                if (SelectedProcess != null)
                {
                    lock (Lock)
                    {
                        processId = SelectedProcess.Pid;
                    }
                }
                
                ProcessesData.Clear();

                foreach (ProcessWrapper pw in updatedProcesses)
                {
                    if (SelectedProcess != null && pw.Pid == processId)
                    {
                        SelectedProcess = pw;
                        ProcessesData.Add(SelectedProcess);
                    }
                    else ProcessesData.Add(pw);
                }

                MessageBox.Show($"Complete.\nTotal: {watch.ElapsedMilliseconds}");
            }
        }

        private static void Sort(ref ProcessWrapper[] wrappers, string target)
        {
            wrappers = (from wrapper in wrappers
                orderby wrapper.GetType().GetProperty(target)
                    .GetValue(wrapper, null)
                select wrapper).ToArray();
        }

        // internal void launchUpdates()
        // {
        //     Refresh();
        // }
        //
        // private async void Refresh()
        // {
        //     while (true)
        //     {
        //         List<ProcessWrapper> refreshed = await new Task<List<ProcessWrapper>>(DelegateLogic);
        //         ProcessesData.Clear();
        //         foreach (var pw in refreshed)
        //         {
        //             refreshed.Add(pw);
        //         }
        //     }
        // }
        //
        // private List<ProcessWrapper> DelegateLogic()
        // {
        //     List<ProcessWrapper> updatedProcesses = new List<ProcessWrapper>(); 
        //     Process[] processes = Process.GetProcesses();
        //     foreach (Process p in processes)
        //     {
        //         updatedProcesses.Add(new ProcessWrapper(p).Update());
        //     }
        //     if (_sortIsSet) sort(updatedProcesses, _sortTarget);
        //     return updatedProcesses;
        // }
    }
}
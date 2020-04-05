using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Lab_5.Models;
using Lab_5.Properties;
using Lab_5.Tools.MVVM;
using Lab_5.Views;

namespace Lab_5.ViewModels
{
    public class ManagerVM : INotifyPropertyChanged
    {
        #region Fields

        private bool _sortIsSet;
        private string _sortTarget;
        private static readonly object Lock = new object();

        private readonly HashSet<int> _deniedProcesses = new HashSet<int>();

        private delegate void AddPw(ProcessWrapper future);

        private delegate void VoidAction();

        #endregion

        #region Props

        private ProcessWrapper _selectedProcess;

        public ProcessWrapper SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

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
            try
            {
                ProcessModuleCollection pmc = Process.GetProcessById(SelectedProcess.Pid).Modules;
                StringBuilder sb = new StringBuilder();
                foreach (ProcessModule module in pmc)
                {
                    sb.Append("Module Name: ").Append(module.ModuleName).AppendLine();
                    sb.Append("Module Path: ").Append(module.FileName).AppendLine().AppendLine();
                }

                Task.Run(() => FlexibleMessageBox.Show(sb.ToString()));
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to show modules info.");
            }
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
            try
            {
                ProcessThreadCollection ptc = Process.GetProcessById(SelectedProcess.Pid).Threads;
                StringBuilder sb = new StringBuilder();
                foreach (ProcessThread thread in ptc)
                {
                    sb.Append("ID: ").Append(Convert.ToString(thread.Id)).AppendLine();
                    sb.Append("State: ").Append(thread.ThreadState).AppendLine();
                    sb.Append("Start Time: ").Append(thread.StartTime).AppendLine().AppendLine();
                }

                Task.Run(() => FlexibleMessageBox.Show(sb.ToString()));
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to show threads data.");
            }
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
            try
            {
                Process.GetProcessById(SelectedProcess.Pid).Kill();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to terminate this process.");
            }
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
            try
            {
                Process.Start("explorer", @SelectedProcess.FilePath);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open folder.\n");
            }
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
        }

        internal void LaunchRefresh()
        {
            new Task(DelegateLogic).Start();
        }

        private async void DelegateLogic()
        {
            bool firstRun = true;
            while (true)
            {
                List<Process> processes =
                    (from p in Process.GetProcesses() where !_deniedProcesses.Contains(p.Id) select p).ToList();
                List<Task<ProcessWrapper>> futuresList =
                    processes.Select(ProcessWrapper.ConstructProcessWrapper).ToList();
                if (firstRun)
                {
                    firstRun = false;
                    await ProcessFirstRun(futuresList);
                    processes.ForEach(process => process.Dispose());
                    continue;
                }

                HashSet<ProcessWrapper> updatedProcesses = new HashSet<ProcessWrapper>(await Task.WhenAll(futuresList));
                processes.ForEach(process => process.Dispose());

                foreach (ProcessWrapper pw in updatedProcesses.ToArray())
                {
                    if (pw.Name != null) continue;
                    if (pw.Pid != -1)
                        _deniedProcesses.Add(pw.Pid);
                    updatedProcesses.Remove(pw);
                }

                if (_sortIsSet) Sort(ref updatedProcesses, _sortTarget);

                UpdateProcessWrapper(updatedProcesses.ToList());
                await Task.Delay(100);
            }
        }

        private void UpdateProcessWrapper(IReadOnlyList<ProcessWrapper> pws)
        {
            // `selected` is buggy 
            int processId = -1;
            {
                lock (Lock)
                {
                    if (SelectedProcess != null)
                        processId = SelectedProcess.Pid;
                }
            }

            Application.Current.Dispatcher.BeginInvoke(new VoidAction(ProcessesData.Clear), DispatcherPriority.Render);

            DispatcherOperation[] operations = new DispatcherOperation[pws.Count];
            for (int i = 0; i < pws.Count; i++)
            {
                operations[i] = Application.Current.Dispatcher.BeginInvoke(new AddPw(ProcessesData.Add),
                    DispatcherPriority.Render, pws[i]);
                if (processId != -1 && pws[i].Pid == processId)
                {
                    int copyOfValue = i;
                    Application.Current.Dispatcher.BeginInvoke(new VoidAction(() =>
                        {
                            //SelectedProcess = pws[copyOfValue];
                            SelectedProcess = ProcessesData.First(p => p.Equals(pws[copyOfValue]));
                        }),
                        DispatcherPriority.Render);
                }
            }

            //MessageBox.Show("Selected: \n" + SelectedProcess);
        }

        private async Task ProcessFirstRun(ICollection<Task<ProcessWrapper>> futures)
        {
            while (futures.Count > 0)
            {
                Task<ProcessWrapper> nextPw = await Task.WhenAny(futures);
                futures.Remove(nextPw);
                ProcessWrapper sample = await nextPw;
                if (sample.Name == null) _deniedProcesses.Add(sample.Pid);
                else
                    Application.Current.Dispatcher.BeginInvoke(new AddPw(ProcessesData.Add), DispatcherPriority.Render,
                        sample);
            }
        }


        private static void Sort(ref HashSet<ProcessWrapper> wrappers, string target)
        {
            wrappers = (from wrapper in wrappers
                orderby wrapper.GetType().GetProperty(target)
                    .GetValue(wrapper, null)
                select wrapper).ToHashSet();
        }

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
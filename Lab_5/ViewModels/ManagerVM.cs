using System;
using System.Collections.ObjectModel;
using Lab_5.Models;
using Lab_5.Tools.MVVM;

namespace Lab_5.ViewModels
{
    public class ManagerVM
    {
        #region Props

        public ProcessWrapper CurrentProcess { get; set; }

        #endregion
        
        #region Commands

        private RelayCommand<object> _modulesCommand;
        private RelayCommand<object> _threadsCommand;
        private RelayCommand<object> _terminateCommand;
        private RelayCommand<object> _folderCommand;
        
        #region ModuleCmd

        public RelayCommand<object> ModulesCommand
        {
            get { return _modulesCommand ?? (_modulesCommand = new RelayCommand<object>(ModulesCmdImpl, o => CanListModules())); }
        }

        private void ModulesCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanListModules()
        {
            return CurrentProcess != null;
        }

        #endregion
        
        #region ThreadCmd

        public RelayCommand<object> ThreadsCommand
        {
            get { return _threadsCommand ?? (_threadsCommand = new RelayCommand<object>(ThreadsCmdImpl, o => CanListThreads())); }
        }

        private void ThreadsCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanListThreads()
        {
            return CurrentProcess != null;
        }

        #endregion
        
        #region TerninateCmd

        public RelayCommand<object> TerminateCommand
        {
            get { return _terminateCommand ?? (_terminateCommand = new RelayCommand<object>(TerminateCmdImpl, o => CanTerminate())); }
        }

        private void TerminateCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanTerminate()
        {
            return CurrentProcess != null;
        }

        #endregion
        
        #region FolderCmd

        public RelayCommand<object> FolderCommand
        {
            get { return _folderCommand ?? (_folderCommand = new RelayCommand<object>(FolderCmdImpl, o => CanOpenFolder())); }
        }

        private void FolderCmdImpl(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanOpenFolder()
        {
            return CurrentProcess != null;
        }

        #endregion

        #endregion
        public ObservableCollection<ProcessWrapper> ProcessesData { get; }

        internal ManagerVM()
        {
            ProcessesData = new ObservableCollection<ProcessWrapper>();
        }
    }
}
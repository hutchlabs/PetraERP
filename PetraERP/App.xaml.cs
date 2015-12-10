using PetraERP.Shared.Models;
using PetraERP.Shared.Utility;
using PetraERP.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PetraERP
{
    public partial class App : Application
    {
        #region Private Members

        private bool _saveOnExit = false;

        private ShellViewModel _viewModel;

        #endregion

        #region Public Properties

        public bool SaveOnExit
        {
            get { return _saveOnExit; }
            set { _saveOnExit = value; }
        }

        #endregion

        #region Constructor
        
        static App()
        {
        }

        #endregion

        #region Public Methods

        public void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogUtil.LogError("App", "AppDispatcherUnhandledException", e.Exception);
            e.Handled = true;
        }

        #endregion

        #region Overrides

        protected override void OnStartup(StartupEventArgs e)
        {
            LogUtil.LogInfo("App", "OnStartup", "Starting up.");
            
            base.OnStartup(e);
            
            var window = new Shell();

            _viewModel = new ShellViewModel();
            
            window.DataContext = _viewModel;
            
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (null != _viewModel)
                _viewModel.Dispose();

            if (_saveOnExit)
            {
                try
                {
                    Users.GetCurrentUser().logged_in = false;
                    Users.GetCurrentUser().last_login = DateTime.Now;
                    Users.GetCurrentUser().updated_at = DateTime.Now;
                    //Database.ERP.;
                }
                catch (Exception ex)
                {
                    LogUtil.LogError("App", "OnExit", ex);
                }
            }

            LogUtil.LogInfo("App", "OnExit", "Application closing.");
        }

        private void CloseWithoutSaving()
        {
            _saveOnExit = false;
            Application.Current.Shutdown();
        }

        #endregion
    }
}

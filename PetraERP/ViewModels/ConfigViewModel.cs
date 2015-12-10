using MahApps.Metro.Controls.Dialogs;
using PetraERP.ViewModels;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.MessagingService;
using PetraERP.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PetraERP.Shared;
using MahApps.Metro.Controls;

namespace PetraERP.ViewModels
{
    public class ConfigViewModel : WorkspaceViewModelBase
    {
        #region Private Members 

        private string _server;
        private bool _spinner = false;
        private event EventHandler<PetraERP.Shared.PetraEventArgs.DBConnectedEventArgs> _dbConnected;

        #endregion

        #region Public Members

        public IEnumerable<ComboBoxPairs> Servers
        {
            private set { ; }
            get { return Database.GetSQLServers(); }
        }

        public string Server
        {
            get { return _server; }
            set
            {
                if (_server == value)
                    return;
                _server = value;
                OnPropertyChanged(GetPropertyName(() => Server));
            }
        }

        public bool Spinner
        {
            get { return _spinner; }
            set
            {
                if (_spinner == value)
                    return;
                _spinner = value;
                OnPropertyChanged(GetPropertyName(() => Spinner));
            }
        }

        #endregion

        #region Events

        public event EventHandler<PetraERP.Shared.PetraEventArgs.DBConnectedEventArgs> DBConnected
        {
            add { _dbConnected += value; }
            remove { _dbConnected -= value; }
        }

        #endregion

        #region Commands

        public ICommand TryConnectCommand { get; private set; }

        #endregion

        #region Constructor

        public ConfigViewModel() : base("Config")
        {
            //Server = "ELMINA\\SQLEXPRESS";
            TryConnectCommand = new RelayCommand(TryConnect, CanTryConnect);
        }

        #endregion

        #region Public Methods

        public void ShowError(string error)
        {
            if (error != "")
            {
                if (this.IsInitialized)
                    AppData.MessageService.ShowMessage("An error occurred. More Info: " + error);
            }
        }

        #endregion

        #region Private Helpers

        private void TryConnect()
        {
            Spinner = true;

            try
            {
                if (Database.Setup(Server))
                {
                    Spinner = false;

                    if (_dbConnected != null)
                    {
                        _dbConnected(this, new PetraERP.Shared.PetraEventArgs.DBConnectedEventArgs(true));
                    }
                    else
                    {
                        LogUtil.LogInfo("ConfigViewModel", "TryConnect", string.Format("Failed db connection attempted for server: {0}.", Server));
                        AppData.MessageService.ShowMessage("Connection Error");
                    }
                }
            }
            catch (Exceptions.DBConnectionException ex)
            {
                Spinner = false;
                AppData.MessageService.ShowMessage("Error connecting. "+ex.Message);
            }
            catch (Exception ex)
            {
                Spinner = false;
                AppData.MessageService.ShowMessage("Database setup error: " + ex.GetBaseException().ToString(), DialogType.Error);
            }                 
        }

        private bool CanTryConnect()
        {
            return !string.IsNullOrEmpty(Server);
        }

        #endregion
    }
}
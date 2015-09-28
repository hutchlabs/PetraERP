using PetraERP.Shared.UI;
using PetraERP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI.MessagingService;
using PetraERP.Shared;
using PetraERP.Shared.Utility;
using System.Windows;
using System.Threading;

namespace PetraERP.ViewModels
{
    public class ShellViewModel : ViewModelBase, IFlyoutContainer
    {     
        #region Private Members

        private bool _isUserLoggedIn;
        private bool _isDBConnected;

        private string _userNameRole = "";
        private string _notificationToolTip = "Notifications";
        private string _notificationText = "Notifications";

        private readonly LoginViewModel _loginView;
        private readonly ConfigViewModel _configView;
        private WorkspaceViewModelBase _selectedView;
        private ObservableCollection<FlyoutViewModelBase> _flyouts;

        #endregion

        #region Public Members

        public WorkspaceViewModelBase SelectedView
        {
            get { return _selectedView; }
            set
            {
                if (_selectedView == value)
                    return;
                _selectedView = value;
                OnPropertyChanged(GetPropertyName(() => SelectedView));
            }
        }

        public UserSettingsViewModel SettingsView { get; private set; }

        public NotificationsViewModel NotificationsView { get; private set; }

        public bool IsUserLoggedIn
        {
            get { return _isUserLoggedIn; }
            set
            {
                if (_isUserLoggedIn == value)
                    return;
                _isUserLoggedIn = value;
                OnPropertyChanged(GetPropertyName(() => IsUserLoggedIn));
            }
        }

        public bool IsDBConnected
        {
            get { return _isDBConnected; }
            set
            {
                if (_isDBConnected == value)
                    return;
                _isDBConnected = value;
                OnPropertyChanged(GetPropertyName(() => IsDBConnected));
            }
        }

        public string UserNameRole
        {
            get { return _userNameRole; }
            set
            {
                if (_userNameRole == value)
                    return;
                _userNameRole = value;
                OnPropertyChanged(GetPropertyName(() => UserNameRole));
            }
        }

        public string NoteText
        {
            get { return _notificationText; }
            set
            {
                if (_notificationText == value)
                    return;
                _notificationText = value;
                OnPropertyChanged(GetPropertyName(() => NoteText));
            }
        }

        public string NoteToolTip
        {
            get { return _notificationToolTip; }
            set
            {
                if (_notificationToolTip == value)
                    return;
                _notificationToolTip = value;
                OnPropertyChanged(GetPropertyName(() => NoteToolTip));
            }
        }

        public ObservableCollection<FlyoutViewModelBase> Flyouts
        {
            get { return _flyouts; }
            set
            {
                if (value == _flyouts)
                    return;
                _flyouts = value;
                OnPropertyChanged(GetPropertyName(() => Flyouts));
            }
        }

        #endregion

        #region Commands

        public ICommand ShowSettingsCommand { get; private set; }
        public ICommand ShowNotificationsCommand { get; private set; }

        #endregion

        #region Constructor 

        public ShellViewModel()
        {
            AppData.MessageService = MessageServiceFactory.GetMessagingServiceInstance();

            _loginView = new LoginViewModel();
            _configView = new ConfigViewModel();

            string err = Database.Initialize();

            if (err=="true")
            {
                ShowLoginView();
            }
            else 
            {
                LogUtil.LogInfo("ShellViewModel", "Constructor", err);
                ShowConfigView();
                _configView.ShowError(err);
            }
        }

        #endregion

        #region Overriden methods

        protected override void OnInitialize()
        {
            Flyouts = new ObservableCollection<FlyoutViewModelBase>();
            _loginView.UserLoggedIn += LoginViewUserLoggedIn;
            _configView.DBConnected += ConfigViewDBConnected;
            AppData.MessageService.RegisterFlyout(SettingsView);
            AppData.MessageService.RegisterFlyout(NotificationsView);
        }

        protected override void OnDispose()
        {
            Flyouts.Clear();

            if (null != _loginView)
            {
                _loginView.UserLoggedIn -= LoginViewUserLoggedIn;
                _loginView.Dispose();
            }

            if (null != _configView)
            {
                _configView.DBConnected -= ConfigViewDBConnected;
                _configView.Dispose();
            }

            if (null != SelectedView)
            {
                SelectedView.Dispose();
            }
        }

        #endregion

        #region Event Handlers

        private void ShowLoginView()
        {
            SelectedView = _loginView;

            SettingsView = new UserSettingsViewModel()
            {
                Position = VisibilityPosition.Right,
                Theme = FlyoutTheme.AccentedTheme,
                Header = "Settings"
            };

            NotificationsView = new NotificationsViewModel()
            {
                Position = VisibilityPosition.Right,
                Theme = FlyoutTheme.AccentedTheme,
                Header = "Notifications"
            };

            IsUserLoggedIn = false;

            ShowSettingsCommand = new RelayCommand(() => SettingsView.IsOpen = !SettingsView.IsOpen, () => IsUserLoggedIn);
            ShowNotificationsCommand = new RelayCommand(() => NotificationsView.IsOpen = !NotificationsView.IsOpen, () => IsUserLoggedIn);
        }

        private void ShowConfigView(string error="")
        {
            SelectedView = _configView;

            SettingsView = new UserSettingsViewModel()
            {
                Position = VisibilityPosition.Right,
                Theme = FlyoutTheme.AccentedTheme,
                Header = "Settings"
            };

            NotificationsView = new NotificationsViewModel()
            {
                Position = VisibilityPosition.Right,
                Theme = FlyoutTheme.AccentedTheme,
                Header = "Notifications"
            };

            IsUserLoggedIn = false;
            ShowSettingsCommand = new RelayCommand(() => SettingsView.IsOpen = !SettingsView.IsOpen, () => IsUserLoggedIn);
            ShowNotificationsCommand = new RelayCommand(() => NotificationsView.IsOpen = !NotificationsView.IsOpen, () => IsUserLoggedIn);
        }

        private void LoginViewUserLoggedIn(object sender, PetraERP.Shared.PetraEventArgs.UserLoggedInEventArgs e)
        {
            AppData.ApplicationId = 1;

            Users.SetCurrentUser(e.LoggedInUser);
            
            IsUserLoggedIn = true;
            
            SetUserPreference();

            UserNameRole = Users.GetCurrentUserTitle();

            StartNotificationService();
           
            var applicationViewModel = new ApplicationViewModel();
            SelectedView = applicationViewModel;
        }

        private void ConfigViewDBConnected(object sender, PetraEventArgs.DBConnectedEventArgs e)
        {
            ShowLoginView();
        }

        private async void StartNotificationService()
        {
            var dueTime = TimeSpan.FromSeconds(0);
            var interval = TimeSpan.FromMinutes(Double.Parse(Settings.GetSetting(Constants.SETTINGS_TIME_INTERVAL_UPDATE_NOTIFICATIONS)));
            await Utils.DoPeriodicWorkAsync(new Func<bool>(UpdateNotifications), dueTime, interval, CancellationToken.None);
        }

        public bool UpdateNotifications()
        {
            NoteText = Notification.GetNotificationStatus();
            NoteToolTip = Notification.GetNotificationToolTip();
            NotificationsView.UpdateNotifications();
            return true;
        }

        private void SetUserPreference()
        {
            string selectedTheme = (Users.IsLoggedIn()) ? Users.GetCurrentUser().theme.Trim() : "BaseLight";
            string selectedAccent = (Users.IsLoggedIn()) ? Users.GetCurrentUser().accent.Trim() : "Olive";
            SettingsView.SetDefaultSettings(selectedTheme, selectedAccent);
        }

        #endregion
    }
}

using MahApps.Metro;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.MessagingService;
using PetraERP.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;


namespace PetraERP.ViewModels
{
    public class UserSettingsViewModel : FlyoutViewModelBase
    {
        #region Private Members

        private string _selectedAccent;
        private string _selectedTheme;
        private string _oldPassword;
        private string _newPassword;
        private string _reenterNewPassword;

        #endregion

        #region Public Properties

        public IList<string> AccentColorlist { get; private set; }

        public IList<string> ThemeColorlist { get; private set; }

        public string SelectedAccent
        {
            get { return _selectedAccent; }
            set
            {
                if (value == _selectedAccent)
                    return;
                _selectedAccent = value;
                OnPropertyChanged(GetPropertyName(() => SelectedAccent));
                ChangeAppearance();
            }
        }

        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (value == _selectedTheme)
                    return;
                _selectedTheme = value;
                OnPropertyChanged(GetPropertyName(() => SelectedTheme));
                ChangeAppearance();
            }
        }

        public string OldPassword
        {
            get { return _oldPassword; }
            set
            {
                if (value == _oldPassword)
                    return;
                _oldPassword = value;
                OnPropertyChanged(GetPropertyName(() => OldPassword));
            }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                if (value == _newPassword)
                    return;
                _newPassword = value;
                OnPropertyChanged(GetPropertyName(() => NewPassword));
                OnPropertyChanged(GetPropertyName(() => ReenterNewPassword));
            }
        }

        public string ReenterNewPassword
        {
            get { return _reenterNewPassword; }
            set
            {
                if (value == _reenterNewPassword)
                    return;
                _reenterNewPassword = value;
                OnPropertyChanged(GetPropertyName(() => ReenterNewPassword));
            }
        }

        #endregion

        #region Commands

        public ICommand ChangePasswordCommand { get; private set; }

        #endregion

        #region Constructor

        public UserSettingsViewModel()
        {
            ThemeColorlist = ThemeManager.AppThemes.Select(a => a.Name).ToList();
            AccentColorlist = ThemeManager.Accents.Select(a => a.Name).ToList();
            
            _selectedTheme = (Users.IsLoggedIn())  ? Users.GetCurrentUser().theme.Trim() : "BaseLight";
            _selectedAccent = (Users.IsLoggedIn()) ? Users.GetCurrentUser().accent.Trim() : "Olive";

            ChangeAppearance(false);

            //Initialize commands
            ChangePasswordCommand = new RelayCommand(ChangePassword, CanChangePassword);
        }

        #endregion

        #region Public Methods

        public void SetDefaultSettings(string themeName, string accentName)
        {
            if (themeName == null)
                throw new ArgumentNullException("themeName");
            if (accentName == null)
                throw new ArgumentNullException("accentName");
            
            _selectedTheme = themeName;
            OnPropertyChanged(GetPropertyName(() => SelectedTheme));

            _selectedAccent = accentName;
            OnPropertyChanged(GetPropertyName(() => SelectedAccent));
        }

        #endregion

        #region Overriden Methods

        protected override string GetErrorForProperty(string propertyName)
        {
            string error = null;
            switch (propertyName)
            {
                case "ReenterNewPassword":
                    if (string.IsNullOrEmpty(ReenterNewPassword))
                        error = "New password cannot be empty";
                    if (string.Compare(ReenterNewPassword, NewPassword, false) != 0)
                        error = "New password does not match confirmation.";
                    break;
            }
            CommandManager.InvalidateRequerySuggested();
            return error;
        }

        #endregion

        #region Private Helpers

        private void ChangeAppearance(bool save = true)
        {
            // Change accent 
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(_selectedAccent);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);

            // Change theme
            theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(_selectedTheme);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);

            // Save new appearance
            if (save)
            {
                Users.SaveAppearance(SelectedTheme, SelectedAccent);
            }
        }

        private void PromptUserToSaveAppearance()
        {
            if (AppData.MessageService.ShowMessage("Save new settings?", DialogType.Question) == DialogResponse.Yes)
            {
                LogUtil.LogInfo("SettingsViewModel", "PromptUserToSaveAppearance", "User changed apperance.");
                try
                {
                    Users.SaveAppearance(SelectedTheme, SelectedAccent);
                }
                catch (Exception ex)
                {
                    LogUtil.LogError("SettingsViewModel", "PromptUserToSaveAppearance",ex);
                    AppData.MessageService.ShowMessage("Error", DialogType.Error);
                }
            }
        }

        private void ChangePassword()
        {
            if(AppData.CurrentUser!=null)
            {
                try
                {
                    LogUtil.LogInfo("SettingsViewModel", "PromptUserToSaveAppearance", "User attempted password change.");
                    string oldp = BCrypt.HashPassword(OldPassword, BCrypt.GenerateSalt());
                    if (string.Compare(AppData.CurrentUser.password, oldp, false) != 0)
                    {
                        AppData.MessageService.ShowMessage("Old password does not match password in DB.", DialogType.Error);
                        return;
                    }
                    Users.UpdateUserPassword(AppData.CurrentUser.username, NewPassword);

                    AppData.MessageService.ShowMessage("Your password has been updated");

                    OldPassword = NewPassword = ReenterNewPassword = null;
                }
                catch (Exception ex)
                {
                    LogUtil.LogError("SettingsViewModel", "ChangePassword", ex);
                    AppData.MessageService.ShowMessage("Error", DialogType.Error);
                }
            }
            else
            {
                AppData.MessageService.ShowMessage("Error", DialogType.Error);
            }
        }

        private bool CanChangePassword()
        {
            return GetErrorForProperty("ReenterNewPassword") == null;
        }

        #endregion
    }
}

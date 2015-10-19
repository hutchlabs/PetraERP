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

namespace PetraERP.ViewModels
{
    public class LoginViewModel : WorkspaceViewModelBase
    {
        #region Members Variables

        private string _userName;
        private string _password;
        private string _buttonText = "Login";
        private int _passChangeCount = 0;
        private event EventHandler<PetraERP.Shared.PetraEventArgs.UserLoggedInEventArgs> _userLoggedIn;

        #endregion

        #region Public Members

        public string Username
        {
            get { return _userName; }
            set
            {
                if (_userName == value)
                    return;
                _userName = value;
                OnPropertyChanged(GetPropertyName(() => Username));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value)
                    return;
                _password = value;
                OnPropertyChanged(GetPropertyName(() => Password));
            }
        }

        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                if (_buttonText == value)
                    return;
                _buttonText = value;
                OnPropertyChanged(GetPropertyName(() => ButtonText));
            }
        }

        #endregion

        #region Events

        public event EventHandler<PetraERP.Shared.PetraEventArgs.UserLoggedInEventArgs> UserLoggedIn
        {
            add { _userLoggedIn += value; }
            remove { _userLoggedIn -= value; }
        }

        #endregion

        #region Commands

        public ICommand TryLogInCommand { get; private set; }

        public ICommand TryResetPassCommand { get; private set; }

        #endregion

        #region Constructor

        public LoginViewModel() : base("Login")
        {
            TryLogInCommand = new RelayCommand(TryLoginIn, CanTryLogin);
            TryResetPassCommand = new RelayCommand(TryResetPass, CanTryReset);
            Username = "admin@petratrust.com";
            Password = "redred";
        }

        #endregion

        #region Private Helpers

        private void doLogin(string username, string password)
        {
            var u = (_passChangeCount > 0) ? Users.GetUser(username) : Users.ValidateLoginAttempt(username, password);

            if (u != null)
            {
                if (Users.IsFirstLogin(username))
                {
                    ButtonText = "Change Password";
                    changePassword(username, u.password);
                }
                else if (_userLoggedIn != null)
                {
                    _userLoggedIn(this, new PetraERP.Shared.PetraEventArgs.UserLoggedInEventArgs(u));
                }
            }
            else
            {
                LogUtil.LogInfo("LoginViewModel", "doLogin", string.Format("Failed login attempted for  username: {0}.", username));
                AppData.MessageService.ShowMessage("Login Error: wrong password.");
            }
        }

        private void TryLoginIn()
        {
            try
            {
                doLogin(Username, Password);
            }
            catch (Exception ex)
            {
                LogUtil.LogError("LoginViewModel", "TryLoginIn", ex);
                AppData.MessageService.ShowMessage(ex.Message, DialogType.Error);
            }
        }

        private void TryResetPass()
        {
            bool success = false;
            bool cancel = false;

            try
            {
                DialogResponse r = AppData.MessageService.ShowMessage("Do you really want to request a password reset?", "Password Reset", DialogType.QuestionWithCancel);

                if (r == DialogResponse.Ok)
                {
                    Users.ResetPasswordRequest(Username);
                    success = true;
                }
                else 
                { 
                    cancel = true;
                }
            }
            catch (Exception) { success = false; }

            if (!cancel)
            {
                if (success)
                {

                    AppData.MessageService.ShowMessage("Reset message sent to Administrator. Please follow up with them.", "Reset Password", DialogType.Message);
                }
                else
                {
                    AppData.MessageService.ShowMessage("Username is incorrect. Please re-enter.", "Reset Password Error", DialogType.Error);
                }
            }
        }

        private void changePassword(string username, string oldpass)
        {

            if (_passChangeCount==0) {
                AppData.MessageService.ShowMessage("This is your first login. Please change your password.", "Change Password");
                _passChangeCount++;
            } else {

                if (Users.CheckPassword(Password, oldpass)) // password not changed
                {
                    AppData.MessageService.ShowMessage("Password is the same: You really need to change  your password.", "Change Password", DialogType.Error);
                }
                else
                {
                    if (Password.Length > 3)
                    {
                        bool success = false;
                        string err = "";
                        try
                        {
                            Users.UpdateUserPassword(username, Password);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            err = ex.GetBaseException().ToString();
                        }

                        if (success)
                        {
                           AppData.MessageService.ShowMessage("Password Updated!", "Change Password", DialogType.Message);

                           doLogin(Username, Password);
                        }
                        else
                        {
                            AppData.MessageService.ShowMessage(err, "Change Password", DialogType.Error);
                        }
                    }
                    else
                    {
                        AppData.MessageService.ShowMessage("Password is too short!", "Change Password", DialogType.Error);
                    }
                }
            }
        }

        private bool CanTryLogin()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        private bool CanTryReset()
        {
            return !string.IsNullOrEmpty(Username);
        }

        #endregion
    }
}
using PetraERP.Shared;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.Utility;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PetraERP.CRM.ViewModels
{
    public class CRMViewModel : WorkspaceViewModelBase
    {
        #region Private Members

        #endregion

        #region Public Properties

        public Visibility ShowAdminTab
        {
            get
            {
                return (Users.IsCurrentUserCRMAdmin()) ? Visibility.Visible : Visibility.Collapsed;
            }
            private set { }
        }

        #endregion

        #region Constructor

        public CRMViewModel(string registeredName, bool canUserNavigate) : base(registeredName, canUserNavigate)
        {
            DisplayName = "CRM";
            CanGoBack = true;
        }

        #endregion

        #region Private Methods

        private void setting_ValueChanged(string sender, string e="")
        {
            string setting = "";
            string value = e.ToString();
            bool save = false;

            switch (sender)
            {
                //case "time_interval_updatenotifications": { setting = Constants.SETTINGS_TIME_INTERVAL_UPDATE_NOTIFICATIONS; value = _tiUpdateNotifications.ToString(); save = validate_time_value(setting, _tiUpdateNotifications.ToString()); break; }
                //case "tb_emailsmtphost": { setting = Constants.SETTINGS_EMAIL_SMTP_HOST; save = validate_email_value(setting, value); break; }
                //case "tb_emailfrom": { setting = Constants.SETTINGS_EMAIL_FROM; save = validate_email_value(setting, value); break; }
                case "": break;
                default: save = false; break;
            }


            if (save)
            {
                Settings.Save(setting, value);
            }
        }

        private bool validate_time_value(string setting, string value)
        {
            bool pass = true;

            if (value == string.Empty || value == null)
                pass = false;

            return pass;
        }

        private bool validate_email_value(string setting, string value)
        {
            bool pass = true;

            if (value == string.Empty || value == null)
                pass = false;

            pass = (setting == PetraERP.Shared.Constants.SETTINGS_EMAIL_SMTP_HOST) ? SendEmail.IsValidSMTP(value) : SendEmail.IsValidEmail(value);

            return pass;
        }

        #endregion

        #region Override Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            try
            {

            }
            catch (Exception ex)
            {
                AppData.MessageService.ShowMessage(ex.GetBaseException().ToString());
            }
        }


        protected override string GetErrorForProperty(string propertyName)
        {
            if (propertyName == "SMTPProperty" && "" == string.Empty)
            {
                return "Invalid SMTP host address. Please re-enter.";
            }

            if (propertyName == "EmailProperty" && "" == string.Empty)
            {
                return "Invalid email address. Please re-enter.";
            }

            return null;
        }


        protected override void OnDispose()
        {
            base.OnDispose();
        }

        #endregion
    }
}

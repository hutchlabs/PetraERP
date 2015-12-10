using PetraERP.Shared;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.Utility;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace PetraERP.Tracker.ViewModels
{
    public class TrackerViewModel : WorkspaceViewModelBase
    {
        #region Private Members

        private string _smtpProperty;
        private string _emailProperty;
        private double _tiUpdateNotifications;
        private bool _spinnerActive = false;

        #endregion

        #region Public Properties

        public bool SpinnerActive { get { return _spinnerActive; } set { _spinnerActive = value; } }

        public double TI_UpdateNotifications { 
            get { return _tiUpdateNotifications;  } 
            set
            {
                if (_tiUpdateNotifications == value)
                    return;
                _tiUpdateNotifications = value;
                OnPropertyChanged(GetPropertyName(() => TI_UpdateNotifications));
                setting_ValueChanged("time_interval_updatenotifications");
            }
        }

        public string SMTPProperty
        {
            get { return this._smtpProperty; }
            set
            {
                if (Equals(value, _smtpProperty))
                {
                    return;
                }

                _smtpProperty = value;
                OnPropertyChanged(GetPropertyName(() => SMTPProperty));
                setting_ValueChanged("SMTPPropery", value);
            }
        }

        public string EmailProperty
        {
            get { return this._emailProperty; }
            set
            {
                if (Equals(value, _emailProperty))
                {
                    return;
                }

                _emailProperty = value;
                OnPropertyChanged(GetPropertyName(() => EmailProperty));
                setting_ValueChanged("SMTPPropery", value);
            }
        }

        #endregion

        #region Constructor

        public TrackerViewModel(string registeredName, bool canUserNavigate) : base(registeredName, canUserNavigate)
        {
            DisplayName = "Tracker";
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
                case "time_interval_updatenotifications": { setting = Constants.SETTINGS_TIME_INTERVAL_UPDATE_NOTIFICATIONS; value = _tiUpdateNotifications.ToString(); save = validate_time_value(setting, _tiUpdateNotifications.ToString()); break; }
                case "tb_emailsmtphost": { setting = Constants.SETTINGS_EMAIL_SMTP_HOST; save = validate_email_value(setting, value); break; }
                case "tb_emailfrom": { setting = Constants.SETTINGS_EMAIL_FROM; save = validate_email_value(setting, value); break; }
                default: save = false; break;
            }

            SpinnerActive = false;

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

            if (setting == Constants.SETTINGS_EMAIL_SMTP_HOST)
                SpinnerActive = true;

            if (value == string.Empty || value == null)
                pass = false;

            pass = (setting == Constants.SETTINGS_EMAIL_SMTP_HOST) ? SendEmail.IsValidSMTP(value) : SendEmail.IsValidEmail(value);

            return pass;
        }

        #endregion

        #region Override Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            try
            {
                // set time values
                TI_UpdateNotifications = Double.Parse(Settings.GetSetting(Constants.SETTINGS_TIME_INTERVAL_UPDATE_NOTIFICATIONS));

                // set mail values
                SMTPProperty = Settings.GetSetting(Constants.SETTINGS_EMAIL_SMTP_HOST);
                EmailProperty = Settings.GetSetting(Constants.SETTINGS_EMAIL_FROM);
            }
            catch (Exception ex)
            {
                AppData.MessageService.ShowMessage(ex.GetBaseException().ToString());
            }
        }


        protected override string GetErrorForProperty(string propertyName)
        {
            if (propertyName == "SMTPProperty" && SMTPProperty == string.Empty)
            {
                return "Invalid SMTP host address. Please re-enter.";
            }

            if (propertyName == "EmailProperty" && EmailProperty == string.Empty)
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

using PetraERP.Shared;
using PetraERP.Shared.Datasources;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.MessagingService;
using PetraERP.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PetraERP.ViewModels
{
    public class ERPSettingsViewModel : WorkspaceViewModelBase
    {
        #region Private Members

        private int _selectedIdx = 0;
        private bool _isUpdate = true;
        private string _title = "Welcome";
        private Visibility _showCancel = Visibility.Collapsed;


        private ERP_User _user;
        private IEnumerable<ERP_User> _users;
        private ObservableCollection<object> _selectedUsers = new ObservableCollection<object>();

        private string _filterValue;
        private bool _filterChecked = false;
        private readonly string[] _userFilterOptions = { Constants.STATUS_ALL, Constants.USER_STATUS_ACTIVE,
                                                         Constants.USER_STATUS_NONACTIVE, Constants.USER_STATUS_ONLINE,
                                                         Constants.USER_STATUS_OFFLINE};

        private ERP_Role _erpRole;
        private ERP_Role _crmRole;
        private ERP_Role _trackerRole;
        private ComboBoxPairs _selectedDepartment;

        private string _smtpProperty;
        private string _emailProperty;
        private double _tiUpdateNotifications;
        private bool _spinnerActive = false;

        #endregion

        #region Public Properties

        #region For User grid
        
        public int SelectedIdx
        {
            get { return _selectedIdx; }
            set
            {
                _selectedIdx = value;
                OnPropertyChanged(GetPropertyName(() => SelectedIdx));
            }
        }

        public ERP_User SelectedUser
        {
            get { return _user; }
            set
            {
                if (value == _user)
                    return;
                _user = value;

                reset();

                if (_user != null)
                {
                    Title = "Editing " + _user.first_name + " " + _user.last_name;
                    SetRoles();
                }
                OnPropertyChanged(GetPropertyName(() => SelectedUser));
            }
        }

        public IEnumerable<ERP_User> Users
        {
            get { return _users; }
            set
            {
                if (value == _users)
                    return;
                _users = value;
                OnPropertyChanged(GetPropertyName(() => Users));
                OnPropertyChanged(GetPropertyName(() => UsersCount));
            }
        }

        public string UsersCount
        {
            get
            {
                if (_users == null)
                    return "0 Users";
                return string.Format("{0} Users", _users.Count());
            }
            private set { ; }
        }

        public string[] UserFilterOptions
        {
            get { return _userFilterOptions; }
            private set { ;  }
        }

        public string UsersFilterValue
        {
            get { return _filterValue; }
            set { _filterValue = value; }
        }

        public bool UsersFilterChecked
        {
            get { return _filterChecked; }
            set
            {
                _filterChecked = value;
                UpdateGrid();
            }
        }

        public ObservableCollection<object> SelectedItems
        {
            get
            {
                return _selectedUsers;
            }
            set
            {
                _selectedUsers = value;
            }
        }

        #endregion

        #region For User adding/editing
   
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(GetPropertyName(() => Title)); }
        }
        
        public Visibility ShowCancel
        {
            get { return _showCancel; }
            set
            {
                _showCancel = value;
                OnPropertyChanged(GetPropertyName(() => ShowCancel));
            }
        }

        public ComboBoxPairs SelectedDepartment
        {
            get { return _selectedDepartment; }
            set
            {
                _selectedDepartment = value;
                OnPropertyChanged(GetPropertyName(() => SelectedDepartment));
            }
        }
        public ObservableCollection<ComboBoxPairs> Departments
        {
            get 
            {
                IEnumerable<ERP_Department> deps = PetraERP.Shared.Models.Users.GetDepartments();
                ObservableCollection<ComboBoxPairs> cbp = new ObservableCollection<ComboBoxPairs>();

                foreach (var d in deps)
                {
                    cbp.Add(new ComboBoxPairs(d.id.ToString(), d.name));
                }

                return cbp;
            }
            private set { ; }
        }

        public ERP_Role ERPRole { get { return _erpRole; } set { _erpRole = value;  OnPropertyChanged(GetPropertyName(() => ERPRole)); } }
        public IEnumerable<ERP_Role> ERPRoles
        {
            get { return PetraERP.Shared.Models.Users.ERPRoles(); }
            private set { ; }
        }

        public ERP_Role CRMRole { get { return _crmRole; } set { _crmRole = value;  OnPropertyChanged(GetPropertyName(() => CRMRole)); } }
        public IEnumerable<ERP_Role> CRMRoles
        {
            get { return PetraERP.Shared.Models.Users.CRMRoles(); }
            private set { ; }
        }

        public ERP_Role TrackerRole { get { return _trackerRole; } set { _trackerRole = value; OnPropertyChanged(GetPropertyName(() => TrackerRole)); } }
        public IEnumerable<ERP_Role> TrackerRoles
        {
            get { return PetraERP.Shared.Models.Users.TrackerRoles(); }
            private set { ; }
        }

        #endregion

        #region For time & settings 

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

        #endregion

        #region Commands

        public ICommand CancelCreateCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            DialogResponse res = AppData.MessageService.ShowMessage("Are you sure you want to cancel?", DialogType.Question);
                            if (res == DialogResponse.Yes)
                            {
                                reset();
                                SelectedIdx = 0;
                            }
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }
    
        public ICommand AddUserCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            SelectedUser = new ERP_User();
                            Title = "Adding New User";
                            _isUpdate = false;
                            ShowCancel = Visibility.Visible;
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message + err.GetBaseException().ToString());
                        }
                    }
                };
            }
        }

        public ICommand SaveUserCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            if (validate_user())
                            {
                                string message = "";

                                if (_isUpdate)
                                {
                                    SelectedUser.ERP_Department = PetraERP.Shared.Models.Users.GetDepartment(int.Parse(SelectedDepartment._Key));
                                    PetraERP.Shared.Models.Users.UpdateUserRoles(new UserAppRoles { user_id = SelectedUser.id,
                                                                                                    erp_role = ERPRole,
                                                                                                    crm_role = CRMRole,
                                                                                                    tracker_role = TrackerRole});
                                    
                                    PetraERP.Shared.Models.Users.Save(SelectedUser);

                                    message = "User successfully updated.";
                                }
                                else
                                {
                                    int uid = PetraERP.Shared.Models.Users.AddUser(SelectedUser.username, 
                                                                         SelectedUser.password, 
                                                                         SelectedUser.first_name, 
                                                                         SelectedUser.last_name, 
                                                                         "", 
                                                                         int.Parse(SelectedDepartment._Key));

                                    PetraERP.Shared.Models.Users.AddRoleToUser(ERPRole.id, uid);
                                    PetraERP.Shared.Models.Users.AddRoleToUser(CRMRole.id, uid);
                                    PetraERP.Shared.Models.Users.AddRoleToUser(TrackerRole.id, uid);

                                    UpdateGrid();
                                    SelectedIdx = Users.Count() - 1;
                                    message = "New user successfully created.";
                                }

                                AppData.MessageService.ShowMessage(message, "Manage Users");
                            }
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }

        public ICommand ActivateUserCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            //TicketsAddView win = new TicketsAddView();
                            //win.Closed += window_ClosingFinished;
                            //win.ShowDialog();
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }

        public ICommand DeactivateUserCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            //TicketsAddView win = new TicketsAddView();
                            //win.Closed += window_ClosingFinished;
                            //win.ShowDialog();
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }

        public ICommand FilterUsersCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        UpdateGrid();
                    }
                };
            }
        }

        public ActionCommand<SelectionChangedEventArgs> FilterUsersSelectionCommand
        {
            get { return new ActionCommand<SelectionChangedEventArgs>(OnUserFilterSelect); }
        }

        public ActionCommand<RoutedEventArgs> PasswordChangedCommand
        {
            get 
            {
                return new ActionCommand<RoutedEventArgs>(OnPasswordChanged); 
            }
        }

        #endregion

        #region Constructor

        public ERPSettingsViewModel(string registeredName, bool canUserNavigate) : base(registeredName, canUserNavigate)
        {
            DisplayName = "Admin Settings";
            CanGoBack = true;
        }

        #endregion

        #region Private Methods

        #region User Methods

        private void SetRoles()
        {
            try
            {
                UserAppRoles uar = PetraERP.Shared.Models.Users.GetUserRoles(SelectedUser.id);
                ERPRole = uar.erp_role;
                CRMRole = uar.crm_role;
                TrackerRole = uar.tracker_role;
            }
            catch (Exception) { }
        }


        private void OnUserFilterSelect(SelectionChangedEventArgs e)
        {
            UpdateGrid();
        }

        private void OnPasswordChanged(RoutedEventArgs e)
        {
            PasswordBox pb = (PasswordBox) e.Source;

            if (SelectedUser.password != pb.Password)
            {
                SelectedUser.password = BCrypt.HashPassword(pb.Password, BCrypt.GenerateSalt());
            }
        }

        private void reset()
        {
            _isUpdate = true;
            Title = "";
            ShowCancel = Visibility.Collapsed;
        }

        private bool validate_user()
        {
            if (SelectedUser.first_name == string.Empty)
            {
                AppData.MessageService.ShowMessage("Please provide the first name of the user.", "Error", DialogType.Error);
                return false;
            }
            else if (SelectedUser.first_name == string.Empty)
            {
                AppData.MessageService.ShowMessage("Please provide the last name of the user.", "Error", DialogType.Error);
                return false;
            }
            else if (SelectedUser.username == string.Empty)
            {
                AppData.MessageService.ShowMessage("Please provide the email of the user.", "Error", DialogType.Error);
                return false;
            }
            else if (SelectedUser.password == string.Empty)
            {
                AppData.MessageService.ShowMessage("Please provide a password the user.", "Error", DialogType.Error);
                return false;
            }
            else if (SelectedDepartment == null)
            {
                AppData.MessageService.ShowMessage("Please select a department for the user.", "Error", DialogType.Error);
                return false;
            }
            else if (ERPRole == null)
            {
                AppData.MessageService.ShowMessage("Please select a role for the ERP application.", "Error", DialogType.Error);
                return false;
            }
            else if (CRMRole == null)
            {
                AppData.MessageService.ShowMessage("Please select a role for the CRM application.", "Error", DialogType.Error);
                return false;
            }
            else if (TrackerRole == null)
            {
                AppData.MessageService.ShowMessage("Please select a role for the Tracker application.", "Error", DialogType.Error);
                return false;
            }


            return true;
        }

        private void UpdateGrid()
        {
            string filter = UsersFilterValue;

            // Get items
            if (filter == Constants.USER_STATUS_ACTIVE) { Users = PetraERP.Shared.Models.Users.GetActiveUsers(); }
            else if (filter == Constants.USER_STATUS_NONACTIVE) { Users = PetraERP.Shared.Models.Users.GetNonActiveUsers(); }
            else if (filter == Constants.USER_STATUS_ONLINE) { Users = PetraERP.Shared.Models.Users.GetOnlineUsers(); }
            else if (filter == Constants.USER_STATUS_OFFLINE) { Users = PetraERP.Shared.Models.Users.GetOfflineUsers(); }
            else { Users = PetraERP.Shared.Models.Users.GetUsers(); }
        }

        #endregion

        #region Time setting methods

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

                // Update user grid
                UpdateGrid();
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

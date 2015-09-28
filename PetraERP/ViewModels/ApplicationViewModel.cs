using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.Navigation;
using PetraERP.CRM.ViewModels;
using PetraERP.Tracker.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace PetraERP.ViewModels
{
    public class ApplicationViewModel : WorkspaceViewModelBase
    {
        #region Private members

        private WorkspaceViewModelBase _selectedWorkspace;

        #endregion

        #region Public Properties

        public INavigator Navigator { get; private set; }
 
        public WorkspaceViewModelBase SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set
            {
                if (_selectedWorkspace == value)
                    return;
                _selectedWorkspace = value;
                OnPropertyChanged(GetPropertyName(() => SelectedWorkspace));
            }
        }

        #endregion

        #region Commands

        public ICommand NavigateBackCommand { get; private set; }

        #endregion

        #region Constructor

        public ApplicationViewModel() : base("ApplicationViewModel")
        {
            //Configure the navigator
            Navigator = NavigatorFactory.GetNavigator();
            
            var viewList = new List<WorkspaceViewModelBase>()
                               {
                                   new TrackerViewModel("Tracker", Users.IsCurrentUserTrackerUser()),
                                   new CRMViewModel("CRM", Users.IsCurrentUserCRMUser()),
                                   new ERPSettingsViewModel("Admin Settings", Users.IsCurrentUserSuperAdmin())
                               };
            viewList.ForEach(wvm => Navigator.AddView(wvm));
            
            Navigator.AddHomeView(new HomeViewModel(viewList, "Homeview"));
            
            Navigator.PropertyChanged += NavigatorPropertyChanged;
            
            Navigator.NavigateToHome();
            
            NavigateBackCommand = new RelayCommand(Navigator.NavigateBack);
        }

        #endregion

        #region Private event handler

        private void NavigatorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GetPropertyName(() => Navigator.CurrentView))
            {
                SelectedWorkspace = Navigator.CurrentView;
                switch (Navigator.CurrentView.RegisteredName)
                {
                    case "Tracker":   AppData.ApplicationId = PetraERP.Shared.Constants.ERPAPPS_TRACKER; break;
                    case "CRM": AppData.ApplicationId = PetraERP.Shared.Constants.ERPAPPS_CRM; break;
                    default:
                        AppData.ApplicationId = PetraERP.Shared.Constants.ERPAPPS_ERP; break;
                }
                //System.Console.WriteLine("Application id is " + AppData.ApplicationId);
            }
        }

        #endregion

        #region Overriden Methods

        protected override void OnDispose()
        {
            Navigator.PropertyChanged -= NavigatorPropertyChanged;
        }

        #endregion
    }
}

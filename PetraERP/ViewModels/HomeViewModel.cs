using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PetraERP.ViewModels
{
    public sealed class HomeViewModel : WorkspaceViewModelBase
    {
        #region Private Members

        private ObservableCollection<WorkspaceViewModelBase> _allViews;

        #endregion

        #region Public Properties

        public ObservableCollection<WorkspaceViewModelBase> AllViews
        {
            get { return _allViews; }
            set
            {
                if (_allViews == value)
                    return;
                _allViews = value;
                OnPropertyChanged(GetPropertyName(() => AllViews));
            }
        }

        #endregion

        #region Commands

        public ICommand GoToCommand { get; private set; }

        #endregion

        #region Constructor

        public HomeViewModel(IEnumerable<WorkspaceViewModelBase> viewList, string registeredName) : base(registeredName)
        {
            if (null == viewList)
                throw new ArgumentNullException("viewList");

            DisplayName = "Home";
            CanGoBack = false;
            AllViews = new ObservableCollection<WorkspaceViewModelBase>(viewList);
            GoToCommand = new RelayCommand<string>(GoToView);
        }

        #endregion

        #region Private Methods

        private void GoToView(string viewRegisteredName)
        {
            var navigator = NavigatorFactory.GetNavigator();

            var wvm = AllViews.FirstOrDefault(vm => vm.RegisteredName == viewRegisteredName);
            if (wvm != null && !wvm.CanUserNavigate)
            {
                AppData.MessageService.ShowMessage("You do not have access to this application.");
                return;
            }
            navigator.NavigateToView(viewRegisteredName);
        }

        #endregion
    }
}

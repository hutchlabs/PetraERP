using PetraERP.Shared.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PetraERP.Shared.UI.Navigation
{
    internal class Navigator : INavigator
    {
        #region Member Variables

        private PropertyChangedEventHandler _propertyChanged;
        private readonly IDictionary<string, WorkspaceViewModelBase> _views;
        private WorkspaceViewModelBase _homeView;
        private WorkspaceViewModelBase _currentView;

        #endregion

        #region INavigator Implementation

        void INavigator.NavigateBack()
        {
            if (_currentView.CanGoBack)
            {
                NavigateToHome();
            }
        }

        WorkspaceViewModelBase INavigator.CurrentView
        {
            get { return _currentView; }
            set
            {
                if (value == _currentView)
                    return;
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }

        void INavigator.AddView(WorkspaceViewModelBase workspaceView)
        {
            if (null == workspaceView)
                throw new ArgumentNullException("workspaceView");
            _views.Add(workspaceView.RegisteredName, workspaceView);
        }

        void INavigator.AddHomeView(WorkspaceViewModelBase homeView)
        {
            _homeView = homeView;
        }

        void INavigator.NavigateToView(string viewKey)
        {
            if (_views.ContainsKey(viewKey))
            {
                _currentView =_views[viewKey];
                OnPropertyChanged("CurrentView");
            }
        }

        void INavigator.NavigateToHome()
        {
            NavigateToHome();
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        IEnumerable<WorkspaceViewModelBase> INavigator.GetAllView()
        {
            return _views.Keys.Select(key => _views[key]).ToList();
        }

        #endregion

        #region Constructor

        public Navigator()
        {
            _views = new Dictionary<string, WorkspaceViewModelBase>();
        }

        #endregion

        #region Private Methods

        private void OnPropertyChanged(string propertyName)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void NavigateToHome()
        {
            _currentView = _homeView;
            OnPropertyChanged("CurrentView");
        }

        #endregion
    }
}

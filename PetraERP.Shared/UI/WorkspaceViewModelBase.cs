using System;

namespace PetraERP.Shared.UI
{
    public abstract class WorkspaceViewModelBase : ViewModelBase
    {
        #region Public properties

        public bool CanGoBack
        {
            get { return _canGoBack; }
            set
            {
                if (_canGoBack == value) 
                    return;
                _canGoBack = value;
                OnPropertyChanged(GetPropertyName(() => CanGoBack));
            }
        }

        public bool CanUserNavigate { get; protected set; }

        public string RegisteredName { get; private set; }

        #endregion

        #region Constructor

        protected WorkspaceViewModelBase(string registeredName, bool canUserNavigate=true)
        {
            if (string.IsNullOrEmpty(registeredName))
                throw new ArgumentNullException("registeredName");
            CanUserNavigate = canUserNavigate;
            RegisteredName = registeredName;
        }

        #endregion

        #region Private Variables

        private bool _canGoBack;

        #endregion
    }
}

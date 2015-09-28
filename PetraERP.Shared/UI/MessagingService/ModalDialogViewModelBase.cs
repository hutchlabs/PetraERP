using System;
using System.Windows.Input;


namespace PetraERP.Shared.UI.MessagingService
{
    public abstract class ModalDialogViewModelBase : ViewModelBase
    {
        #region Public Members

        public bool IsCancelled
        {
            get { return _isCancelled; }
            set
            {
                if (value != _isCancelled)
                {
                    _isCancelled = value;
                    OnPropertyChanged(GetPropertyName(() => IsCancelled));
                }
            }
        }

        public bool IsOk
        {
            get { return _isOk; }
            set
            {
                if (value != _isOk)
                {
                    _isOk = value;
                    OnPropertyChanged(GetPropertyName(() => IsOk));
                }
            }
        }

        public string TitleText
        {
            get { return _titleText; }
            set
            {
                if (value != _titleText)
                {
                    _titleText = value;
                    OnPropertyChanged(GetPropertyName(() => TitleText));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand OkSelectedCommand { get; private set; }

        public ICommand CancelSelectedCommand { get; private set; }

        #endregion

        #region Constructor

        protected ModalDialogViewModelBase()
        {
            OkSelectedCommand = new RelayCommand(OkSelected);
            CancelSelectedCommand = new RelayCommand(CancelSelected);
        }

        #endregion

        #region Protected Overidable methods

        protected virtual void OnOkSelected()
        {

        }

        protected virtual void OnCancelSelected()
        {

        }

        #endregion

        #region Internal Events

        internal event EventHandler DialogResultSelected
        {
            add { _dialogResultSelected += value; }
            remove { _dialogResultSelected -= value; }
        }

        #endregion

        #region Private Methods

        private void OkSelected()
        {
            IsOk = true;
            NotifyView(DialogResponse.Ok);
        }

        private void CancelSelected()
        {
            IsCancelled = true;
            NotifyView(DialogResponse.Cancel);
        }

        private void NotifyView(DialogResponse dialogResponse)
        {
            if (null != _dialogResultSelected)
                _dialogResultSelected(this, System.EventArgs.Empty);
        }

        #endregion

        #region Private Members

        private bool _isCancelled;
        private bool _isOk;
        private string _titleText;
        internal event EventHandler _dialogResultSelected;

        #endregion
    }
}

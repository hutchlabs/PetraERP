namespace PetraERP.Shared.UI
{
    public abstract class FlyoutViewModelBase : ViewModelBase
    {
        #region protected members.

        protected bool _isOpen;
        protected string _header;
        protected FlyoutTheme _theme;
        protected VisibilityPosition _position;

        #endregion

        #region Public properties

        public string Header
        {
            get { return _header; }
            set
            {
                if (_header == value)
                    return;
                _header = value;
                OnPropertyChanged(GetPropertyName(() => Header));
            }
        }

        public VisibilityPosition Position
        {
            get { return _position; }
            set
            {
                if (_position == value)
                    return;
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value)
                    return;
                _isOpen = value;
                OnPropertyChanged("IsOpen");
            }
        }

        public FlyoutTheme Theme
        {
            get { return _theme; }
            set
            {
                if (_theme == value)
                    return;
                _theme = value;
                OnPropertyChanged("Theme");
            }
        }

        #endregion
    }

    public enum VisibilityPosition
    {
        Right = 0,
        Bottom,
        Left,
        Top
    };

    public enum FlyoutTheme
    {
        AccentedTheme = 0,
        BaseColorTheme,
        InverseTheme,
    }
}

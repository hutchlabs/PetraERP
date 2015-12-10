namespace PetraERP.Shared.UI.Navigation
{
    public static class NavigatorFactory
    {
        #region Member Variables

        private static INavigator _navigator;
        private static object _syncObject;

        #endregion

        #region Public Methods.

        public static INavigator GetNavigator()
        {
            if(_navigator==null)
            {
                lock (_syncObject)
                {
                    if (_navigator == null)
                    {
                        _navigator = new Navigator();
                    }
                }
            }
            return _navigator;
        }

        #endregion

        #region Static constructor

        static NavigatorFactory()
        {
            _syncObject = new object();
        }

        #endregion
    }
}

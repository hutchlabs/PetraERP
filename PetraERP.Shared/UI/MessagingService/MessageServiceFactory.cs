namespace PetraERP.Shared.UI.MessagingService
{
    public static class MessageServiceFactory
    {
        #region Public static Methods

        public static IMessagingService GetMessagingServiceInstance()
        {
            if (null == _messagingServiceInstance)
            {
                lock (_syncObject)
                {
                    if (null == _messagingServiceInstance)
                    {
                        _messagingServiceInstance = new MessagingService();
                    }
                }
            }
            return _messagingServiceInstance;
        }

        #endregion

        #region Member Variables

        private static volatile IMessagingService _messagingServiceInstance;
        private static readonly object _syncObject = new object();

        #endregion
    }
}

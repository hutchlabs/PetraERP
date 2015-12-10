using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PetraERP.Shared.UI.MessagingService
{
    public class ModalViewRegistry
    {
        #region Public Members

        public static ModalViewRegistry Instance { get { return _instance.Value; } }

        #endregion

        #region Public Methods

        public void RegisterView(string key, Type userControlType)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");
            if (null == userControlType)
                throw new ArgumentException("userControl");
            if (!userControlType.IsSubclassOf(typeof(UserControl)))
                throw new InvalidCastException("Only a user control type can be assigned.");
            if (_modalViewRegistry.ContainsKey(key))
                throw new InvalidOperationException("Key already exists.");
            _modalViewRegistry[key] = userControlType;
        }

        public bool ContainsKey(string viewKey)
        {
            return _modalViewRegistry.ContainsKey(viewKey);
        }

        #endregion

        #region Internal Methods

        internal UserControl GetViewByKey(string key)
        {
            Type userControlType = _modalViewRegistry[key];
            return Activator.CreateInstance(userControlType) as UserControl;
        }

        #endregion

        #region Member Variables

        private readonly IDictionary<string, Type> _modalViewRegistry;
        private static readonly Lazy<ModalViewRegistry> _instance = new Lazy<ModalViewRegistry>(() => new ModalViewRegistry());
        
        #endregion

        #region Contructor

        private ModalViewRegistry()
        {
            _modalViewRegistry = new Dictionary<string, Type>();
        }

        #endregion
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows;

namespace PetraERP.Shared.UI
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        #region Private Members

        private string _displayName;

        #endregion

        #region Public Methods

        public string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression != null)
                return memberExpression.Member.Name;

            return null;
        }

        public void Initialize()
        {
            OnInitialize();
            IsInitialized = true;
        }

        #endregion
        
        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get { return GetErrorForProperty(columnName); }
        }

        protected virtual string GetErrorForProperty(string propertyName)
        {
            return null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            var handler = PropertyChanged;
         
            if (handler == null)
                return;
            
            var e = new PropertyChangedEventArgs(propertyName);
            
            handler(this, e);
        }

        #endregion

        #region Public Properties

        public string ViewName { get; set; }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged(GetPropertyName(() => DisplayName));
            }
        }

        public bool IsInitialized { get; protected set; }

        #endregion
        
        #region Protected Methods

        protected void ThreadSafeInvoke(Action routine)
        {
            var dispatcher = Application.Current.Dispatcher;
            if(null!=dispatcher)
            {
                if(!dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(routine);
                }
                else
                {
                    routine();
                }
            }
        }

        protected virtual void OnInitialize()
        {
            
        }
   
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            OnDispose();
            IsInitialized = true;
        }

        protected virtual void OnDispose()
        {

        }

        #endregion

        #region Debugging Aides

    
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (ThrowOnInvalidPropertyName)
                    throw new Exception(msg);

                Debug.Fail(msg);
            }
        }

    
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion
    }
}

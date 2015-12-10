using System;

namespace PetraERP.Shared
{
    public static class Exceptions
    {
        #region User Exceptions

        [Serializable]
        public class UserNotFoundException : ApplicationException
        {
            public UserNotFoundException() { }
            public UserNotFoundException(string message) : base(message) { }
            public UserNotFoundException(string message, System.Exception inner) { }
            protected UserNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        [Serializable]
        public class UserInvalidPasswordException : ApplicationException
        {
            public UserInvalidPasswordException() { }
            public UserInvalidPasswordException(string message) : base(message) { }
            public UserInvalidPasswordException(string message, System.Exception inner) { }
            protected UserInvalidPasswordException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }


        [Serializable]
        public class RoleNotFoundException : ApplicationException
        {
            public RoleNotFoundException() { }
            public RoleNotFoundException(string message) : base(message) { }
            public RoleNotFoundException(string message, System.Exception inner) { }
            protected RoleNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        #endregion

        #region DB Exceptions

        [Serializable]
        public class DBNotSetupException : ApplicationException
        {
            public DBNotSetupException() { }
            public DBNotSetupException(string message) : base(message) { }
            public DBNotSetupException(string message, System.Exception inner) { }
            protected DBNotSetupException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        [Serializable]
        public class DBConnectionException : ApplicationException
        {
            public DBConnectionException() { }
            public DBConnectionException(string message) : base(message) { }
            public DBConnectionException(string message, System.Exception inner) { }
            protected DBConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        #endregion
    }
}

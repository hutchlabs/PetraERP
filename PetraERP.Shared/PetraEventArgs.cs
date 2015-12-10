using PetraERP.Shared.Datasources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetraERP.Shared
{
    public static class PetraEventArgs
    {
        public class UserLoggedInEventArgs : EventArgs
        {
            #region Public Properties

            public ERP_User LoggedInUser { get; private set; }

            #endregion

            #region Constructor

            public UserLoggedInEventArgs(ERP_User user)
            {
                LoggedInUser = user;
            }

            #endregion
        }

        public class DBConnectedEventArgs : EventArgs
        {
            #region Public Properties

            public bool Connected { get; private set; }

            #endregion

            #region Constructor

            public DBConnectedEventArgs(bool cnx)
            {
                Connected = cnx;
            }

            #endregion
        }
    }
}

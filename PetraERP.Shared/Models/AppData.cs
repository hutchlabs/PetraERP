using PetraERP.Shared.Datasources;
using PetraERP.Shared.UI.MessagingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PetraERP.Shared.Models
{
    public static class AppData
    {
        public static ERP_User CurrentUser { get; set; }

        public static string CurrentRole { get; set; }

        public static int CurrentRoleId { get; set; }

        public static int ApplicationId { get; set; }

        public static IMessagingService MessageService { get; set; }

        public static Window AppMainWindow { get { return Application.Current.MainWindow; } }
    }
}

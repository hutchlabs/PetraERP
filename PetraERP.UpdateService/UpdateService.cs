using PetraERP.UpdateService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace PetraERP.UpdateService
{
    #region Service 

    public class UpdateService : ServiceBase
    {      
        #region Private Members

        private static CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private static CancellationToken _token = _tokenSource.Token;
        
        #endregion

        #region Construct & Main

        public UpdateService()
        {
            ServiceName = "PetraERP.UpdateService";
        }

        public static void Main()
        {
            ServiceBase.Run(new UpdateService());
        }

        #endregion

        #region Override methods

        protected override void OnStart(string[] args)
        {
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = 10000;
            timer1.Start();
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            base.OnStart(args);
        }

        public static void timer1_Elapsed(object sender, EventArgs e)
        {
            Task taskA = Task.Factory.StartNew(() =>
            {
                Task taskB = new Task(() => TrackerSchedule.StartUpdate());
                Task taskC = new Task(() => CrmTicket.StartUpdate());

                taskB.Start();
                taskC.Start();
            }, _token);
            
        }

        protected override void OnStop()
        {
            TrackerSchedule.StopUpdate();
            CrmTicket.StopUpdate();
            _tokenSource.Cancel();
            base.OnStop();
        }

        #endregion
    }

    #endregion

    #region Service Installer

    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "PetraERP.UpdateService";
            service.DisplayName = "PetraERP.UpdateService";
            service.Description = "This service periodically updates the status of schedules for the Tracker and (pre) escalations for CRM Tickets.";
            service.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(service);
        }
    }

    #endregion
}
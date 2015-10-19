using PetraERP.TrackerService.Models;
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

namespace PetraERP.TrackerService
{
    #region Service 

    public class TrackerService : ServiceBase
    {      
        #region Private Members

        #endregion

        #region Construct & Main

        public TrackerService()
        {
            ServiceName = "PetraERP.TrackerService";
        }

        public static void Main()
        {
            ServiceBase.Run(new TrackerService());
        }

        #endregion

        #region Override methods

        protected override void OnStart(string[] args)
        {
            TrackerSchedule.Comment("Starting PetraERP Tracker Service.");
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = 10000;
            timer1.Start();
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            base.OnStart(args);
        }

        public static void timer1_Elapsed(object sender, EventArgs e)
        {
            TrackerSchedule.StartUpdate();
        }

        protected override void OnStop()
        {
            TrackerSchedule.Comment("Stopping PetraERP Tracker Service.");
            TrackerSchedule.StopUpdate();
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
            service.ServiceName = "PetraERP.TrackerService";
            service.DisplayName = "PetraERP.TrackerService";
            service.Description = "This service periodically updates the status of schedules for the Tracker.";
            service.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(service);
        }
    }

    #endregion
}
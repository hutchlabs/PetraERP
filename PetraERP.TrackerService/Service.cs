using PetraERP.TrackerService.Models;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace PetraERP.TrackerService
{
    public partial class Service : ServiceBase
    {
        public TrackerScheduleUpdater tsu = null;

        public Service()
        {
            InitializeComponent();
            ServiceName = "PetraERPTrackerUpdateService";
        }

        protected override void OnStart(string[] args)
        {
            if (tsu != null)
            {
                tsu.Stop();
            }

            tsu = new TrackerScheduleUpdater();
            tsu.Start();
        }

        protected override void OnStop()
        {
            if (tsu != null)
            {
                tsu.Stop();
                tsu = null;
            }
        }
    }

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
            service.ServiceName = "PetraERPTrackerUpdateService";
            service.DisplayName = "PetraERP Tracker Update Service";
            service.Description = "This service updates the schedules in the tracker based on information available.";
            service.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(service);
        }
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetraERP.TrackerService.Models
{
    public class TrackerScheduleUpdater
    {
        public void Start()
        {
            TrackerSchedule.InitiateScheduleWorkFlow();
        }

        public void Stop()
        {

        }
    }
}

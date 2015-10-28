using PetraERP.UpdateService.Datasources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace PetraERP.UpdateService.Models
{
    public class CrmTicket
    {
        #region Private Members

        private static bool _continue = true;
        private static int _interval;
        private static int _defaultUserId = 1;
        private static int OPEN_TICKET_ID = 1;

        #endregion

        #region Constructor

        public CrmTicket()
        {
            _interval = TimeSpan.FromMinutes(5).Milliseconds;
        }

        #endregion

        #region Public Methods

        public static void StartUpdate()
        {
            _continue = true;
            UpdateTicketStatus();    
        }

        public static void StopUpdate()
        {
            _continue = false;
        }

        #endregion

        #region Private Methods

        private static void UpdateTicketStatus()
        {
            if (_continue)
            {
                var tickets = GetOpenTickets();

                foreach (var t in tickets)
                {
                    if (! _continue)
                        break;

                    DateTime now = DateTime.Now;

                    // Let's go through the pre-escalation process 
                    if (now.Subtract(t.created_at).TotalMinutes <= t.pre_escalate)
                    {
                        string job = string.Format("Ticket {0} Pre-escalation", t.ticket_id);

                        ERP_Notification nf = GetNotificationByJob(job, Constants.JOB_TYPE_TICKET, t.id);

                        if (nf == null)
                        {
                            AddNotification((int)t.assigned_to, t.owner, job, Constants.JOB_TYPE_TICKET, t.id);
                        }

                        //Constants.Comment(string.Format("Pre-escalating Ticket {0}. Minutes passed: {1}", t.ticket_id, now.Subtract(t.created_at).TotalMinutes.ToString()));
                    }
                    else if (now.Subtract(t.created_at).TotalMinutes >= t.escalate)
                    {
                        ExpireByJob(string.Format("Ticket {0} Pre-escalation", t.ticket_id), Constants.JOB_TYPE_TICKET, t.id);

                        string job = string.Format("Ticket {0} Escalation", t.ticket_id);
                        
                        ERP_Notification nf = GetNotificationByJob(job, Constants.JOB_TYPE_TICKET, t.id);

                        if (nf == null)
                        {
                            AddNotification((int)t.assigned_to, t.owner, job, Constants.JOB_TYPE_TICKET, t.id);
                        }

                        //Constants.Comment(string.Format("Escalating Ticket {0}. Minutes passed: {1}", t.ticket_id, now.Subtract(t.created_at).TotalMinutes.ToString()));
                    }
                }

                if (_continue)
                {
                    System.Timers.Timer timer1 = new System.Timers.Timer();
                    timer1.Interval = _interval;
                    timer1.Start();
                    timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
                }
            }
        }

        private static void timer1_Elapsed(object sender, EventArgs e)
        {
            UpdateTicketStatus();
        }


        #region Ticket Methods

        private static IEnumerable<Ticket> GetOpenTickets()
        {
            return (from tic in Database.Crm.tickets
                    join cat in Database.Crm.categories on tic.category_id equals cat.id
                    join corress in Database.Crm.correspondences on tic.correspondence_id equals corress.id
                    join sub_corress in Database.Crm.sub_correspondences on corress.id equals sub_corress.correspondence_id
                    join tic_status in Database.Crm.ticket_statuses on tic.status equals tic_status.id
                    from sla in Database.Crm.sla_timers 
                    where tic.status == OPEN_TICKET_ID && sla.ID == sub_corress.sla_id
                    select new Ticket() { id = tic.id, ticket_id = tic.ticket_id, subject = tic.subject, created_at = tic.created_at, owner = tic.owner, assigned_to = tic.assigned_to, pre_escalate=sla.pre_escalate, escalate = sla.escalate});
        }

        #endregion

        #region Notification Methods

        private static void AddNotification(int to_user_id, int from_user_id, string notification_type, string job_type, int jobid)
        {
            try
            {
                ERP_Notification n = new ERP_Notification();
                n.to_user_id = to_user_id;
                n.from_user_id = from_user_id;
                n.application_id = Constants.CRM_APP_ID;
                n.notification_type = notification_type;
                n.job_type = job_type;
                n.job_id = jobid;
                n.times_sent = 1;
                n.last_sent = DateTime.Now;
                n.status = Constants.NF_STATUS_NEW;
                n.modified_by = 1;
                n.created_at = DateTime.Now;
                n.updated_at = DateTime.Now;
                Database.Erp.ERP_Notifications.InsertOnSubmit(n);
                Database.Erp.SubmitChanges();
            }
            catch (Exception e)
            {
                Constants.Comment("Error adding notification: "+e.Message);
            }
        }

        private static ERP_Notification GetNotificationByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                return (from n in Database.Erp.ERP_Notifications
                        where (n.status != Constants.NF_STATUS_EXPIRED && n.status != Constants.NF_STATUS_RESOLVED) &&
                              (n.notification_type == notification_type) &&
                              (n.job_id == job_id) && (n.job_type == job_type)
                        select n).Single();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void ExpireByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                var nf = (from n in Database.Erp.ERP_Notifications
                          where (n.status != Constants.NF_STATUS_EXPIRED && n.status != Constants.NF_STATUS_RESOLVED) &&
                                (n.notification_type == notification_type) &&
                                (n.job_id == job_id) && (n.job_type == job_type)
                          select n).Single();
                nf.status = Constants.NF_STATUS_EXPIRED;
                SaveNotification(nf);
            }
            catch (Exception e)
            {
                //Constants.Comment("Error expiring notification: " + e.Message);
            }
        }

        private static void SaveNotification(ERP_Notification nf)
        {
            try
            {
                nf.modified_by = _defaultUserId; ;
                nf.updated_at = DateTime.Now;
                Database.Erp.SubmitChanges();
            }
            catch (Exception e)
            {
                Constants.Comment("Error saving notification: " + e.Message);

            }
        }

        #endregion

        #endregion

    }

    public class Ticket
    {
        public int id { get; set; }
        public string ticket_id { get; set; }
        public int owner { get; set; }
        public int? assigned_to { get; set; }
        public string subject { get; set; }
        public DateTime created_at { get; set; }
        public int escalate { get; set; }
        public int pre_escalate { get; set; }
    }

}

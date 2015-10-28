using PetraERP.Shared.Datasources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PetraERP.Shared.Models
{
    public class Notification
    {    
        #region Constructor
        
        public Notification()
        {
        }

        #endregion

        #region Public Methods

        public static IEnumerable<ERP_Notification> GetNotifications()
        {
            var userid = AppData.CurrentUser.id;
            
            return (from n in Database.ERP.ERP_Notifications
                    where (n.status != Constants.NF_STATUS_EXPIRED && n.status != Constants.NF_STATUS_RESOLVED) && 
                          (n.to_user_id == userid)
                    orderby n.times_sent descending, n.updated_at descending, n.status descending 
                    select n);
        }

        public static ERP_Notification GetNotificationByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                return (from n in Database.ERP.ERP_Notifications
                        where (n.status != Constants.NF_STATUS_EXPIRED && n.status != Constants.NF_STATUS_RESOLVED) &&
                              (n.notification_type == notification_type) &&
                              (n.job_id == job_id) && (n.job_type==job_type)
                        select n).Single();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ERP_Notification GetAllNotificationByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                return (from n in Database.ERP.ERP_Notifications
                        where (n.notification_type == notification_type) &&
                              (n.job_id == job_id) && (n.job_type == job_type)
                        select n).Single();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void ExpireByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                var nf =  (from n in Database.ERP.ERP_Notifications
                           where (n.status != Constants.NF_STATUS_EXPIRED && n.status != Constants.NF_STATUS_RESOLVED) && 
                                 (n.notification_type == notification_type) &&
                                 (n.job_id == job_id) && (n.job_type == job_type)
                         select n).Single();
                nf.status = Constants.NF_STATUS_EXPIRED;
                Save(nf);
            }
            catch (Exception)
            {
            }    
        }

        public static void ResolveByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                var nf = (from n in Database.ERP.ERP_Notifications
                          where (n.status != Constants.NF_STATUS_EXPIRED) && 
                                (n.status != Constants.NF_STATUS_RESOLVED) && 
                                (n.notification_type == notification_type) &&
                                (n.job_id == job_id) && (n.job_type == job_type)
                          select n).Single();
                nf.status = Constants.NF_STATUS_RESOLVED;
                Save(nf);
            }
            catch (Exception)
            {
            }
        }
     
        public static string GetNotificationStatus()
        {
            var total_notifications = (from n in Database.ERP.ERP_Notifications
                                       where (n.status != Constants.NF_STATUS_EXPIRED) && 
                                             (n.status != Constants.NF_STATUS_RESOLVED) && 
                                             (n.to_user_id == AppData.CurrentUser.id)
                                       select n).Count();
            var new_notifications = (from n in Database.ERP.ERP_Notifications
                                     where (n.to_user_id == AppData.CurrentUser.id) && (n.status == Constants.NF_STATUS_NEW)
                                     select n
                                    ).Count();

            String status = "";
            if (total_notifications == 0)
            {
                status = "Notifications";
            }
            else
            {
                status = (new_notifications == 0) 
                       ? String.Format("{0} Notifications",  total_notifications.ToString())
                       : String.Format("{0} / {1} Notifications", new_notifications.ToString(), total_notifications.ToString()); 
            }
            return  status;
        }

        public static string GetNotificationToolTip()
        {
            var total_notifications = (from n in Database.ERP.ERP_Notifications
                                       where (n.status != Constants.NF_STATUS_EXPIRED) &&
                                             (n.status != Constants.NF_STATUS_RESOLVED) && 
                                             (n.to_user_id == AppData.CurrentUser.id)
                                       select n).Count();
            var new_notifications = (from n in Database.ERP.ERP_Notifications
                                     where (n.to_user_id == AppData.CurrentUser.id) && (n.status == Constants.NF_STATUS_NEW)
                                     select n
                                    ).Count();

            String tip = "";
            if (total_notifications == 0)
            {
                tip = " 0 Notifications";
            }
            else
            {
                tip = String.Format("{0} New Notifications\n{1} Total Notifications", new_notifications.ToString(), total_notifications.ToString());
            }
            return tip;
        }

        public static void Save(ERP_Notification nf)
        {
            try
            {
                nf.modified_by = Users.GetCurrentUser().id;
                nf.updated_at = DateTime.Now;
                Database.ERP.SubmitChanges();
            } catch(Exception)
            {
                throw;
            }
        }

        public static void AddToRole(int role_id, string notification_type, string job_type, int jobid)
        {
            IEnumerable<ERP_User> u = Users.GetUsersByRole(role_id);

            foreach (ERP_User x in u)
            {
                Add(x.id, notification_type, job_type, jobid);
            }
        }

        public static void Add(int to_user_id, string notification_type, string job_type, int jobid)
        {
            try 
            {
                ERP_Notification n = new ERP_Notification();
                n.application_id = AppData.ApplicationId;
                n.to_user_id = to_user_id;
                n.from_user_id = Users.GetCurrentUser().id;
                n.notification_type = notification_type;
                n.job_type = job_type;
                n.job_id = jobid;
                n.times_sent = 1;
                n.last_sent = DateTime.Now;
                n.status = Constants.NF_STATUS_NEW;
                n.modified_by = Users.GetCurrentUser().id;
                n.created_at = DateTime.Now;
                n.updated_at = DateTime.Now;
                Database.ERP.ERP_Notifications.InsertOnSubmit(n);
                Database.ERP.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void MarkAsSeen(ERP_Notification item)
        {
            item.status = Constants.NF_STATUS_SEEN;
            Save(item);
        }

        #endregion
    }
}

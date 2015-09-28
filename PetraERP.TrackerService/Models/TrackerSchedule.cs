using PetraERP.TrackerService.Datasources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PetraERP.TrackerService.Models
{
    public class TrackerSchedule
    {
        #region Private Members

        private static int _defaultUserId = 1;

        #endregion

        #region Constructor

        public TrackerSchedule()
        {
        }

        #endregion
    
        public static bool InitiateScheduleWorkFlow()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(UpdateScheduleWorkFlowStatus);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateScheduleWorkFlowStatusComplete);
            worker.RunWorkerAsync();
            return true;
        }

        private static void UpdateScheduleWorkFlowStatus(object sender, DoWorkEventArgs e)
        {
            var schedules = GetSchedulesForProcessing();

            foreach (var s in schedules)
            {
                if (s.ptas_fund_deal_id == 0 || s.ptas_fund_deal_id == null)
                {
                    s.ptas_fund_deal_id = GetFundDealId(s.company_id, s.tier, s.contributiontypeid, s.month, s.year);
                }
                EvaluateScheduleWorkFlow(s);
            }
        }

        private static async void UpdateScheduleWorkFlowStatusComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            var dueTime = TimeSpan.FromSeconds(5);
            var interval = TimeSpan.FromMinutes(Double.Parse(GetSetting(Constants.SETTINGS_TIME_INTERVAL_UPDATE_SCHEDULES)));
            await DoPeriodicWorkAsync(new Func<bool>(InitiateScheduleWorkFlow), dueTime, interval, CancellationToken.None);
        }

        #region Workflow Methods

        private static Schedule EvaluateScheduleWorkFlow(Schedule s)
        {
            // Lock this schedule for now
            s.processing = true;
            SaveSchedule(s);

            string[] passedStates = { Constants.WF_VALIDATION_PASSED, Constants.WF_STATUS_PASSED_NEW_EMPLOYEE };

            if (passedStates.Contains(s.validation_status))
            {
                EvaluatePassedSchedule(s);
            }
            else
            {
                //  Is the Schedule Validated?
                s.validated = CheckValidation(s.company_id, s.tier, s.contributiontypeid, s.month, s.year);
                
                if (s.validated)
                {
                    s.amount = (decimal) GetTotalContribution(s.company_id, s.tier, s.contributiontypeid, s.month, s.year);

                    s.validation_valuetime = CheckValidationTime(s.company_id, s.tier, s.contributiontypeid, s.month, s.year);

                    s.validation_status = CheckValidationStatus(s.company_id, s.tier, s.contributiontypeid, s.month, s.year);

                    //  Has Schedule has now passed?
                    if (passedStates.Contains(s.validation_status))
                    {
                        // Yes
                        s.workflow_status = Constants.WF_STATUS_PAYMENTS_PENDING;
                        s.workflow_summary = "Schedule has been validated with no errors. Waiting for payments.";
                        
                        // Expire any pending validation requestions
                        ExpireByJob(Constants.NF_TYPE_SCHEDULE_VALIDATION_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);
                        ExpireByJob(Constants.NF_TYPE_SCHEDULE_ERRORFIX_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);
                        ExpireByJob(Constants.NF_TYPE_SCHEDULE_ERRORFIX_ESCALATION_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);
                        
                        EvaluatePassedSchedule(s);
                    }
                    else
                    {
                        // No
                        s.workflow_status = Constants.WF_STATUS_ERROR_PREFIX + s.validation_status;
                        EvaluateReminderStatus(s);  
                    }
                }
                else
                {
                    // send reminders
                    int times_sent = UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_VALIDATION_REQUEST, Constants.SETTINGS_TIME_INTERVAL_VALIDATION_REQUEST);
                    s.workflow_status = Constants.WF_VALIDATION_NOTDONE_REMINDER;
                    s.workflow_summary = string.Format("Schedule is not validated. {0} notification request sent.",times_sent);
                    s.processing = false;
                    SaveSchedule(s);
                }
            }

            return s;
        }

        private static Schedule EvaluateReminderStatus(Schedule s)
        {
            bool vsent = (s.validation_email_sent ==null)? false: (bool)s.validation_email_sent;
            bool escalated = (s.escalation_email_sent==null) ? false : (bool)s.escalation_email_sent;
            string errmsg = ""; //string.Format("{0}", s.validation_status);

            if (escalated)
            {
                ExpireByJob(Constants.NF_TYPE_SCHEDULE_VALIDATION_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);

                ExpireByJob(Constants.NF_TYPE_SCHEDULE_ERRORFIX_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);

                s.workflow_summary = (vsent) ? string.Format("{0} Issue escalated on {1}. Email sent to client on {2}.", errmsg, s.escalation_email_date,s.validation_email_date)
                                           : string.Format("{0} Issue escalated on {1}.",errmsg, s.escalation_email_date);
            }
            else
            {
                Notification nf = GetNotificationByJob(Constants.NF_TYPE_SCHEDULE_ERRORFIX_REQUEST,
                                                           Constants.JOB_TYPE_SCHEDULE,
                                                           s.id);
                DateTime now = DateTime.Now;


                if (nf != null)
                {
                    // Let's go through the escalation process 
                    if ((now.Subtract(s.created_at).Days * 24) > int.Parse(GetSetting(Constants.SETTINGS_TIME_ERRORFIX_3_REMINDER_WINDOW)))
                    {
                        if (nf.times_sent == 2) // It's the 5th day, escalate
                        {
                            // Expire old notification
                            nf.times_sent += 1;
                            nf.last_sent = DateTime.Now;
                            nf.status = Constants.NF_STATUS_EXPIRED;
                            SaveNotification(nf);
                            s.workflow_summary = (vsent) ? string.Format("{0}. 4th and final notification request sent. Email sent to client on {1}. Please escalate issue.", errmsg, s.validation_email_date)
                                                        : string.Format("{0} 4th and final notification request sent. Please escalate issue.",errmsg);
                        }
                    }
                    else if ((now.Subtract(s.created_at).Days * 24) > int.Parse(GetSetting(Constants.SETTINGS_TIME_ERRORFIX_2_REMINDER_WINDOW)))
                    {
                        if (nf.times_sent == 1) // It's the 3rd day, send a remider
                        {
                            nf.times_sent += 1;
                            nf.last_sent = DateTime.Now;
                            nf.status = Constants.NF_STATUS_NEW;
                            SaveNotification(nf);
                            s.workflow_summary = (vsent) ? string.Format("{0} 3rd notification request sent. Email sent to client on {1}.", errmsg, s.validation_email_date)
                                                     :  string.Format("{0} 3rd notification request sent.",errmsg);
                        }
                    }
                    else
                    {
                        s.workflow_summary = (vsent) ? string.Format("{0} 2nd notification request sent. Email sent to client on {1}.", errmsg, s.validation_email_date)
                                                     :  string.Format("{0} 2nd notification request sent.",errmsg);
                    }
                }
                else
                {
                    // Notification has not been sent. It's the 2nd day, send a reminder
                    if ((now.Subtract(s.created_at).Days * 24) > int.Parse(GetSetting(Constants.SETTINGS_TIME_ERRORFIX_1_REMINDER_WINDOW)))
                    {
                        AddNotification(Constants.ROLES_OPS_USER_ID, Constants.NF_TYPE_SCHEDULE_ERRORFIX_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);
                        s.workflow_summary = (vsent) ? string.Format("{0} 2nd notification request sent. Email sent to client on {1}.", errmsg, s.validation_email_date)
                                                     :  string.Format("{0} 2nd notification request sent.",errmsg);
                    }
                    else
                    {
                        s.workflow_summary = (vsent) ? string.Format("{0} 1st notification request sent. Email sent to client on {1}.", errmsg, s.validation_email_date)
                                      :  string.Format("{0} 1st notification request sent yet.",errmsg);
                    }
                }
            }
            
            s.processing = false;
            SaveSchedule(s);

            return s;
        }

        private static void EvaluatePassedSchedule(Schedule s)
        {
            if (s.payment_id == 0 || s.payment_id == null)
            {
                // Check payments
                Tuple<int,string>  status = CheckPaymentStatus(s.company_id, s.tier, s.contributiontype, s.month, s.year, s.contributiontypeid, s.ptas_fund_deal_id);
                if (status.Item1 == 0)
                {
                    // No payments found. Will check later
                    s.processing = false;
                    SaveSchedule(s);
                }
                else
                {
                    s.payment_id = status.Item1;
                    s = SaveSchedule(s);
                    s.workflow_status = (status.Item2=="Linked") ?  Constants.WF_STATUS_PAYMENTS_LINKED : Constants.WF_STATUS_PAYMENTS_RECEIVED;
                    s.workflow_summary = "Schedule linked to Payments. Waiting for Receipt to be sent and File download & uploaded";
                    EvaluatePaymentReceivedSchedule(s);
                }
            }
            else
            {
                EvaluatePaymentReceivedSchedule(s);
            }    
        }
    
        private static Schedule EvaluatePaymentReceivedSchedule(Schedule s)
        {
            s.processing = true;
            SaveSchedule(s);

            if (s.receipt_sent && s.file_downloaded && s.file_uploaded)
            {
                s.workflow_status = Constants.WF_STATUS_COMPLETED;
                s.workflow_summary = string.Format("Receipt sent {0} and File downloaded {1} and File uploaded {2}", s.receipt_sent_date.ToString(), s.file_downloaded_date.ToString(), s.file_uploaded_date.ToString());
            }
            else if (s.receipt_sent && s.file_downloaded && !s.file_uploaded)
            {
                s.workflow_status = Constants.WF_STATUS_RF_SENT_DOWNLOAD_NOUPLOAD;
                s.workflow_summary = string.Format("Receipt sent {0} and File downloaded {1}. No File uploaded", s.receipt_sent_date.ToString(), s.file_downloaded_date.ToString());
                s = EvaluateFileUploadNotificationStatus(s);
            }
            else if (s.receipt_sent && !s.file_downloaded && !s.file_uploaded)
            {
                s.workflow_status = Constants.WF_STATUS_RF_SENT_NODOWNLOAD_NOUPLOAD;
                s.workflow_summary = string.Format("Receipt sent {0}. No File downloaded and no File uploaded", s.receipt_sent_date.ToString());
                UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_FILE_DOWNLOAD_REQUEST, Constants.SETTINGS_TIME_FILE_DOWNLOAD_INTERVAL);
            }
            else if (!s.receipt_sent && s.file_downloaded && s.file_uploaded)
            {
                s.workflow_status = Constants.WF_STATUS_RF_NOSENT_DOWNLOAD_UPLOAD;
                s.workflow_summary = string.Format("No Receipt sent. File downloaded {0} and File uploaded {1}", s.file_downloaded_date.ToString(), s.file_uploaded_date.ToString());
                UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_RECEIPT_SEND_REQUEST, Constants.SETTINGS_TIME_INTERVAL_SEND_RECEIPT);
            }
            else if (!s.receipt_sent && s.file_downloaded && !s.file_uploaded)
            {
                s.workflow_status = Constants.WF_STATUS_RF_NOSENT_DOWNLOAD_NOUPLOAD;
                s.workflow_summary = string.Format("No Receipt sent. File downloaded {0} and no File uploaded", s.file_downloaded_date.ToString());
                UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_RECEIPT_SEND_REQUEST, Constants.SETTINGS_TIME_INTERVAL_SEND_RECEIPT);
                s = EvaluateFileUploadNotificationStatus(s);
            }
            else if (!s.receipt_sent && !s.file_downloaded && !s.file_uploaded)
            {
                Tuple<int, string> status = CheckPaymentStatus(s.company_id, s.tier, s.contributiontype, s.month, s.year, s.contributiontypeid, s.ptas_fund_deal_id);
                s.workflow_status = (status.Item2=="Linked") ? Constants.WF_STATUS_PAYMENTS_LINKED: Constants.WF_STATUS_PAYMENTS_RECEIVED;
                s.workflow_summary = "Schedule linked to Payments. Waiting for Receipt to be sent and File download & uploaded";
                UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_RECEIPT_SEND_REQUEST, Constants.SETTINGS_TIME_INTERVAL_SEND_RECEIPT);
                UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_FILE_DOWNLOAD_REQUEST, Constants.SETTINGS_TIME_FILE_DOWNLOAD_INTERVAL);
            }
            else
            {
                // Shouldn't reach here..but we should handle file_uploaded, but not downloaded yet issue. how?
            }

            s.processing = false;
            SaveSchedule(s);

            return s;
        }

        private static Schedule EvaluateFileUploadNotificationStatus(Schedule s)
        {
            bool fileuploaded = false;
            try
            {
                fileuploaded = CheckFileUploaded(s.company, s.company_id, s.tier, s.PPayment.value_date, s.PPayment.transaction_amount);
            }
            catch(Exception)
            {
            }

            if (fileuploaded)
            {
                 s.file_uploaded = true;
                 s.file_uploaded_date = DateTime.Now;
                               
                if (s.receipt_sent)
                {
                    s.workflow_status = Constants.WF_STATUS_COMPLETED;
                    s.workflow_summary = string.Format("Receipt sent {0} and File downloaded {1} and File uploaded {2}", s.receipt_sent_date.ToString(), s.file_downloaded_date.ToString(), s.file_uploaded_date.ToString());
                }
                else
                {
                    s.workflow_status = Constants.WF_STATUS_RF_NOSENT_DOWNLOAD_UPLOAD;
                    s.workflow_summary = string.Format("No Receipt sent. File downloaded {0} and File uploaded {1}", s.file_downloaded_date.ToString(), s.file_uploaded_date.ToString());
                }

                // Resolve old notifications
                ResolveByJob(Constants.NF_TYPE_SCHEDULE_FILE_UPLOAD_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);
            }
            else
            {
                if (FileUploadWindowHasExpired((DateTime)s.file_downloaded_date))
                {

                    s.file_downloaded = false;
                    s.file_downloaded_date = null;

                    if (s.receipt_sent)
                    {
                        s.workflow_status = Constants.WF_STATUS_RF_SENT_NODOWNLOAD_NOUPLOAD;
                        s.workflow_summary = string.Format("Receipt sent {0}. No File downloaded and no File uploaded", s.receipt_sent_date.ToString());
                    }
                    else
                    {
                        Tuple<int, string> status = CheckPaymentStatus(s.company_id, s.tier, s.contributiontype, s.month, s.year, s.contributiontypeid, s.ptas_fund_deal_id);
                        s.workflow_status = (status.Item2 == "Linked") ? Constants.WF_STATUS_PAYMENTS_LINKED : Constants.WF_STATUS_PAYMENTS_RECEIVED;
                        s.workflow_summary = "Schedule linked to Payments. Waiting for Receipt to be sent and File download & uploaded";
                    }

                    // Expire old notifications and send a new one for the file download.
                    ExpireByJob(Constants.NF_TYPE_SCHEDULE_FILE_UPLOAD_REQUEST, Constants.JOB_TYPE_SCHEDULE, s.id);
                    UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_FILE_DOWNLOAD_REQUEST, Constants.SETTINGS_TIME_FILE_DOWNLOAD_INTERVAL);
                }
                else
                {
                    UpdateNotificationStatus(s.id, Constants.NF_TYPE_SCHEDULE_FILE_UPLOAD_REQUEST, Constants.SETTINGS_TIME_FILE_UPLOAD_INTERVAL);
                }
            }
            return s;
        }

        #endregion

        #region Private Helper Methods

        #region Schedule Methods
  
        private static IEnumerable<Schedule> GetSchedulesForProcessing()
        {
            return (from j in Database.Tracker.Schedules
                    where j.processing == false &&
                          j.workflow_status != Constants.WF_STATUS_INACTIVE &&
                          j.workflow_status != Constants.WF_STATUS_COMPLETED &&
                          j.workflow_status != Constants.WF_STATUS_ERROR_ESCALATED &&
                          j.workflow_status != Constants.WF_STATUS_EXPIRED &&
                          j.workflow_status != Constants.WF_STATUS_REVALIDATE
                    orderby j.updated_at ascending
                    select j);
        }

        private static Schedule SaveSchedule(Schedule s)
        {
            s.modified_by = _defaultUserId;
            s.updated_at = DateTime.Now;
            Database.Tracker.SubmitChanges();
            return s;
        }

        #endregion

        #region Validation Methods

        private static bool CheckValidation(string companyid, string tier, int ctid, int month, int year)
        {
            DateTime dealDate = new DateTime(year, month, 1);
            try
            {
                int fd = (from j in Database.PTAS.FundDeals
                          where j.ContribType_ID == ctid &&
                                j.CompanyEntityId == companyid.Trim() &&
                                j.Tier == tier.Replace(" ", "") &&
                                j.TotalContribution != 0 &&
                          ((DateTime)j.DealDate).Date == dealDate.Date
                          select j).Count();

                return (fd > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static DateTime CheckValidationTime(string companyid, string tier, int ctid, int month, int year)
        {
            DateTime dealDate = new DateTime(year, month, 1);
            try
            {

               var fd = (from j in Database.PTAS.FundDeals
                          where j.ContribType_ID == ctid &&
                                j.CompanyEntityId == companyid.Trim() &&
                                j.Tier == tier.Replace(" ", "") &&
                                j.TotalContribution != 0 &&
                                ((DateTime)j.DealDate).Date == dealDate.Date
                         select j).Single();

               var fld = (from k in Database.PTAS.FundDealLines where k.FundDealID == fd.FundDealID select k.DateStamp).Min();

               return (DateTime) fld;
            }
            catch (Exception)
            {
                return dealDate;
            }
        }

        private static string CheckValidationStatus(string companyid, string tier, int ct, int month, int year)
        {
            DateTime dealDate = new DateTime(year, month, 1);
            string status = Constants.WF_VALIDATION_NOTDONE;

            try
            {
                var fd = (from j in Database.PTAS.FundDeals
                          where j.ContribType_ID == ct &&
                           j.CompanyEntityId == companyid.Trim() &&
                           j.Tier == tier.Replace(" ", "") &&
                           j.TotalContribution != 0 &&
                           ((DateTime)j.DealDate).Date == dealDate.Date
                          select j).Single();

                var fld = (from k in Database.PTAS.FundDealLines where k.FundDealID == fd.FundDealID select k);

                bool passed = true;
                bool newemp = false;
                bool ssnit = false;
                bool name = false;

                foreach(FundDealLine line in fld)
                {
                    if (line.LineStatus.Contains("Employee not found"))
                    {
                        newemp = true;
                    }
                    if (line.LineStatus.Contains("Error:SSNIT number") || line.LineStatus.Contains("Error:Staff ID"))
                    {
                        passed = false;
                        ssnit = true;
                    }
                    if (line.LineStatus.Contains("Error:Full Name"))
                    {
                        passed = false;
                        name = true;
                    }
                }

                if (passed && newemp) { status = Constants.WF_STATUS_PASSED_NEW_EMPLOYEE; }
                else if (passed) { status = Constants.WF_VALIDATION_PASSED; }
                else if (newemp && (ssnit || name)) { status = Constants.WF_VALIDATION_ERROR_ALL;  }
                else if (ssnit && name) { status = Constants.WF_VALIDATION_ERROR_SSNIT_NAME; }
                else if (newemp) { status = Constants.WF_VALIDATION_NEW_EMPLOYEE; }
                else if (ssnit) { status = Constants.WF_VALIDATION_ERROR_SSNIT; }
                else if (name) { status = Constants.WF_VALIDATION_ERROR_NAME; }
                else { status = Constants.WF_VALIDATION_NOTDONE; }

                return status;            
            }
            catch (Exception)
            {
                return status;
            }
        }

        #endregion
  
        #region Payment Methods

        private static int GetFundDealId(string companyid, string tier, int ctid, int month, int year)
        {
            DateTime dealDate = new DateTime(year, month, 1);
            try
            {
                FundDeal fd = (from j in Database.PTAS.FundDeals
                               where j.ContribType_ID == ctid &&
                                     j.CompanyEntityId == companyid.Trim() &&
                                     j.Tier == tier.Replace(" ", "") &&
                                     j.TotalContribution != 0 &&
                                     ((DateTime)j.DealDate).Date == dealDate.Date
                               select j).DefaultIfEmpty<FundDeal>().FirstOrDefault();

                return (fd == null) ? 0 : fd.FundDealID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.GetBaseException().ToString());
                return 0;
            }
        }

        private static decimal? GetTotalContribution(string companyid, string tier, int ctid, int month, int year)
        {
            DateTime dealDate = new DateTime(year, month, 1);
            try
            {
                FundDeal fd = (from j in Database.PTAS.FundDeals
                               where j.ContribType_ID == ctid &&
                                     j.CompanyEntityId == companyid.Trim() &&
                                     j.Tier == tier.Replace(" ", "") &&
                                     j.TotalContribution != 0 &&
                               ((DateTime)j.DealDate).Date == dealDate.Date
                               select j).Single();

                return fd.TotalContribution;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static Tuple<int, string> CheckPaymentStatus(string company_id, string tier, string ct, int month, int year, int ctid, int? funddealid)
        {
            try
            {
                company_id = GetCompanyCode(company_id);
                string type = "None";

                PPayment pm = GetSubscription(company_id, tier, month, year, ctid);

                if (pm != null)
                {
                    type = (IsLinkedSubscription((int)funddealid)) ? "Linked" : "Received";
                }

                return (pm != null) ? Tuple.Create(pm.id, type) : Tuple.Create(0, type);
            }
            catch (Exception)
            {
                return Tuple.Create(0, "None");
            }
        }

        private static bool IsLinkedSubscription(int funddealid)
        {
            try
            {
                var fd = (from p in Database.PTAS.PaymentScheduleLinks
                          where p.FundDealID == funddealid
                          select p).Count();

                return (fd > 0) ? true : false;
            }

            catch (Exception)
            {
                return false;
            }
        }

        private static PPayment GetSubscription(string company_id, string tier, int month, int year, int ctid)
        {
            try
            {
                return (from p in Database.Tracker.PPayments
                        join pd in Database.Tracker.PDealDescriptions on p.id equals pd.payment_id
                        where p.company_code == company_id &&
                              p.tier == tier &&
                              pd.year == year && pd.month == month &&
                              pd.contribution_type_id == ctid
                        select p).First();
            }
            catch (Exception)
            {
              throw;
            }
        }
        #endregion

        #region File upload Methods

        private static bool FileUploadWindowHasExpired(DateTime file_downloaded_date)
        {
            DateTime now = DateTime.Now;
            return ((now.Subtract(file_downloaded_date).Days * 24) >= int.Parse(GetSetting(Constants.SETTINGS_TIME_FILE_UPLOAD_WINDOW)));
        }

        private static bool CheckFileUploaded(string company, string companyid, string tier, DateTime valueDate, decimal? amount)
        {
            try
            {
                var fd = from a in Database.Microgen.Associations
                         join ae2 in Database.Microgen.cclv_AllEntities on a.TargetEntityID equals ae2.EntityID
                         join ec in Database.Microgen.EntityClients on a.SourceEntityID equals ec.EntityID
                         join d in Database.Microgen.fndDeals on ec.EntityID equals d.EntityFundID
                         join p in Database.Microgen.Purposes on ec.PurposeID equals p.PurposeID
                         where (a.RoleTypeID == 1003)
                               && (ae2.EntityID == int.Parse(companyid.Trim()))
                               && d.DealTypeID == 4 && (d.DealStatusID == 2 || d.DealStatusID == 3) && d.CancellingDealID == null
                               && p.Description.Substring(0, 6).Equals(tier)
                               && (((DateTime)d.DealingDate).Date == valueDate.Date)
                         group new { d, p } by new { d.DealingDate, p.Description } into s
                         select new
                         {
                             Tier = s.Key.Description.Substring(0, 6),
                             DealDate = s.Key.DealingDate,
                             TotalAmount = s.Sum(y => y.d.PaymentAmountDealCcy),
                         };

                foreach (var f in fd)
                {
                    string m = string.Format("Checking against {0} on {1} for {2}", f.Tier, f.DealDate, f.TotalAmount);

                    if (f.TotalAmount == amount && f.Tier == tier)
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Notification Methods

        private static void AddNotification(int role_id, string notification_type, string job_type, int jobid)
        {
            try
            {
                Notification n = new Notification();
                n.to_role_id = role_id;
                n.from_user_id = _defaultUserId;
                n.notification_type = notification_type;
                n.job_type = job_type;
                n.job_id = jobid;
                n.times_sent = 1;
                n.last_sent = DateTime.Now;
                n.status = Constants.NF_STATUS_NEW;
                n.modified_by = _defaultUserId;
                n.created_at = DateTime.Now;
                n.updated_at = DateTime.Now;
                Database.Tracker.Notifications.InsertOnSubmit(n);
                Database.Tracker.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static Notification GetNotificationByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                return (from n in Database.Tracker.Notifications
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

        private static void ResolveByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                var nf = (from n in Database.Tracker.Notifications
                          where (n.status != Constants.NF_STATUS_EXPIRED) &&
                                (n.status != Constants.NF_STATUS_RESOLVED) &&
                                (n.notification_type == notification_type) &&
                                (n.job_id == job_id) && (n.job_type == job_type)
                          select n).Single();
                nf.status = Constants.NF_STATUS_RESOLVED;
                SaveNotification(nf);
            }
            catch (Exception)
            {
            }
        }

        private static void ExpireByJob(string notification_type, string job_type, int job_id)
        {
            try
            {
                var nf = (from n in Database.Tracker.Notifications
                          where (n.status != Constants.NF_STATUS_EXPIRED && n.status != Constants.NF_STATUS_RESOLVED) &&
                                (n.notification_type == notification_type) &&
                                (n.job_id == job_id) && (n.job_type == job_type)
                          select n).Single();
                nf.status = Constants.NF_STATUS_EXPIRED;
                SaveNotification(nf);
            }
            catch (Exception)
            {
            }
        }

        private static int UpdateNotificationStatus(int sch_id, string job, string retry_interval)
        {
            int numNotifications = 1;

            Notification nf = GetNotificationByJob(job, Constants.JOB_TYPE_SCHEDULE, sch_id);

            if (nf != null)
            {
                DateTime now = DateTime.Now;
                if (now.Subtract(nf.last_sent).Hours > int.Parse(GetSetting(retry_interval)))
                {
                    nf.times_sent += 1;
                    nf.last_sent = DateTime.Now;
                    nf.status = Constants.NF_STATUS_NEW; ;
                    SaveNotification(nf);
                }
                numNotifications = nf.times_sent;
            }
            else
            {
                AddNotification(Constants.ROLES_OPS_USER_ID, job, Constants.JOB_TYPE_SCHEDULE, sch_id);
            }

            return numNotifications;
        }

        private static void SaveNotification(Notification nf)
        {
            try
            {
                nf.modified_by = _defaultUserId; ;
                nf.updated_at = DateTime.Now;
                Database.Tracker.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        private static string GetSetting(string setting)
        {
            var x = (from n in Database.Tracker.Settings where n.setting1 == setting select n).Single();
            return x.value;
        }

        private static string GetCompanyCode(string company_id)
        {
            try
            {
                var u = (from c in Database.Microgen.cclv_AllEntities
                         where c.EntityTypeDesc == "Company" && c.FullName.ToLower() != "available" && c.FullName != "Available Company"
                              && c.EntityID == int.Parse(company_id)
                         orderby c.FullName
                         select c).Single();
                return u.EntityKey;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task DoPeriodicWorkAsync(Delegate todoTask, TimeSpan dueTime, TimeSpan interval, CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Update Task
                todoTask.DynamicInvoke();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }

        #endregion
    }

    public static class Constants
    {
        public const int ROLES_OPS_USER_ID = 4;
        public const int ROLES_SUPER_OPS_USER_ID = 3;

        // Job types for Jobs table
        public const string JOB_TYPE_SCHEDULE = "Schedule";
        public const string JOB_TYPE_SUBSCRIPTION = "Subscription";

        // User status
        public const string STATUS_ALL = "All";
        public const string USER_STATUS_ACTIVE = "Active";
        public const string USER_STATUS_NONACTIVE = "Non-Active";
        public const string USER_STATUS_ONLINE = "Online";
        public const string USER_STATUS_OFFLINE = "Offline";

        // Payments status
        public const string PAYMENT_STATUS_APPROVED = "Approved";
        public const string PAYMENT_STATUS_INPROGRESS = "In Progress";

        public const string PAYMENT_STATUS_UNIDENTIFIED = "Unidentified";
        public const string PAYMENT_STATUS_IDENTIFIED = "Identified";
        public const string PAYMENT_STATUS_IDENTIFIED_APPROVED = "Identified and Approved";
        public const string PAYMENT_STATUS_RETURNED = "Returned";
        public const string PAYMENT_STATUS_REJECTED = "Rejected";

        // Notification status and types
        public const string NF_STATUS_NEW = "New";
        public const string NF_STATUS_SEEN = "Seen";
        public const string NF_STATUS_RESOLVED = "Resolved";
        public const string NF_STATUS_EXPIRED = "Expired";

        public const string NF_TYPE_SUBSCRIPTION_APPROVAL_REQUEST = "Subscription Approval Request";
        public const string NF_TYPE_SUBSCRIPTION_APPROVAL_REJECTED = "Subscription Approval Rejected";

        public const string NF_TYPE_SCHEDULE_VALIDATION_REQUEST = "Schedule Validation Request";
        public const string NF_TYPE_SCHEDULE_ERRORFIX_REQUEST = "Schedule Error Fix Request";
        public const string NF_TYPE_SCHEDULE_ERRORFIX_ESCALATION_REQUEST = "Escalation: Schedule Error Fix Request";
        public const string NF_TYPE_SCHEDULE_RECEIPT_SEND_REQUEST = "Schedule Send Receipt Request";
        public const string NF_TYPE_SCHEDULE_FILE_DOWNLOAD_REQUEST = "Schedule File Download Request";
        public const string NF_TYPE_SCHEDULE_FILE_UPLOAD_REQUEST = "Schedule File Upload Request";

        // Workflow Status
        public const string WF_VALIDATION_NOTDONE = "Not Validated";
        public const string WF_VALIDATION_NOTDONE_REMINDER = "Not Validated. Reminders sent";
        public const string WF_VALIDATION_DONE_EMAILSENT = "Validated with errors. Email sent to client";
        public const string WF_VALIDATION_ERROR_SSNIT = "SSNIT Number Error";
        public const string WF_VALIDATION_ERROR_NAME = "Name Error";
        public const string WF_VALIDATION_ERROR_SSNIT_NAME = "SSNIT Number & Name Errors";
        public const string WF_VALIDATION_ERROR_ALL = "New Employee & Errors";
        public const string WF_VALIDATION_ERROR_ESCALATED = "Issue Escalated";
        public const string WF_VALIDATION_PASSED = "Passed";
        public const string WF_VALIDATION_NEW_EMPLOYEE = "New Employee";

        public const string WF_STATUS_INACTIVE = "Inactive";
        public const string WF_STATUS_PASSED_NEW_EMPLOYEE = WF_VALIDATION_PASSED + ". " + WF_VALIDATION_NEW_EMPLOYEE;
        public const string WF_STATUS_ERROR_PREFIX = "Validated with Errors: ";
        public const string WF_STATUS_ERROR_SSNIT = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_SSNIT;
        public const string WF_STATUS_ERROR_NAME = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_NAME;
        public const string WF_STATUS_ERROR_SSNIT_NAME = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_SSNIT_NAME;
        public const string WF_STATUS_ERROR_ALL = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_ALL;
        public const string WF_STATUS_ERROR_ESCALATED = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_ESCALATED;
        public const string WF_STATUS_PAYMENTS_PENDING = "Passed. Payment Pending";
        public const string WF_STATUS_PAYMENTS_LINKED = "Payment Linked. No Receipt. No Download. No Upload";
        public const string WF_STATUS_PAYMENTS_RECEIVED = "Payment Received. No Receipt. No Download. No Upload";
        public const string WF_STATUS_RF_SENT_NODOWNLOAD_NOUPLOAD = "Receipt Sent. No Download. No Upload";
        public const string WF_STATUS_RF_SENT_DOWNLOAD_NOUPLOAD = "Receipt Sent. File Downloaded. No Upload";
        public const string WF_STATUS_RF_NOSENT_DOWNLOAD_UPLOAD = "No Receipt Sent. File Downloaded. File Uploaded";
        public const string WF_STATUS_RF_NOSENT_DOWNLOAD_NOUPLOAD = "No Receipt Sent. File Downloaded. No Upload";
        public const string WF_STATUS_COMPLETED = "Completed";
        public const string WF_STATUS_REVALIDATE = "Resolved and Revalidate";
        public const string WF_STATUS_EXPIRED = "Resolved and Expired";

        // Settings
        public const string SETTINGS_EMAIL_FROM = "email_from";
        public const string SETTINGS_EMAIL_SMTP_HOST = "email_smtp_host";
        public const string SETTINGS_PERM_APPROVE_OWN = "permission_approveown";
        public const string SETTINGS_TIME_ERRORFIX_1_REMINDER_WINDOW = "time_retry_errorfix1st";
        public const string SETTINGS_TIME_ERRORFIX_2_REMINDER_WINDOW = "time_retry_errorfix2nd";
        public const string SETTINGS_TIME_ERRORFIX_3_REMINDER_WINDOW = "time_retry_errorfix3rd";
        public const string SETTINGS_TIME_FILE_DOWNLOAD_INTERVAL = "time_retry_filedownloadrequest";
        public const string SETTINGS_TIME_FILE_UPLOAD_INTERVAL = "time_retry_fileuploadrequest";
        public const string SETTINGS_TIME_FILE_UPLOAD_WINDOW = "time_window_fileupload";
        public const string SETTINGS_TIME_INTERVAL_SEND_RECEIPT = "time_retry_receiptsendrequest";
        public const string SETTINGS_TIME_INTERVAL_VALIDATION_REQUEST = "time_retry_validationrequest";
        public const string SETTINGS_TIME_INTERVAL_UPDATE_SCHEDULES = "time_update_schedules";
        public const string SETTINGS_TIME_INTERVAL_UPDATE_NOTIFICATIONS = "time_update_notifications";
    }

}

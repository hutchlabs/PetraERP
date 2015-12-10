using MahApps.Metro;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.MessagingService;
using PetraERP.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using PetraERP.Shared.Datasources;
using PetraERP.Shared;
using PetraERP.CRM.Views;

namespace PetraERP.ViewModels
{
    public class NotificationsViewModel : FlyoutViewModelBase
    {
        #region Private Members

        private IEnumerable<ERP_Notification> _notifications;
        private ERP_Notification _notification;

        #endregion

        #region Public Properties

        public IEnumerable<ERP_Notification> Notifications 
        {
            get { return _notifications; } 
            set
            {
                if (value == _notifications)
                    return;
                _notifications = value;
                OnPropertyChanged(GetPropertyName(() => Notifications));
            }
        }
 
        public ERP_Notification SelectedNotification
        {
            get { return _notification; }
            set
            {
                if (value == _notification)
                    return;
                _notification = value;
                OnPropertyChanged(GetPropertyName(() => SelectedNotification));
            }
        }

        #endregion

        #region Commands

        public ICommand OpenNotificationCommand { get; private set; }

        #endregion

        #region Constructor

        public NotificationsViewModel()
        {
           OpenNotificationCommand = new RelayCommand(OpenNotification, CanOpenNotification);
        }

        #endregion

        #region Public Methods

        public void UpdateNotifications()
        {
            Notifications = Notification.GetNotifications();
        }

        #endregion

        #region Overriden Methods

        protected override string GetErrorForProperty(string propertyName)
        {
            string error = null;
            switch (propertyName)
            {
                case "NotificationSelected":
                    if (SelectedNotification==null)
                        error = "Selected notification does not exist";
                    break;
            }
            CommandManager.InvalidateRequerySuggested();
            return error;
        }

        #endregion

        #region Private Methods

        private void OpenNotification()
        {
            Notification.MarkAsSeen(SelectedNotification);

            string[] schStates = { Constants.NF_TYPE_SCHEDULE_VALIDATION_REQUEST,
                                   Constants.NF_TYPE_SCHEDULE_ERRORFIX_REQUEST,
                                   Constants.NF_TYPE_SCHEDULE_ERRORFIX_ESCALATION_REQUEST,
                                   Constants.NF_TYPE_SCHEDULE_RECEIPT_SEND_REQUEST,
                                   Constants.NF_TYPE_SCHEDULE_FILE_DOWNLOAD_REQUEST,
                                   Constants.NF_TYPE_SCHEDULE_FILE_UPLOAD_REQUEST };

            string[] subStates = { Constants.NF_TYPE_SUBSCRIPTION_APPROVAL_REQUEST,
                                   Constants.NF_TYPE_SUBSCRIPTION_APPROVAL_REJECTED };

            if (SelectedNotification != null)
            {
                Console.WriteLine("Opening notification: " + SelectedNotification.notification_type);

                if (SelectedNotification.job_type.Trim().Equals(PetraERP.Shared.Constants.JOB_TYPE_TICKET))
                {
                    crmTicketDetails td = CrmData.get_ticket(SelectedNotification.job_id);

                    try
                    {
                        if (td.status_id == 4) //resolved ticket
                        {
                            AppData.MessageService.ShowMessage("Please note: this ticket has already been resolved.", Shared.UI.MessagingService.DialogType.Message);
                        }

                        TicketsEditView win = new TicketsEditView(td.ticket_id);
                        //win.Closed += window_ClosingFinished;
                        win.ShowDialog();
                    }
                    catch (Exception err)
                    {
                        AppData.MessageService.ShowMessage(err.Message);
                    }
                }

                /*object obj = this.FindName("surrogateFlyout");
                Flyout mflyout = (Flyout)obj;
                mflyout.ClosingFinished += notificationflyout_ClosingFinished;

                if (schStates.Contains(SelectedNotification.notification_type.Trim()))
                {
                    mflyout.Content = new ScheduleView(SelectedNotification.job_id, true);
                    ShowHideNotificationFlyout(false);
                    mflyout.IsOpen = !mflyout.IsOpen;
                }
                else if (subStates.Contains(SelectedNotification.notification_type.Trim()))
                {
                    string s = (SelectedNotification.notification_type.Trim() == Constants.NF_TYPE_SUBSCRIPTION_APPROVAL_REQUEST)
                        ? Constants.PAYMENT_STATUS_IDENTIFIED : Constants.PAYMENT_STATUS_REJECTED;

                    mflyout.Content = new verifySubscription(s, SelectedNotification.job_id, true);
                    ShowHideNotificationFlyout(false);
                    mflyout.IsOpen = !mflyout.IsOpen;
                }
                 */
            }
        }

        private bool CanOpenNotification()
        {
            return GetErrorForProperty("NotificationSelected") == null;
        }

        #endregion
    }
}

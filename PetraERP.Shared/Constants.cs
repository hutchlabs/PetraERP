namespace PetraERP.Shared
{
    public static class Constants
    {
        // APP IDS
        public const int ERPAPPS_ERP = 1;
        public const int ERPAPPS_TRACKER = 2;
        public const int ERPAPPS_CRM = 3;

        // Role ids
        public const string ROLES_OPS_USER = "Ops User";
        public const string ROLES_SUPER_OPS_USER = "Super Ops User";
        public const string ROLES_SUPER_USER = "Super User";
        public const string ROLES_CRM_USER = "CRM User";
        public const string ROLES_CRM_ADMIN = "CRM Admin";
        public const string ROLES_ADMINISTRATOR = "Administrator";

        public const int ROLES_OPS_USER_ID = 4;
        public const int ROLES_SUPER_OPS_USER_ID = 3;
        public const int ROLES_CRM_ADMIN_ID = 11;

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
        public const string WF_VALIDATION_ERROR_SSNIT = "SSNIT Number Error";
        public const string WF_VALIDATION_ERROR_NAME = "Name Error";
        public const string WF_VALIDATION_ERROR_SSNIT_NAME = "SSNIT Number & Name Errors";
        public const string WF_VALIDATION_ERROR_ALL = "New Employee & Errors";
        public const string WF_VALIDATION_ERROR_ESCALATED = "Issue Escalated";
        public const string WF_VALIDATION_PASSED = "Passed";
        public const string WF_VALIDATION_NEW_EMPLOYEE = "New Employee";

        public const string WF_STATUS_PASSED_NEW_EMPLOYEE = WF_VALIDATION_PASSED + ". " + WF_VALIDATION_NEW_EMPLOYEE;
        public const string WF_STATUS_ERROR_PREFIX = "Validated with Errors: ";
        public const string WF_STATUS_ERROR_SSNIT = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_SSNIT;
        public const string WF_STATUS_ERROR_NAME = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_NAME;
        public const string WF_STATUS_ERROR_SSNIT_NAME = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_SSNIT_NAME;
        public const string WF_STATUS_ERROR_ALL = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_ALL;
        public const string WF_STATUS_ERROR_ESCALATED = WF_STATUS_ERROR_PREFIX + WF_VALIDATION_ERROR_ESCALATED;
        public const string WF_STATUS_PAYMENTS_PENDING = "Passed. Payment Pending";
        public const string WF_STATUS_PAYMENTS_RECEIVED = "Payment Received. No Receipt. No Download. No Upload";
        public const string WF_STATUS_RF_SENT_NODOWNLOAD_NOUPLOAD = "Receipt Sent. No Download. No Upload";
        public const string WF_STATUS_RF_SENT_DOWNLOAD_NOUPLOAD = "Receipt Sent. File Downloaded. No Upload";
        public const string WF_STATUS_RF_NOSENT_DOWNLOAD_UPLOAD = "No Receipt Sent. File Downloaded. File Uploaded";
        public const string WF_STATUS_RF_NOSENT_DOWNLOAD_NOUPLOAD = "No Receipt Sent. File Downloaded. No Upload";
        public const string WF_STATUS_COMPLETED = "Completed";
        public const string WF_STATUS_EXPIRED = "Resolved and Expired";

        // Settings
        public const string SETTINGS_EMAIL_ADMIN = "email_admin";
        public const string SETTINGS_EMAIL_FROM = "email_from";
        public const string SETTINGS_EMAIL_SMTP_HOST = "email_smtp_host";
        public const string SETTINGS_EMAIL_TMPL_NEWUSER = "tmpl_newuser_email";
        public const string SETTINGS_EMAIL_TMPL_RESETPASS = "tmpl_resetpass_email";
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

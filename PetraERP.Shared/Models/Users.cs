using PetraERP.Shared.Datasources;
using PetraERP.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetraERP.Shared.Models
{
    public static class Users
    {
        #region Private Members

        private static readonly string[] _adminRoles = { Constants.ROLES_ADMINISTRATOR, Constants.ROLES_SUPER_USER };

        #endregion

        #region Login methods

        public static bool IsLoggedIn()
        {
            return (AppData.CurrentUser == null) ? false : true;
        }

        public static ERP_User ValidateLoginAttempt(string username, string password)
        {
            try
            {
                var user = Database.ERP.ERP_Users.Single(u => u.username == username);
                return (CheckPassword(password, user.password)) ? user : null;
            }
            catch (Exception ex)
            {
                AppData.CurrentUser = null;
                AppData.CurrentRole = null;
                LogUtil.LogError("Users", "ValidateLoginAttempt", ex);
                throw new Exceptions.UserNotFoundException("User not found: " + ex.Message);
            }
        }

        public static bool IsFirstLogin(string username = null)
        {
            try
            {
                if (username == null)
                {
                    return AppData.CurrentUser.first_login;
                }
                else
                {
                    var x = Database.ERP.ERP_Users.Single(u => u.username == username);
                    return x.first_login;
                }
            }
            catch (Exception e)
            {
                return false;
                throw e;
            }
        }

        public static void SetCurrentUser(ERP_User user)
        {
            AppData.CurrentUser = user;
            var role = Users.GetCurrentUserRole();
            AppData.CurrentRole = role.role;
            AppData.CurrentRoleId = role.id;
        }

        #endregion

        #region Current User Helper methods

        public static ERP_User GetCurrentUser()
        {
            return AppData.CurrentUser;
        }

        public static string GetCurrentUserTitle()
        {
            string v = "";
            try
            {
                return String.Format("{0} {1} ({2})", AppData.CurrentUser.first_name,
                                                      AppData.CurrentUser.last_name,
                                                      AppData.CurrentRole);
            }
            catch (Exception ex)
            {
                LogUtil.LogError("Users", "GetCurrentUserTitle", ex);
            }

            return v;
        }

        public static bool IsCurrentUserAdmin()
        {
            try
            {
                return _adminRoles.Contains(AppData.CurrentRole);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserSuperAdmin()
        {
            try
            {
                return AppData.CurrentRole.Equals(Constants.ROLES_SUPER_USER);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserSuperOps()
        {
            try
            {
                return AppData.CurrentRole.Equals(Constants.ROLES_SUPER_OPS_USER);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserOps()
        {
            try
            {
                return AppData.CurrentRole.Equals(Constants.ROLES_OPS_USER);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserSuperParser()
        {
            try
            {
                return (IsCurrentUserAdmin() || IsCurrentUserSuperOps());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserParser()
        {
            try
            {
                return (IsCurrentUserAdmin() || IsCurrentUserSuperOps() || IsCurrentUserOps());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserCRMAdmin()
        {
            try
            {
                return (IsCurrentUserSuperAdmin() || AppData.CurrentRole.Equals(Constants.ROLES_CRM_ADMIN));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsCurrentUserCRMUser()
        {
            try
            {
                return (IsCurrentUserSuperAdmin() || AppData.CurrentRole.Equals(Constants.ROLES_CRM_ADMIN) ||
                        AppData.CurrentRole.Equals(Constants.ROLES_CRM_USER));
            }
            catch (Exception)
            {
                throw;
            }
        }

        // TODO: implement Tracker access rules.
        public static bool IsCurrentUserTrackerUser()
        {
            return true;
        }

        // TODO: implement Tracker access rules.
        public static bool IsCurrentUserTrackerAdmin()
        {
            return true;
        }

        #endregion

        #region General User Methods

        public static ERP_User GetUser(string username)
        {
            return (from u in Database.ERP.ERP_Users where u.username == username select u).Single();
        }

        public static IEnumerable<ERP_User> GetUsers()
        {
            return (from u in Database.ERP.ERP_Users select u);
        }

        public static IEnumerable<ERP_User> GetApplicationUsers()
        {
            return (from u in Database.ERP.ERP_Users
                    join ur in Database.ERP.ERP_Users_Roles on u.id equals ur.user_id
                    join r in Database.ERP.ERP_Roles on ur.role_id equals r.id
                    where r.application_id == AppData.ApplicationId
                    select u);
        }

        public static IEnumerable<ERP_User> GetUsersByRole(int role_id)
        {
            return (from u in Database.ERP.ERP_Users
                    join ur in Database.ERP.ERP_Users_Roles on u.id equals ur.user_id
                    join r in Database.ERP.ERP_Roles on ur.role_id equals r.id
                    where r.id == role_id
                    select u);
        }

        public static IEnumerable<ERP_User> GetActiveUsers()
        {
            return (from u in Database.ERP.ERP_Users where u.status == true select u);
        }

        public static IEnumerable<ERP_User> GetNonActiveUsers()
        {
            return (from u in Database.ERP.ERP_Users where u.status == false select u);
        }

        public static IEnumerable<ERP_User> GetOnlineUsers()
        {
            return (from u in Database.ERP.ERP_Users where u.logged_in == true select u);
        }

        public static IEnumerable<ERP_User> GetOfflineUsers()
        {
            return (from u in Database.ERP.ERP_Users where u.logged_in == false select u);
        }

        public static string GetAdminEmail()
        {
            var v = (from r in Database.ERP.ERP_Roles
                     join ur in Database.ERP.ERP_Users_Roles on r.id equals ur.role_id
                     join u in Database.ERP.ERP_Users on ur.user_id equals u.id
                     where r.role == Constants.ROLES_ADMINISTRATOR
                     select u).Single();
            return v.username;
        }

        #endregion

        #region Add and Save methods

        public static int AddUser(string username, string password, string first_name, string last_name, string middle_name, int department)
        {
            try
            {
                ERP_User newUser = new ERP_User();
                newUser.username = username;
                newUser.password = BCrypt.HashPassword(password, BCrypt.GenerateSalt());
                newUser.middle_name = middle_name;
                newUser.first_name = first_name;
                newUser.last_name = last_name;
                newUser.department_id = department;
                newUser.first_login = true;
                newUser.theme = "BaseLight";
                newUser.accent = "Blue";
                newUser.modified_by = AppData.CurrentUser.id;
                newUser.created_at = DateTime.Now;
                newUser.updated_at = DateTime.Now;
                Database.ERP.ERP_Users.InsertOnSubmit(newUser);
                Database.ERP.SubmitChanges();

                SendEmail.sendNewUserMail(first_name + " " + last_name, username, password);

                return newUser.id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SaveAppearance(string SelectedTheme, string SelectedAccent)
        {
            try
            {
                AppData.CurrentUser.theme = SelectedTheme;
                AppData.CurrentUser.accent = SelectedAccent;
                AppData.CurrentUser.modified_by = AppData.CurrentUser.id;
                AppData.CurrentUser.updated_at = DateTime.Now;
                Database.ERP.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ERP_User Save(ERP_User u)
        {
            try
            {
                u.updated_at = DateTime.Now;
                Database.ERP.SubmitChanges();
                return u;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Password methods

        public static bool CheckPassword(string password, string oldpass)
        {
            try
            {
                return BCrypt.CheckPassword(password, oldpass);
            }
            catch (Exception e)
            {
                return false;
                throw e;
            }
        }

        public static void UpdateUserPassword(string username, string p)
        {
            try
            {
                var x = Database.ERP.ERP_Users.Single(u => u.username == username);
                var newpassword = BCrypt.HashPassword(p, BCrypt.GenerateSalt());
                x.password = newpassword;
                x.first_login = false;
                x.updated_at = DateTime.Now;
                Database.ERP.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void ResetPasswordRequest(string username)
        {
            try
            {
                var user = Database.ERP.ERP_Users.Single(u => u.username == username);
                SendEmail.sendResetPasswordMail(username);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Roles methods

        public static int GetCurrentUserRoleId()
        {
            return AppData.CurrentRoleId;
        }

        public static ERP_Role GetCurrentUserRole()
        {
            try
            {
                return (from r in Database.ERP.ERP_Roles
                        join ur in Database.ERP.ERP_Users_Roles on r.id equals ur.role_id
                        join u in Database.ERP.ERP_Users on ur.user_id equals u.id
                        where (r.application_id == AppData.ApplicationId && u.id == AppData.CurrentUser.id)
                        orderby r.id
                        select r).Single();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static UserAppRoles GetUserRoles(int user_id)
        {
            ERP_Role erp = null; ERP_Role crm = null; ERP_Role tracker = null;

            try
            {
                erp = (from r in Database.ERP.ERP_Roles
                                join ur in Database.ERP.ERP_Users_Roles on r.id equals ur.role_id
                                join u in Database.ERP.ERP_Users on ur.user_id equals u.id
                                where (r.application_id == Constants.ERPAPPS_ERP && u.id == user_id)
                                select r).FirstOrDefault();
            } catch(Exception) {}

            try{
                crm = (from r in Database.ERP.ERP_Roles
                                join ur in Database.ERP.ERP_Users_Roles on r.id equals ur.role_id
                                join u in Database.ERP.ERP_Users on ur.user_id equals u.id
                                where (r.application_id == Constants.ERPAPPS_CRM && u.id == user_id)
                                select r).FirstOrDefault();
            } catch(Exception) {}

            try{
                tracker = (from r in Database.ERP.ERP_Roles
                                join ur in Database.ERP.ERP_Users_Roles on r.id equals ur.role_id
                                join u in Database.ERP.ERP_Users on ur.user_id equals u.id
                                where (r.application_id == Constants.ERPAPPS_TRACKER && u.id == user_id)
                                select r).FirstOrDefault();

            }
            catch (Exception){}

            return new UserAppRoles { user_id = user_id, erp_role = erp, crm_role = crm, tracker_role = tracker };
        }

        public static IEnumerable<ERP_Role> ERPRoles()
        {
            return (from r in Database.ERP.ERP_Roles where r.application_id == Constants.ERPAPPS_ERP select r);
        }

        public static IEnumerable<ERP_Role> CRMRoles()
        {
            return (from r in Database.ERP.ERP_Roles where r.application_id == Constants.ERPAPPS_CRM select r);
        }

        public static IEnumerable<ERP_Role> TrackerRoles()
        {
            return (from r in Database.ERP.ERP_Roles where r.application_id == Constants.ERPAPPS_TRACKER select r);
        }

        public static IEnumerable<ERP_Role> GetRoles()
        {
            return (from r in Database.ERP.ERP_Roles select r);
        }

        public static void AddNewRole(int application, string name, string description)
        {
            try
            {
                ERP_Role newRole = new ERP_Role();
                newRole.role = name;
                newRole.application_id = application;
                newRole.description = description;
                newRole.modified_by = AppData.CurrentUser.id;
                newRole.created_at = DateTime.Now;
                newRole.updated_at = DateTime.Now;
                Database.ERP.ERP_Roles.InsertOnSubmit(newRole);
                Database.ERP.SubmitChanges();
            }
            catch (Exception ex)
            {
                LogUtil.LogError("Users", "AddNewRole", ex);
            }
        }

        public static void AddRoleToUser(int role_id, int user_id)
        {
            // Add Role
            ERP_Users_Role ur = new ERP_Users_Role();
            ur.user_id = user_id;
            ur.role_id = role_id;
            Database.ERP.ERP_Users_Roles.InsertOnSubmit(ur);
            Database.ERP.SubmitChanges();
        }

        public static void UpdateUserRoles(UserAppRoles uar)
        {
            DeleteRoles(uar.user_id);

            // Add Role
            AddRoleToUser(uar.erp_role.id, uar.user_id);
            AddRoleToUser(uar.crm_role.id, uar.user_id);
            AddRoleToUser(uar.tracker_role.id, uar.user_id);
        }

        public static void DeleteRoles(int user_id)
        {
            IEnumerable<ERP_Users_Role> roles = (from u in Database.ERP.ERP_Users_Roles where u.user_id==user_id select u);
            Database.ERP.ERP_Users_Roles.DeleteAllOnSubmit(roles);
            Database.ERP.SubmitChanges();
        }

        #endregion

        #region Department methods

        public static ERP_Department GetDepartment(int id)
        {
            return (from d in Database.ERP.ERP_Departments where d.id == id select d).Single();
        }

        public static IEnumerable<ERP_Department> GetDepartments()
        {
            return (from d in Database.ERP.ERP_Departments select d);
        }

        #endregion
    }

    public class UserAppRoles
    {
        public int user_id { get; set; }
        public ERP_Role erp_role { get; set; }
        public ERP_Role crm_role { get; set; }
        public ERP_Role tracker_role { get; set; }
    }
}

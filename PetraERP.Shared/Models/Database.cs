using PetraERP.Shared.Datasources;
using PetraERP.Shared.UI;
using PetraERP.Shared.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace PetraERP.Shared.Models
{
    public static class Database
    {
        #region Private Members

        private static bool _isSetup = false;
        private static CRMDBDataContext _crm;
        private static ERPDBDataContext _erp;
        private static MicrogenDBDataContext _microgen;
        private static PTASDBDataContext _ptas;
        
        private enum CompareValue
        {
            EQUAL = 0,
            LESS,
            MORE
        };

        #endregion

        #region Public Properties

        public static bool IsSetup
        {
            get { return _isSetup; }
            set { _isSetup = value; }
        }

        public static string CurrentVersion
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed
                       ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                       : Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static CRMDBDataContext CRM
        {
            get
            {
                if (_crm == null)
                {
                    string err = Initialize();
                    if (err.Contains("Error code"))
                    {
                        throw new Exceptions.DBConnectionException(err);
                    }
                }
                return _crm;
            }
            set { _crm = value; }
        }

        public static ERPDBDataContext ERP
        {
            get
            {
                if (_erp == null)
                {
                    string err = Initialize();
                    if (err.Contains("Error code"))
                    {
                        throw new Exceptions.DBConnectionException(err);
                    }
                }
                return _erp;
            }
            set { _erp = value; }
        }

        public static MicrogenDBDataContext Microgen
        {
            get
            {
                if (_microgen == null)
                {
                    string err = Initialize();
                    if (err.Contains("Error code"))
                    {
                        throw new Exceptions.DBConnectionException(err);
                    }
                }
                return _microgen;
            }
            set { _microgen = value; }
        }

        public static PTASDBDataContext PTAS
        {
            get
            {
                if (_ptas == null)
                {
                    string err = Initialize();
                    if (err.Contains("Error code"))
                    {
                        throw new Exceptions.DBConnectionException(err);
                    }
                }
                return _ptas;
            }
            set { _ptas = value; }
        }

        #endregion

        #region Constructor

        static Database()
        {
            IsSetup = (Properties.Settings.Default.database_erp != "" &&
                       Properties.Settings.Default.database_microgen != "" &&
                       Properties.Settings.Default.database_ptas != "" &&
                       Properties.Settings.Default.database_crm != "");
        }

        #endregion

        #region Public Methods

        public static string Initialize()
        {
            if (IsSetup)
            {
                try
                {
                    CRM = new CRMDBDataContext(Properties.Settings.Default.database_crm);
                    ERP = new ERPDBDataContext(Properties.Settings.Default.database_erp);
                    Microgen = new MicrogenDBDataContext(Properties.Settings.Default.database_microgen);
                    PTAS = new PTASDBDataContext(Properties.Settings.Default.database_ptas);
                    UpdateDatabase();
                    return "true";
                }
                catch (Exception ex)
                {
                    if (ERP.DatabaseExists())
                    {
                        Utility.LogUtil.LogError("Database", "Initialize", ex);
                        return "Error code 1: Cannot connect to database. " + ex.Message;
                    }
                    else 
                    {
                        Exceptions.DBNotSetupException exe = new Exceptions.DBNotSetupException("Database does not exist.");
                        Utility.LogUtil.LogError("Database", "Initialize", exe);
                        return "Error code 2: Cannot connect to database. Database does not exist.";
                    }
                }
           }
            else
            {
                Exceptions.DBNotSetupException ex = new Exceptions.DBNotSetupException("Database parameters not stored in System settings.");
                Utility.LogUtil.LogError("Database", "Initialize", ex);
                return "Error code 3: Cannot connect to database. Database parameters not stored in System settings.";
            }
        }

        public static bool Setup(string datasource)
        {
            bool success = false;

            try
            {
                String cconStr = "Data Source=" + datasource + ";Initial Catalog=Petra_CRM;Integrated Security=True";
                String tconStr = "Data Source=" + datasource + ";Initial Catalog=Petra_ERP;Integrated Security=True";
                String pconStr = "Data Source=" + datasource + ";Initial Catalog=Petra5;Integrated Security=True";
                String ptasStr = "Data Source=" + datasource + ";Initial Catalog=PTASDB;Integrated Security=True";

                CRM = new CRMDBDataContext(cconStr);
                ERP = new ERPDBDataContext(tconStr);
                Microgen = new MicrogenDBDataContext(pconStr);
                PTAS = new PTASDBDataContext(ptasStr);

                bool t = false;
                bool m = false;
                bool p = false;

                t = ERP.DatabaseExists();
                m = Microgen.DatabaseExists();
                p = PTAS.DatabaseExists();

                if (m && t && p)
                {
                    Properties.Settings.Default.database_crm = cconStr;
                    Properties.Settings.Default.database_erp = tconStr;
                    Properties.Settings.Default.database_microgen = pconStr;
                    Properties.Settings.Default.database_ptas = ptasStr;
                    Properties.Settings.Default.Save();

                    IsSetup = true;

                    UpdateDatabase();

                    success = true;
                }
                else
                {
                    #region Figure out which DB connection failed and throw appropriate error

                    if (!m && !t && !p)
                    {
                        throw new Exceptions.DBConnectionException("Connection to all databases failed.");
                    }
                    else if (!m && !t)
                    {
                        throw new Exceptions.DBConnectionException("Connection to PetraERP and Microgen databases failed.");
                    }
                    else if (!m && !p)
                    {
                        throw new Exceptions.DBConnectionException("Connection to PTASDB and Microgen databases failed.");
                    }
                    else if (!t && !p)
                    {
                        throw new Exceptions.DBConnectionException("Connection to PetraERP and PTASDB databases failed.");
                    }
                    else if (!t)
                    {
                        try
                        {
                            // Create tracker DB and try again
                            ERP.CreateDatabase();
                            RunSql("setup.sql");
                            UpdateDatabase();
                            return Setup(datasource);
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }                   
                    }
                    else if (!m)
                    {
                        throw new Exceptions.DBConnectionException("Connection to Microgen database failed.");
                    }
                    else if (!p)
                    {
                        throw new Exceptions.DBConnectionException("Connection to PTASDB database failed.");
                    }
                    else
                    {
                        throw new Exceptions.DBConnectionException("Connection to a database failed.");
                    }
                    #endregion
                }
            }
            catch (Exceptions.DBNotSetupException)
            {
                throw;
            }
            catch (Exceptions.DBConnectionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogUtil.LogError("Database", "Setup", ex);
                throw ex;
            }

            return success;
        }

        #endregion

        #region Helper Methods

        public static IEnumerable<ComboBoxPairs> GetSQLServers()
        {
            SqlDataSourceEnumerator sqldatasourceenumerator = SqlDataSourceEnumerator.Instance;
            DataTable sources = sqldatasourceenumerator.GetDataSources();

            List<ComboBoxPairs> cbpc = new List<ComboBoxPairs>();

            foreach (DataRow row in sources.Rows)
            {
                string server = string.Format("{0}\\{1}", row["ServerName"], row["InstanceName"]);
                cbpc.Add(new ComboBoxPairs(server, server));
            }

            return cbpc;
        }

        public static string GetCompanyEmail(string company_id)
        {
            try
            {
                /*var c = (from c in Microgen.cclv_AllEntities
                         where c.EntityID == int.Parse(company_id)
                         select c).Single();*/
                return "no-reply@petratrust.com";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetCompanyCode(string company_id)
        {
            try
            {
                var u = (from c in Microgen.cclv_AllEntities
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

        public static IEnumerable<cclv_AllEntity> GetCompanies()
        {
            try
            {
                return (from c in Microgen.cclv_AllEntities
                        where c.EntityTypeDesc == "Company" && c.FullName.ToLower() != "available" && c.FullName != "Available Company"
                        orderby c.FullName
                        select c);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Private Methods for managing DB versioning
        
        private static void UpdateDatabase()
        {
            string[] changes = GetResourcesUnder("Resources/db/changes");
         
            var dbVersion = (from u in ERP.ERP_Settings where u.setting == "app_version" select u).Single();
            if (CompareVersions(dbVersion.value, Database.CurrentVersion)==CompareValue.LESS)
            {
                foreach (string changefile in changes)
                {
                    if (CompareVersions(changefile.Replace(".sql",""), dbVersion.value) == CompareValue.MORE)
                    {
                        string logmsg = string.Format("Updating database from version {0} to {1}", dbVersion.value, changefile.Replace(".sql", ""));
                        Utility.LogUtil.LogInfo("Database", "UpdateDatabase", logmsg);
                        RunSql("changes/" + changefile);
                        dbVersion = (from u in ERP.ERP_Settings where u.setting == "app_version" select u).Single();
                    }
                }
            }
            else if (CompareVersions(dbVersion.value, Database.CurrentVersion) == CompareValue.MORE)
            {
                // throw an error as this an old version of the application.
                Exceptions.DBNotSetupException ex = new Exceptions.DBNotSetupException("The database available is for a newer version of this application. Please upgrade the application or restore the older version of the database.");
                Utility.LogUtil.LogError("Database", "UpdateDatabase", ex);
                throw ex;
            }      
        }

        private static CompareValue CompareVersions(string v1, string v2)
        {
            int ver1 = int.Parse(v1.Replace(".",""));
            int ver2 = int.Parse(v2.Replace(".",""));
            return (ver1 == ver2) ? CompareValue.EQUAL : ((ver1 < ver2) ? CompareValue.LESS : CompareValue.MORE);
        }

        private static void RunSql(string file)
        {
            Uri uri = new Uri("pack://application:,,,/Resources/db/"+file, UriKind.RelativeOrAbsolute);

            var streamResourceInfo = Application.GetResourceStream(uri);

            using (var stream = streamResourceInfo.Stream)
            {
                System.IO.StreamReader r = new System.IO.StreamReader(stream);
                string sql = r.ReadToEnd();
                sql += "\n\nSELECT * FROM Petra_ERP.dbo.ERP_Settings";
                ERP.ExecuteQuery<ERP_Setting>(sql);
            }
        }

        private static string[] GetResourcesUnder(string folder)
        {
            folder = folder.ToLower() + "/";

            var assembly = Assembly.GetCallingAssembly();
            var resourcesName = assembly.GetName().Name + ".g.resources";
            var stream = assembly.GetManifestResourceStream(resourcesName);
            var resourceReader = new ResourceReader(stream);
            var resources =
                from p in resourceReader.OfType<DictionaryEntry>()
                let theme = (string)p.Key
                where theme.StartsWith(folder)
                orderby theme.Substring(folder.Length)
                select theme.Substring(folder.Length);

            return resources.ToArray();
        }

        #endregion
    }
}


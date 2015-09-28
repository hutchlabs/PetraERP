using PetraERP.Shared.Datasources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PetraERP.Shared.Models
{
    public static class Settings
    {
        #region Constructor
       
       
        #endregion

        #region Public Helper Methods

        public static IEnumerable<ERP_Setting> GetSettings()
        {
            return (from n in Database.ERP.ERP_Settings orderby n.setting descending select n);
        }

        public static string GetSetting(string setting)
        {
            var x = (from n in Database.ERP.ERP_Settings where n.setting==setting select n).Single();
            return x.value;
        }

        public static void Save(ERP_Setting s)
        {
            s.modified_by = AppData.CurrentUser.id;
            s.updated_at = DateTime.Now;
            Database.ERP.SubmitChanges();
        }

        public static void Save(string setting, string newval)
        {
            var x = (from n in Database.ERP.ERP_Settings where n.setting == setting select n).Single();
            x.value = newval;
            Save(x);
        }

        public static void Add(string name, string value)
        {
            try
            {
                ERP_Setting s = new ERP_Setting();
                s.setting = name;
                s.value = value;
                s.modified_by = AppData.CurrentUser.id;
                s.created_at = DateTime.Now;
                s.updated_at = DateTime.Now;
                Database.ERP.ERP_Settings.InsertOnSubmit(s);
                Database.ERP.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }
     
        #endregion
    }

}

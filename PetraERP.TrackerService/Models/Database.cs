using PetraERP.TrackerService.Datasources;
using System;

namespace PetraERP.TrackerService.Models
{
    public static class Database
    {
        #region Private Members

        private static TrackerDataContext _tracker;
        private static MicrogenDBDataContext _microgen;
        private static PTASDataContext _ptas;
        
        #endregion

        #region Public Properties

        public static TrackerDataContext Tracker
        {
            get { return _tracker;  }
            set { _tracker = value; }
        }

        public static MicrogenDBDataContext Microgen
        {
            get { return _microgen; }
            set { _microgen = value; }
        }

        public static PTASDataContext PTAS
        {
            get { return _ptas;  }
            set { _ptas = value; }
        }

        #endregion

        static Database()
        {
            string datasource = "ELMINA\\SQLEXPRESS";
            String tconStr = "Data Source=" + datasource + ";Initial Catalog=Petra_tracker;MultipleActiveResultSets=True;Integrated Security=True";
            String pconStr = "Data Source=" + datasource + ";Initial Catalog=Petra5;MultipleActiveResultSets=True;Integrated Security=True";
            String ptasStr = "Data Source=" + datasource + ";Initial Catalog=PTASDB;MultipleActiveResultSets=True;Integrated Security=True";

            Tracker = new TrackerDataContext(tconStr);
            Microgen = new MicrogenDBDataContext(pconStr);
            PTAS = new PTASDataContext(ptasStr);
        }
    }
}


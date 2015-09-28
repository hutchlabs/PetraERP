using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.Sql;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Xml;

namespace PetraERP.Shared.Utility
{
    public static class Utils
    {
        public static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        public static async Task DoPeriodicWorkAsync(Delegate todoTask, TimeSpan dueTime, TimeSpan interval, CancellationToken token)
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

        /// <summary>
        /// Shows the element, playing a storyboard if one is present
        /// </summary>
        /// <param name="element"></param>
        public static void Show(this FrameworkElement element, Action completedAction)
        {
          string animationName = element.Name + "ShowAnim";

          // check for presence of a show animation
          Storyboard showAnim = element.Resources[animationName] as Storyboard;
          if (showAnim != null)
          {
            showAnim.Begin();
            showAnim.Completed += (s, e) => completedAction();
          }
          else
          {
            element.Visibility = Visibility.Visible;
          }
        }

        /// <summary>
        /// Hides the element, playing a storyboard if one is present
        /// </summary>
        /// <param name="element"></param>
        public static void Hide(this FrameworkElement element)
        {
          string animationName = element.Name + "HideAnim";

          // check for presence of a hide animation
          Storyboard showAnim = element.Resources[animationName] as Storyboard;
          if (showAnim != null)
          {
            showAnim.Begin();
          }
          else
          {
            element.Visibility = Visibility.Collapsed;
          }
        }
  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DataTable GetDataTable(string excelQL, string connectionString)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(excelQL, conn))
                    {
                        using (OleDbDataReader rdr = cmd.ExecuteReader())
                        {
                            dt.Load(rdr);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }
    }
}

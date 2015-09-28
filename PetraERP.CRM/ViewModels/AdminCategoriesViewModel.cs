using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PetraERP.Shared.Models;
using System.Windows.Input;
using PetraERP.Shared.UI;
using PetraERP.CRM.Views;
using MahApps.Metro;
using System.Windows.Controls;
using System.Windows;


namespace PetraERP.CRM.ViewModels
{
    public class AdminCategoriesViewModel : ViewModelBase 
    {
        #region Private Members

        private crmCategoryView _category;
        private IEnumerable<crmCategoryView> _categories;

        #endregion

        #region Public Properties

        public crmCategoryView SelectedCategory
        {
            get { return _category; }
            set
            {
                if (value == _category)
                    return;
                _category = value;
            }
        }

        public IEnumerable<crmCategoryView> Categories
        {
            get { return _categories; }
            set
            {
                if (value == _categories)
                    return;
                _categories = value;
                OnPropertyChanged(GetPropertyName(() => Categories));
                OnPropertyChanged(GetPropertyName(() => CategoriesCount));
            }
        }

        public string CategoriesCount
        {
            get { return string.Format("{0} Categories", _categories.Count()); }
            private set { ; } 
        }

        #endregion

        #region Commands

        public ICommand OpenCategoryCommand 
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            AdminCategoriesEditView win = new AdminCategoriesEditView(SelectedCategory.Id);
                            win.Closed += window_ClosingFinished;
                            win.ShowDialog();
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }

        public ICommand AddCategoryCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        try
                        {
                            AdminCategoriesAddView win = new AdminCategoriesAddView();
                            win.Closed += categorywindow_ClosingFinished;
                            win.ShowDialog();
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }

        #endregion
        
        #region Constructor

        public AdminCategoriesViewModel() 
        {
            try
            {
                UpdateCategoryGrid();
            }
            catch (Exception ex)
            {
                AppData.MessageService.ShowMessage(ex.GetBaseException().ToString());
            }
        }

        #endregion

        #region Private Methods

        private void categorywindow_ClosingFinished(object sender, EventArgs e)
        {
            UpdateCategoryGrid();
        }

        private void UpdateCategoryGrid()
        {
            try
            {                
                // Get items
                Categories = CrmData.get_Categories();
            }
            catch (Exception err)
            {
                AppData.MessageService.ShowMessage(err.Message);
            }
        }

        #endregion
    }
}

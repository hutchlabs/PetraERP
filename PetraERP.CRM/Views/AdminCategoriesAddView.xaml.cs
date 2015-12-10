using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PetraERP.Shared.Datasources;
using PetraERP.Shared.Models;

namespace PetraERP.CRM.Views
{
    /// <summary>
    /// Interaction logic for pageCategories.xaml
    /// </summary>
    public partial class AdminCategoriesAddView : MetroWindow
    {
        private bool isUpdate = false;

        public AdminCategoriesAddView()
        {
            InitializeComponent();
        }

        private bool create_new_category()
        {
            try
            {
                category newCat = new category();
                newCat.category_name = txtCategoryName.Text;
                newCat.code = txtCode.Text;
                newCat.description = txtDescription.Text;
                newCat.owner = 0;
                newCat.created_at = DateTime.Now;
                Database.CRM.categories.InsertOnSubmit(newCat);
                Database.CRM.SubmitChanges();
                return true;
            }
            catch (Exception newCatError)
            {
                MessageBox.Show(newCatError.Message);
                return false;
            }

        }

        private bool update_category()
        {
            try
            {
                var cat = from c in Database.CRM.categories
                             where c.id == int.Parse(txtCategoryID.Text)
                             select c;

                foreach (category c in cat)
                {
                    c.category_name = txtCategoryName.Text;
                    c.code = txtCode.Text;
                    c.description = txtDescription.Text;
                    c.updated_at = DateTime.Now;
                    c.modified_by = 0;
                    Database.CRM.SubmitChanges();
                }
                return true;
            }
            catch (Exception newCatError)
            {
                MessageBox.Show(newCatError.Message);
                return false;
            }
        }

        private void clear_entries()
        {
            try
            {
                isUpdate = false;
                txtCategoryID.Text = "Auto";
                txtCode.Text = string.Empty;
                txtCategoryName.Text = string.Empty;
                txtDescription.Text = string.Empty;
            }
            catch (Exception) { MessageBox.Show("An error occured while setting up from."); }
        }

        private bool validate_entries()
        {
            try
            {
                if (txtCategoryName.Text == string.Empty) { MessageBox.Show("Please specify the name of the category you want to create", "No category name", MessageBoxButton.OK, MessageBoxImage.Stop); txtCategoryName.Focus(); return false; }
                else if (txtCode.Text == string.Empty) { MessageBox.Show("Please specify the code of the category you want to create", "No category code", MessageBoxButton.OK, MessageBoxImage.Stop); txtCode.Focus(); return false; }
                else { return true; }
            }
            catch (Exception) { return false; }
        }

        private void load_category_details(int id)
        {
            try
            {
                var cat = CrmData.get_Categories(id);
                txtCategoryID.Text = cat.Id.ToString();
                txtCategoryName.Text = cat.Name;
                txtDescription.Text = cat.description;
                txtCode.Text = cat.code;
                isUpdate = true;
            }
            catch(Exception)
            {
                MessageBox.Show("An error occured while loading category details.");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            clear_entries();
            load_categories();
        }

        private void load_categories()
        {
            dgCategories.ItemsSource = CrmData.get_Categories();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (validate_entries())
            {
                if (!isUpdate)
                {
                    if (create_new_category()) { MessageBox.Show("New Category Created Successfully.", "New Record Added", MessageBoxButton.OK, MessageBoxImage.Information); clear_entries(); load_categories(); }
                }
                else
                {
                    if (update_category()) { MessageBox.Show("Category Updated Successfully.", "Record Updated", MessageBoxButton.OK, MessageBoxImage.Information); clear_entries(); load_categories(); }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit this form?", "Exit this form", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            { this.Close(); }
        }

        private void dgCategories_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                crmCategoryView cats = (crmCategoryView)dgCategories.SelectedItem;
                load_category_details(cats.Id);
            }
            catch(Exception)
            {
                //
            }
        }

        
    }
}

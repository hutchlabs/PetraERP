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
using MahApps.Metro.Controls;
using PetraERP.Shared.Datasources;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;

namespace PetraERP.CRM.Views
{
    public partial class TicketsAddView : MetroWindow
    {
        private bool isCompany = false;
        private string _petraId = "";
        private bool _spinner = false;

        public bool Spinner
        {
            get { return _spinner; }
            set
            {
                if (_spinner == value)
                    return;
                _spinner = value;
            }
        }

        public TicketsAddView()
        {
            InitializeComponent();
            this.DataContext = this;

            tbCustomerInfo.NextPage = tbTicketInfo;
            tbCompanyInfo.NextPage = tbTicketInfo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            load_categories();
            load_users();
        }

        private void PageChanged(object sender, AvalonWizard.CurrentPageChangedEventArgs e)
        {
            if (AddTicketWizard.CurrentPageIndex == 1 || AddTicketWizard.CurrentPageIndex == 2)
            {
                AddTicketWizard.NextButtonContent = "Go to Create Ticket";
                AddTicketWizard.BackButtonContent = "Go back to Search Results";
            }
            else if (AddTicketWizard.CurrentPageIndex == 3)
            {
                AddTicketWizard.FinishButtonContent = "Create Ticket";
                AddTicketWizard.BackButtonContent = "Go Back";
            }
            else
            {
                AddTicketWizard.NextButtonContent = "Next";
                AddTicketWizard.BackButtonContent = "Back";
            }
        }

        private async void CloseWin(bool check=true)
        {
            if (check)
            {
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes, cancel",
                    NegativeButtonText = "No, continue with ticket creation",
                    AnimateShow = true,
                    AnimateHide = false
                };

                var result = await this.ShowMessageAsync("Quit Creating ticket?",
                                                         "Sure you want to cancel creating a ticket?",
                                                          MessageDialogStyle.AffirmativeAndNegative, mySettings);

                if (result == MessageDialogResult.Affirmative)
                {
                    this.Closing -= CloseWindow;
                    this.Close();
                }
            }
            else
            {
                this.Closing -= CloseWindow;
                this.Close();
            }
        }

        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            CloseWin();
        }

        private void AddTicketWizard_Cancelled(object sender, RoutedEventArgs e)
        {
            CloseWin();
        }


        private void clear_existing_details()
        {

            txtPetraID.Text = string.Empty;
            txtCustomerPetraID.Text = string.Empty;
            txtSSN.Text = string.Empty;
            txtFirstName.Text = string.Empty;
            txtMiddleNames.Text = string.Empty;
            txtSurname.Text = string.Empty;
            txtCustomerName.Text = string.Empty;
            txtTicketEmail.Text = string.Empty;
            txtTicketContactNo.Text = string.Empty;
            cmbEmployers.ItemsSource = null;
        }

        public ICommand SearchButtonCmd
        {
            get
            {
                return new SimpleCommand { CanExecuteDelegate = x => true, ExecuteDelegate = x => { search_for_customer(); }};
            }
        }

        // Step 1
        private void search_for_customer()
        {
            Spinner = true;
            if (txtSearch.Text != string.Empty)
            {
                if (rdPetraID.IsChecked == true) { dgFoundRecords.ItemsSource = CrmData.search_customer_by_petra_id(txtSearch.Text); isCompany = false; }
                else if (rdCustomerName.IsChecked == true) { dgFoundRecords.ItemsSource = CrmData.search_customer_by_name(txtSearch.Text); isCompany = false; }
                else if (rdSSNITNo.IsChecked == true) { dgFoundRecords.ItemsSource = CrmData.search_customer_by_ssnit_no(txtSearch.Text); isCompany = false; }
                else if (rdCompanyName.IsChecked == true) { dgFoundRecords.ItemsSource = CrmData.search_companies_by_name(txtSearch.Text); isCompany = true; }
                lblRecordsFound.Content = dgFoundRecords.Items.Count.ToString()+" Records Found";
            }
            else
            {
                MessageBox.Show("Please specify a search value.");
            }
            Spinner = false;
        }

        // Step 2
        private void dgFoundRecords_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (isCompany) 
                {
                    crmCompanyList comp = (crmCompanyList)dgFoundRecords.SelectedItem;
                    get_company_details(comp.petra_id);
                    _petraId = comp.petra_id;
                    AddTicketWizard.NextPageByIndex(2);
                }
                else 
                {
                    crmCustomerDetails cust = (crmCustomerDetails)dgFoundRecords.SelectedItem;
                    get_customer_details(cust.Petra_ID);
                    _petraId = cust.Petra_ID;
                    AddTicketWizard.NextPageByIndex(1);
                }    
            }
            catch (Exception ex)
            {
                AppData.MessageService.ShowMessage(ex.Message, "Error retrieving information.",Shared.UI.MessagingService.DialogType.Error);
            }
        }

        // Step 2 a
        private void get_customer_details(string petra_id)
        {
            int entity_id = -1;

            try
            {
                clear_existing_details();

                var cust_info = CrmData.get_customer_by_petra_id(petra_id);
                entity_id = cust_info.Entity_ID;

                txtPetraID.Text = cust_info.Petra_ID;
                txtCustomerPetraID.Text = cust_info.Petra_ID;
                txtSSN.Text = cust_info.SSNIT_No;
                txtFirstName.Text = cust_info.First_Name;
                txtMiddleNames.Text = cust_info.Second_Name;
                txtSurname.Text = cust_info.Last_Name;
                txtCustomerName.Text = cust_info.First_Name + " " + cust_info.Second_Name + " " + cust_info.Last_Name;
                txtTicketSubject.Text = cust_info.First_Name + " " + cust_info.Second_Name + " " + cust_info.Last_Name+" ()";
             }
             catch (Exception e)
             {
               MessageBox.Show("Unable to load customer details: "+e.Message);
             }

            try
            {
                //CustomerPreviousTickets.Items.Clear();
                CustomerPreviousTickets.ItemsSource = null;
                CustomerPreviousTickets.ItemsSource = CrmData.get_customer_active_tickets(petra_id);
            }
            catch(Exception)
            {
                MessageBox.Show("A new set of ticket history will be loaded for the selected client.");
            }


            //Get customer emails
            try
            {
                var cust_emails = CrmData.get_customer_emails(entity_id);

                cmbEmails.Items.Clear();

                foreach (var emp in cust_emails)
                {
                    cmbEmails.Items.Add(new KeyValuePair<string, string>(emp.Email, emp.Email));
                }
                if (cmbEmails.Items.Count > 0) { cmbEmails.SelectedIndex = 0; }
            }
            catch (Exception)
            {
                MessageBox.Show("No email address found.");
            }


            //Get customer contact #s
            try
            {
                var cust_contact_nos = CrmData.get_customer_contact_nos(entity_id);

                cmbContactNo.Items.Clear();

                foreach (var emp in cust_contact_nos)
                {
                    cmbContactNo.Items.Add(new KeyValuePair<string, string>(emp.Contact_No, emp.Contact_No));
                }
                if (cmbContactNo.Items.Count > 0) { cmbContactNo.SelectedIndex = 0; }
            }
            catch (Exception)
            {
                MessageBox.Show("No contact nos found.");
            }    


            try
            {
                var cust_employers = CrmData.get_customer_employers(entity_id);

                cmbEmployers.Items.Clear();

                foreach (var emp in cust_employers)
                {
                    cmbEmployers.Items.Add(new KeyValuePair<string, string>(emp.Employer_Name, emp.Employer_Name));
                }
                if (cmbEmployers.Items.Count > 0) { cmbEmployers.SelectedIndex = 0; }        
            }
            catch(Exception)
            {
                MessageBox.Show("Employer details not found.");
            }            
        }

        // Step 2 b
        private void get_company_details(string petra_id)
        {
            var comp_info = CrmData.get_company_by_petra_id(petra_id);
            try
            {
                txtCompanyPetraID.Text = comp_info.petra_id;
                txtPetraID.Text = comp_info.petra_id;
                txtCompanyName.Text = comp_info.company_name;
                txtCustomerName.Text = comp_info.company_name;
                txtTicketSubject.Text = comp_info.company_name+" ()";
                txtCompanyRegNo.Text = comp_info.bus_reg_num;
                CompanyPreviousTickets.Items.Clear();
                CompanyPreviousTickets.ItemsSource = null;
                CompanyPreviousTickets.ItemsSource = CrmData.get_customer_active_tickets(petra_id);
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to load company details. "+e.Message);
            }
        }

        // Step 3
        private void create_ticket(object sender, RoutedEventArgs e)
        {
            if (validate())
            {
                get_ticket_code();
                String conf_msg = "The ticket will be created with the following details:\n\n Ticket ID : " + txtTicketID.Text + " \n Subject : " + txtTicketSubject.Text + "";
                if (MessageBox.Show(conf_msg, "Creating Ticket", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK) { if (save_ticket()) { CloseWin(false); } }
            }
        }


        private int get_priority()
        {
            if (rdPriorityLow.IsChecked == true) { return 0; }
            else if (rdPriorityMedium.IsChecked == true) { return 1; }
            else if (rdPriorityHigh.IsChecked == true) { return 2; }
            else { return -1; };
        }

        private bool validate()
        {
            try
            {
                if (txtPetraID.Text == string.Empty) { MessageBox.Show("No Petra ID found for this ticket. Ticket can not be created without an ID.", "No Petra ID", MessageBoxButton.OK, MessageBoxImage.Exclamation); return false; }
                else if (txtPetraID.Text == string.Empty) { MessageBox.Show("No Petra ID found for this ticket. Ticket can not be created without an ID.", "No Petra ID", MessageBoxButton.OK, MessageBoxImage.Exclamation); return false; }
                else if (txtTicketSubject.Text == string.Empty) { MessageBox.Show("No subject found for this ticket. Ticket can not be created without a subject.", "No Subject", MessageBoxButton.OK, MessageBoxImage.Exclamation); return false; }
                else if (cmbTicketCategory.SelectedIndex < 0) { MessageBox.Show("Please select a department for this ticket. Ticket can not be created without a department.", "No Department", MessageBoxButton.OK, MessageBoxImage.Exclamation); return false; }
                else if (cmbTicketCorrespondence.SelectedIndex < 0) { MessageBox.Show("Please select a category for this ticket. Ticket can not be created without a category.", "No Category", MessageBoxButton.OK, MessageBoxImage.Exclamation); return false; }
                else if (cmbTicketSubCorrespondence.SelectedIndex < 0) { MessageBox.Show("Please select a request type for this ticket. Ticket can not be created without a request type.", "No Request Type", MessageBoxButton.OK, MessageBoxImage.Exclamation); return false; }
                else { return true; }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void get_ticket_code()
        {
            var cat_code = CrmData.get_Categories(int.Parse(cmbTicketCategory.SelectedValue.ToString()));
            var corres_code = CrmData.get_Correspondence(int.Parse(cmbTicketCorrespondence.SelectedValue.ToString()));
            var sub_corres_code = CrmData.get_Sub_Correspondence(int.Parse(cmbTicketSubCorrespondence.SelectedValue.ToString()));

            string sequence = CrmData.get_ticket_seqence_no(cat_code.Id, corres_code.Id, sub_corres_code.Id, DateTime.Now.Month, DateTime.Now.Year);
            txtTicketID.Text = cat_code.code + corres_code.code + sub_corres_code.code + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + sequence;
        }

        private bool save_ticket()
        {
            try
            {
                ticket newTicket = new ticket();
                newTicket.ticket_id = txtTicketID.Text;
                newTicket.customer_id = txtPetraID.Text;
                if (isCompany) { newTicket.customer_id_type = 1; } else { newTicket.customer_id_type = 0; }
                newTicket.ticket_priority = get_priority();
                newTicket.subject = txtTicketSubject.Text;
                newTicket.category_id = int.Parse(cmbTicketCategory.SelectedValue.ToString());
                newTicket.correspondence_id = int.Parse(cmbTicketCorrespondence.SelectedValue.ToString());
                newTicket.sub_correspondence_id = int.Parse(cmbTicketSubCorrespondence.SelectedValue.ToString());
                newTicket.contact_no = txtTicketContactNo.Text;
                newTicket.email = txtTicketEmail.Text;
                newTicket.notes = txtNotes.Text;
                newTicket.ticket_month = DateTime.Now.Month;
                newTicket.ticket_year = DateTime.Now.Year;
                newTicket.status = 1;
                newTicket.owner = Users.GetCurrentUser().id;
                newTicket.assigned_to = int.Parse(cmbAssignTo.SelectedValue.ToString());
                newTicket.created_at = DateTime.Now;
                Database.CRM.tickets.InsertOnSubmit(newTicket);
                Database.CRM.SubmitChanges();

                string t = string.Format("Ticket {0} Added",newTicket.ticket_id);
                
                Notification.Add((int)newTicket.assigned_to, t, PetraERP.Shared.Constants.JOB_TYPE_TICKET, newTicket.id);

                return true;
            }
            catch (Exception newSlaError)
            {
                MessageBox.Show(newSlaError.Message);
                return false;
            }
        }

        private void load_users()
        {
            try
            {
                cmbAssignTo.Items.Clear();
                IEnumerable<ERP_User> users = Users.GetApplicationUsers();

                foreach (var u in users)
                {
                    if (u.status)
                    {
                        cmbAssignTo.Items.Add(new ComboBoxPairs(u.id.ToString(), u.first_name + " " + u.last_name));
                    }
                }
            } catch(Exception e)
            {
                AppData.MessageService.ShowMessage(e.Message);
            }
        }

        private void load_categories()
        {
            cmbTicketCategory.Items.Clear();
            var cats = CrmData.get_Active_Categories();

            foreach (var inicat in cats)
            {
                cmbTicketCategory.Items.Add(new KeyValuePair<string, int>(inicat.Name, inicat.Id));
            }
        }

        private void load_correspondences(int cat_id)
        {
            cmbTicketCorrespondence.Items.Clear();
            var corres = CrmData.get_Active_Correspondence_Filter_By_Category(cat_id);

            foreach (var inicorres in corres)
            {
                cmbTicketCorrespondence.Items.Add(new KeyValuePair<string, int>(inicorres.Name, inicorres.Id));
            }

        }

        private void load_sub_correspondences(int corres_id)
        {
            cmbTicketSubCorrespondence.Items.Clear();
            var sub_corres = CrmData.get_Active_Sub_Correspondence_Filter_By_Correspondence(corres_id);

            foreach (var inicorres in sub_corres)
            {
                cmbTicketSubCorrespondence.Items.Add(new KeyValuePair<string, int>(inicorres.Name, inicorres.Id));
            }
        }

        private void cmbTicketCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try 
            {
                if (cmbTicketCategory.SelectedIndex >= 0)
                {
                    load_correspondences(int.Parse(cmbTicketCategory.SelectedValue.ToString()));
                }
                else
                { 
                    cmbTicketCorrespondence.Items.Clear();
                }
            }
            catch(Exception)
            {
                cmbTicketCorrespondence.Items.Clear();
            }
        }


        private void generateDefSubject()
        {
            string [] sel = (string [])cmbTicketSubCorrespondence.SelectedItem.ToString().Split(',');
            txtTicketSubject.Text = txtTicketSubject.Text.Substring(0, txtTicketSubject.Text.IndexOf('(')) + "(" + sel[0].Substring(1) + ")";
        }

        private void cmbTicketSubCorrespondence_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTicketSubCorrespondence.SelectedIndex >= 0)
                {
                    generateDefSubject();
                    var sub_corres = CrmData.get_Sub_Correspondence(int.Parse(cmbTicketSubCorrespondence.SelectedValue.ToString()));
                    var sla = CrmData.get_SLAs_By_Name_View(sub_corres.SLA);
                    txtAssocaitedSLA.Text = sla.Name + " TIMERS [Pre Escalate : " + sla.code + "] [Escalate : " + sla.Escalated + "]";
                }
                else
                { 
                    txtAssocaitedSLA.Text = string.Empty;
                }
            }
            catch (Exception)
            {
                txtAssocaitedSLA.Text = string.Empty;
            }
        }

        private void cmbTicketCorrespondence_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTicketCorrespondence.SelectedIndex >= 0)
                {
                    load_sub_correspondences(int.Parse(cmbTicketCorrespondence.SelectedValue.ToString()));
                }
                else
                { 
                    cmbTicketSubCorrespondence.Items.Clear();
                }
            }
            catch (Exception)
            {
                cmbTicketSubCorrespondence.Items.Clear();
            }
        }

        private void do_search(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                search_for_customer();
            }
        }

        private void open_ticket(string ticket_id)
        {
            try
            {
                TicketsEditView win = new TicketsEditView(ticket_id);
                win.Closed += window_ClosingFinished;
                win.ShowDialog();
            }
            catch (Exception err)
            {
                AppData.MessageService.ShowMessage(err.Message);
            }
        }

        private void window_ClosingFinished(object sender, EventArgs e)
        {
            if (isCompany)
            {
                CompanyPreviousTickets.ItemsSource = CrmData.get_customer_active_tickets(_petraId);
            }
            else
            {
                CustomerPreviousTickets.ItemsSource = CrmData.get_customer_active_tickets(_petraId);
            }
        }

        private void CompanyPreviousTickets_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            crmTicketsView c = (crmTicketsView) CompanyPreviousTickets.SelectedItem;
            if (c != null)
            {
                open_ticket(c.ticket_id);
            }
        }

        private void CustomerPreviousTickets_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            crmTicketsView c = (crmTicketsView)CustomerPreviousTickets.SelectedItem;
            if (c != null)
            {
                open_ticket(c.ticket_id);
            }
        }
    }
}

using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PetraERP.Shared.Datasources;
using PetraERP.Shared.Models;
using PetraERP.Shared.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;


namespace PetraERP.CRM.Views
{
    public partial class TicketsEditView : MetroWindow
    {
        private string _ticketID;
        private IEnumerable<crmTicketStatus> _statuses = CrmData.get_allowed_ticket_status();

        private MessageCollection _messages;
        private Storyboard scrollViewerStoryboard;
        private DoubleAnimation scrollViewerScrollToEndAnim;

        public MessageCollection Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }
        public IEnumerable<crmTicketStatus> TicketStatuses
        {
            get { return _statuses; }
            private set { ; }
        }

        public TicketsEditView(string ticketID)
        {
            InitializeComponent();
            _ticketID = ticketID;
            load_ticket_details();

            this.DataContext = this;

            scrollViewerScrollToEndAnim = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(scrollViewerScrollToEndAnim, this);
            Storyboard.SetTargetProperty(scrollViewerScrollToEndAnim, new PropertyPath(VerticalOffsetProperty));

            scrollViewerStoryboard = new Storyboard();
            scrollViewerStoryboard.Children.Add(scrollViewerScrollToEndAnim);
            this.Resources.Add("foo", scrollViewerStoryboard);
        }


        private void load_ticket_details()
        {
            var ticket_data = CrmData.get_ticket_details(_ticketID);

            if(ticket_data.customer_type == 0)
            {
                var microgen_data = CrmData.search_cust_by_petra_id(ticket_data.petra_id);
               lblTicketName.Content = microgen_data.Customer_Name;
            }
            else if (ticket_data.customer_type == 1)
            {
                var microgen_data = CrmData.get_company_by_petra_id(ticket_data.petra_id);
                lblTicketName.Content= microgen_data.company_name;
            }

            lblTicketInfo.Content = string.Format("    Ticket ID: {0}\n          Date: {1:ddd dd MMM yyyy HH:mm tt}\n       Owner: {2}",
                                                  ticket_data.ticket_id, DateTime.Parse(ticket_data.ticket_date), ticket_data.owner);

            lblTicketInfo2.Content = string.Format("                   Department: {0}\n                       Category: {1}\n                Request Type: {2}\n",
                                                   ticket_data.category, ticket_data.correspondence,  ticket_data.subcorrespondence);

            lblesca.Content = "Escalation Due: " + ticket_data.esacalation;

            lblIssue.Content = "Issue - " + ticket_data.subject;

            lblContact.Content = "Contact No. : " + ticket_data.contact_no != string.Empty ? ticket_data.contact_no : "None";

            lblEmail.Content = "Email : " + ticket_data.email != string.Empty ? ticket_data.email : "None";

            tbIssue.Text = ticket_data.notes;

            cbxStatus.SelectedValue = ticket_data.status_id;

           

            load_ticket_comments();
        }

        private void load_ticket_comments()
        {
            var comms = CrmData.get_ticket_comments(_ticketID);
            _messages = new MessageCollection();
            foreach (var comm in comms)
            {
                _messages.Add(new Message { Side = MessageSide.You, Author=comm.owner, Text = comm.comment, Timestamp=DateTime.Parse(comm.comment_date) });
            }
            conversationView.DataContext = _messages;
            lblC.Content = string.Format("Comments ({0})", _messages.Count());
            txtComment.Text = "";
        }

        private bool validate()
        {
            if (txtComment.Text == string.Empty) { MessageBox.Show("Please add your comment.", "comment Required", MessageBoxButton.OK, MessageBoxImage.Exclamation); txtComment.Focus(); return false; }
            else { return true; }
        }

        private bool update_ticket(int status)
        {
            try
            {
                IEnumerable<ticket> tic = (from t in Database.CRM.tickets
                           where t.ticket_id == _ticketID
                           select t);

                foreach (var t in tic)
                {
                    t.status = status;
                    t.updated_at = DateTime.Now;
                    t.modified_by = AppData.CurrentUser.id;
                    Database.CRM.SubmitChanges();
                }
                return true;
            }
            catch (Exception updateError)
            {
                MessageBox.Show(updateError.Message);
                return false;
            }
        }

        private void post_comment(int status)
        {
            try
            {
                //if(validate())
                //{
                    if (update_ticket(status))
                    {
                        var ticket_data = CrmData.get_ticket_details(_ticketID);

                        if (status==6)
                        {
                            Notification.AddToRole(11, "CRM Ticket ON HOLD Request " + _ticketID, PetraERP.Shared.Constants.JOB_TYPE_TICKET, ticket_data.id);
                        }
                        if (txtComment.Text != "")
                        {
                            ticket_comment newComment = new ticket_comment();
                            newComment.ticket_id = _ticketID;
                            newComment.comment_date = DateTime.Now;
                            newComment.status = "1".ToString();
                            newComment.owner = AppData.CurrentUser.id;
                            newComment.comment = txtComment.Text;
                            Database.CRM.ticket_comments.InsertOnSubmit(newComment);
                            Database.CRM.SubmitChanges();
                            load_ticket_comments();


                            if (ticket_data.assigned_to != AppData.CurrentUser.id)
                            {
                                Notification.Add(ticket_data.assigned_to, "CRM: Ticket Comment " + _ticketID, PetraERP.Shared.Constants.JOB_TYPE_TICKET, ticket_data.id);
                            }
                        }
                    }
                    else
                    { MessageBox.Show("Your comment was not posted."); }
                //}
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void btnPostComment_Click(object sender, RoutedEventArgs e)
        {
            post_comment(1);
        }


        private async void cbxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             crmTicketStatus s = (crmTicketStatus)cbxStatus.SelectedItem;

             if (s == null)
                 return;

             if (this.IsLoaded)
             {
                 var result = await this.ShowInputAsync("Change Status to "+s.status+"?", "Comment");

                  //if (result == null) //user pressed cancel
                  //    return;

                  //if (result.Length > 0)
                  //{
                      bool success = false;
                      if (result != null && result != "") { txtComment.Text = result; }
                      try
                      {
                          if (s.status == "RESOLVED") { post_comment(4); }
                          else if (s.status == "ON HOLD") { post_comment(2); }
                          else if (s.status == "ON HOLD WAITING APPROVAL") { 
                              post_comment(6);

                              
                          }
                          success = true;
                      }
                      catch (Exception) { success = false; }

                      if (success)
                      {
                          await this.ShowMessageAsync("Ticket Status Changed","The status of the ticket has been set to "+s.status);
                          if (s.status == "RESOLVED" || s.status == "ON HOLD" || s.status == "ON HOLD WAITING APPROVAL")
                          {
                              this.Close();
                          }
                      }
                      else
                      {
                          await this.ShowMessageAsync("Ticket Status Error", "An error occured. Please retry.");
                          cbxStatus_SelectionChanged(sender, e);
                      }
                  //}
             }
        }
     
        private void txtComment_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                post_comment(1);
            }
        }

        private void txtComment_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConversationContentContainer.ActualHeight < ConversationScrollViewer.ActualHeight)
            {
                PaddingRectangle.Show(() => ScrollConversationToEnd());
            }
            else
            {
                ScrollConversationToEnd();
            }
        }

        private void txtComment_LostFocus(object sender, RoutedEventArgs e)
        {
            PaddingRectangle.Hide();
            ScrollConversationToEnd();      
        }

        
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(TicketsEditView), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TicketsEditView app = d as TicketsEditView;
            app.OnVerticalOffsetChanged(e);
        }

        private void OnVerticalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            ConversationScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        private void ScrollConversationToEnd()
        {
            scrollViewerScrollToEndAnim.From = ConversationScrollViewer.VerticalOffset;
            scrollViewerScrollToEndAnim.To = ConversationContentContainer.ActualHeight;
            scrollViewerStoryboard.Begin();
        }

  
    }
}

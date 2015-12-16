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
    public class TicketsViewModel : ViewModelBase 
    {
        #region Private Members

        private crmTicketsView _ticket;

        private IEnumerable<crmTicketsView> _tickets;

        private string _searchValue;

        private string _filterValue;

        private readonly string[] _ticketsFilterOptions = { Constants.STATUS_OPEN_ESCALATED,  Constants.STATUS_ALL, Constants.TICKET_STATUS_OPEN,
                                                            Constants.TICKET_STATUS_ON_HOLD, Constants.TICKET_STATUS_ON_HOLD_WAITING, Constants.TICKET_STATUS_ESCALATED,
                                                            Constants.TICKET_STATUS_RESOLVED};

        #endregion

        #region Public Properties

        public crmTicketsView SelectedTicket
        {
            get { return _ticket; }
            set
            {
                if (value == _ticket)
                    return;
                _ticket = value;
            }
        }

        public IEnumerable<crmTicketsView> Tickets
        {
            get { return _tickets; }
            set
            {
                if (value == _tickets)
                    return;
                _tickets = value;
                OnPropertyChanged(GetPropertyName(() => Tickets));
                OnPropertyChanged(GetPropertyName(() => TicketsCount));
            }
        }

        public string TicketsCount
        {
            get { return string.Format("{0} Tickets", _tickets.Count()); }
            private set { ; } 
        }

        public string TicketsFilterValue
        {
            get { return _filterValue ; }
            set { _filterValue = value; }
        }

        public string[] TicketsFilterOptions
        {
            get { return _ticketsFilterOptions; }
            private set { ; }
        }

        public string SearchValue
        {
            get { return _searchValue; }
            set {         
                _searchValue = value;
                OnPropertyChanged(GetPropertyName(() => SearchValue));  
            }
        }

        #endregion

        #region Commands

        public ICommand OpenTicketCommand 
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
                            if (SelectedTicket.status == "RESOLVED")
                            {
                                AppData.MessageService.ShowMessage("Please note: this ticket has already been resolved.", Shared.UI.MessagingService.DialogType.Message);
                            }

                            TicketsEditView win = new TicketsEditView(SelectedTicket.ticket_id);
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

        public ICommand AddTicketCommand
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
                            TicketsAddView win = new TicketsAddView();
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

        public ICommand SearchCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        doSearch();
                    }
                };
            }
        }

        public ICommand FilterTicketsCommand
        {
            get
            {
                return new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        UpdateTicketGrid();
                    }
                };
            }
        }

        public ActionCommand<KeyEventArgs> SearchKeyUpCommand
        {
            get { return new ActionCommand<KeyEventArgs>(OnKeyUp); }
        }

        public ActionCommand<SelectionChangedEventArgs> FilterTicketsSelectionCommand
        {
            get { return new ActionCommand<SelectionChangedEventArgs>(OnFilterSelect); }
        }

        #endregion
        
        #region Constructor

        public TicketsViewModel() 
        {
            try
            {
                UpdateTicketGrid();
            }
            catch (Exception ex)
            {
                AppData.MessageService.ShowMessage(ex.GetBaseException().ToString());
            }
        }

        #endregion

        #region Private Methods
        
        private void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                doSearch();
            }
        }

        private void OnFilterSelect(SelectionChangedEventArgs e)
        {
            UpdateTicketGrid();
        }

        private void window_ClosingFinished(object sender, EventArgs e)
        {
            UpdateTicketGrid();
        }

        private void doSearch()
        {
            try
            {
                if (SearchValue != "")
                {
                    IEnumerable<crmTicketsView> t = CrmData.search_tickets(SearchValue, GetFilterStatusCode());
                    if (t == null || t.Count() <= 0)
                    {
                        AppData.MessageService.ShowMessage("No results found for '" + SearchValue + "'");
                    }
                    else
                    {
                        Tickets = t;
                    }
                }
                else
                {
                    UpdateTicketGrid();
                    AppData.MessageService.ShowMessage("Please enter a search term.");
                }
            }
            catch (Exception err)
            {
                AppData.MessageService.ShowMessage(err.Message);
            }
        }

        private void UpdateTicketGrid()
        {
            try
            {
                 // Get items
                Tickets = CrmData.get_active_tickets(GetFilterStatusCode());
            }
            catch (Exception err)
            {
                AppData.MessageService.ShowMessage(err.Message);
            }
        }

        private int GetFilterStatusCode()
        {
            int code;
            if (TicketsFilterValue == Constants.TICKET_STATUS_OPEN) { code = 1; }
            else if (TicketsFilterValue == Constants.TICKET_STATUS_ON_HOLD) { code = 2; }
            else if (TicketsFilterValue == Constants.TICKET_STATUS_ESCALATED) { code = 3; }
            else if (TicketsFilterValue == Constants.TICKET_STATUS_RESOLVED) { code = 4; }
            else if (TicketsFilterValue == Constants.TICKET_STATUS_DEACTIVATED) { code = 5; }
            else if (TicketsFilterValue == Constants.TICKET_STATUS_ON_HOLD_WAITING) { code = 6; }
            else if (TicketsFilterValue == Constants.STATUS_OPEN_ESCALATED) { code = -1; }
            else if (TicketsFilterValue == Constants.STATUS_ALL) { code = 0; }
            else { code = -1; }
            return code;
        }

        #endregion
    }
}

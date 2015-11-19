
using PetraERP.Shared.Datasources;
using PetraERP.Shared.Models;
using PetraERP.Shared.UI;
using PetraERP.Shared.UI.MessagingService;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace PetraERP.CRM.ViewModels
{
    public partial class SubCorrespondenceViewModel : ViewModelBase
    {
        #region Private Members
        
        private string _title = "Welcome";
        private int _selectedIdx = 0;
        private bool _isUpdate = true;
        private Visibility _showCancel = Visibility.Collapsed;


        private crmCorrespondenceView _correspondence;
        private IEnumerable<crmCorrespondenceView> _correspondences = CrmData.get_Correspondence();

        private crmSubCorrespondenceView _subcorrespondence;
        private crmSLAView _scsla;
        private IEnumerable<crmSubCorrespondenceView> _subcorrespondences = CrmData.get_Sub_Correspondence();
        
        private crmSLAView _sla;
        private IEnumerable<crmSLAView> _slas = CrmData.get_SLAs_View();

        #endregion

        #region Public Properties

        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(GetPropertyName(() => Title)); }
        }

        public int SelectedIdx
        {
            get { return _selectedIdx; }
            set
            {
                _selectedIdx = value;
                OnPropertyChanged(GetPropertyName(() => SelectedIdx));
            }
        }
 
        public Visibility ShowCancel 
        { 
            get { return _showCancel; } 
            set 
            {
                _showCancel = value;
                OnPropertyChanged(GetPropertyName(() => ShowCancel));
            } 
        }

        public crmCorrespondenceView SelectedCorrespondence
        {
            get { return _correspondence; }
            set
            {
                if (value == _correspondence)
                    return;
                _correspondence = value;
                
                reset();

                if (_correspondence != null)
                    Title = "Editing " + _correspondence.Name;

                OnPropertyChanged(GetPropertyName(() => SelectedCorrespondence));
            }
        }

        public IEnumerable<crmCorrespondenceView> Correspondences
        {
            get { return _correspondences; }
            set
            {
                if (value == _correspondences)
                    return;
                _correspondences = value;
                OnPropertyChanged(GetPropertyName(() => Correspondences));
                OnPropertyChanged(GetPropertyName(() => CorrespondencesCount));
            }
        }

        public string CorrespondencesCount
        {
            get { return string.Format("{0} Categories", _correspondences.Count()); }
            private set { ; }
        }


        public crmSubCorrespondenceView SelectedSubCorrespondence
        {
            get { return _subcorrespondence; }
            set
            {
                if (value == _subcorrespondence)
                    return;
                _subcorrespondence = value;

                reset();

                if (_subcorrespondence != null)
                    Title = "Editing " + _subcorrespondence.Name;

                OnPropertyChanged(GetPropertyName(() => SelectedSubCorrespondence));
            }
        }

        public crmSLAView SelectedSCSLA
        {
            get { return _scsla; }
            set
            {
                if (value == _scsla)
                    return;
                _scsla = value;

                OnPropertyChanged(GetPropertyName(() => SelectedSCSLA));
                OnPropertyChanged(GetPropertyName(() => SLAs));

            }
        }

        public IEnumerable<crmSubCorrespondenceView> SubCorrespondences
        {
            get { return _subcorrespondences; }
            set
            {
                if (value == _subcorrespondences)
                    return;
                _subcorrespondences = value;
                OnPropertyChanged(GetPropertyName(() => SubCorrespondences));
                OnPropertyChanged(GetPropertyName(() => SubCorrespondencesCount));
            }
        }

        public string SubCorrespondencesCount
        {
            get { return string.Format("{0} Request Types", _subcorrespondences.Count()); }
            private set { ; }
        }


        public IEnumerable<crmSLAView> SLAs
        {
            get { return _slas; }
            set
            {
                if (value == _slas)
                    return;
                _slas = value;

                OnPropertyChanged(GetPropertyName(() => SLAs));
                OnPropertyChanged(GetPropertyName(() => SelectedSCSLA));
            }
        }

        #endregion

        #region Commands

        public ICommand LoadedCommand
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
          
                            //_scsla = _slas.ElementAt(0);
                            Console.WriteLine("Loadded agagin");
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }
    

        public ICommand CancelCreateCommand
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
                            DialogResponse res = AppData.MessageService.ShowMessage("Are you sure you want to cancel?", DialogType.Question);
                            if (res == DialogResponse.Yes)
                            {
                                SelectedIdx = 0;
                                reset();
                            }
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message);
                        }
                    }
                };
            }
        }
    
        public ICommand CreateSubCorrespondenceCommand
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
                            SelectedSubCorrespondence = new crmSubCorrespondenceView { Id = 0, code = "", description = "", Name = "", sla_id=0,correspondence_id=-1 };
                            Title = "Adding New Request Type";
                            _isUpdate = false;
                            ShowCancel = Visibility.Visible;
                        }
                        catch (Exception err)
                        {
                            AppData.MessageService.ShowMessage(err.Message + err.GetBaseException().ToString());
                        }
                    }
                };
            }
        }
   
        public ICommand SaveSubCorrespondenceCommand
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
                            if (validate_subcorrespondence())
                            {
                                string message = "";

                                if (_isUpdate)
                                {
                                    CrmData.SaveSubCorrespondence(SelectedSubCorrespondence);
                                    message = "Request Type successfully updated.";
                                }
                                else
                                {
                                    CrmData.AddSubCorrespondence(SelectedSubCorrespondence);
                                    SubCorrespondences = CrmData.get_Sub_Correspondence();
                                    SelectedIdx = SubCorrespondences.Count() - 1;
                                    message = "New Request Type successfully created.";
                                }

                                AppData.MessageService.ShowMessage(message, "Manage Request Type");
                            }
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

        # region Constructor

        public SubCorrespondenceViewModel()
        {
        }

  

        #endregion

        #region Private Methods
        
        private void reset()
        {
            _isUpdate = true;
            ShowCancel = Visibility.Collapsed;
        }

        private bool validate_subcorrespondence()
        {
            if (SelectedSubCorrespondence.Name == string.Empty) { AppData.MessageService.ShowMessage("Please specify the name of the request type you want to create", "No request type name", DialogType.Error);  return false; }
            else if (SelectedSubCorrespondence.correspondence_id <= 0) { AppData.MessageService.ShowMessage("Please select the associated category of the request type you want to create", "No category selected", DialogType.Error); return false; }
            else if (SelectedSubCorrespondence.sla_id < 0) { AppData.MessageService.ShowMessage("Please select the associated SLA of the request type you want to create", "No SLA selected", DialogType.Error); return false; }
            else if (SelectedSubCorrespondence.code == string.Empty) { AppData.MessageService.ShowMessage("Please specify the code of the request type you want to create", "No request type code", DialogType.Error); return false; }
            else { return true; }
        }

        #endregion
    }
}

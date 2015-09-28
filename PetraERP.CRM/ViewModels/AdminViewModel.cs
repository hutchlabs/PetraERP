
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
    public partial class AdminViewModel : ViewModelBase
    {
        #region Private Members
        
        private string _title = "Welcome";
        private int _selectedIdx = 0;
        private bool _isUpdate = true;
        private Visibility _showCancel = Visibility.Collapsed;

        private crmCategoryView _category;
        private IEnumerable<crmCategoryView> _categories = CrmData.get_Categories();

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

  
        public crmCategoryView SelectedCategory
        {
            get { return _category; }
            set
            {
                if (value == _category)
                    return;
                _category = value;

                reset();

                if (_category != null)
                    Title = "Editing " + _category.Name;

                OnPropertyChanged(GetPropertyName(() => SelectedCategory));
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
            get { return string.Format("{0} Correspondences", _correspondences.Count()); }
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
            get { return string.Format("{0} Sub Correspondences", _subcorrespondences.Count()); }
            private set { ; }
        }


        public crmSLAView SelectedSLA
        {
            get { return _sla; }
            set
            {
                if (value == _sla)
                    return;
                _sla = value;

                reset();

                if (_sla != null)
                    Title = "Editing " + _sla.Name;

                OnPropertyChanged(GetPropertyName(() => SelectedSLA));
            }
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
                OnPropertyChanged(GetPropertyName(() => SLACount));
            }
        }

        public string SLACount
        {
            get { return string.Format("{0} SLAs", _slas.Count()); }
            private set { ; }
        }

        #endregion

        #region Commands
    
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
    
        public ICommand CreateCategoryCommand
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
                            SelectedCategory = new crmCategoryView { Id=0, code="", description="", Name="" };
                            Title = "Adding New Category";
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
        public ICommand SaveCategoryCommand
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
                            if (validate_category())
                            {
                                string message = "";

                                if (_isUpdate)
                                {
                                    CrmData.SaveCategory(SelectedCategory);
                                    message =  "Category successfully updated.";
                                }
                                else
                                {
                                    CrmData.AddCategory(SelectedCategory);
                                    Categories = CrmData.get_Categories();
                                    SelectedIdx = Categories.Count() - 1;
                                    message = "New category successfully created.";
                                }
                                                         
                                AppData.MessageService.ShowMessage(message, "Manage Categories");
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

        public ICommand CreateCorrespondenceCommand
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
                            SelectedCorrespondence = new crmCorrespondenceView { Id = 0, code = "", description = "", Name = "", category="", category_id=-1 };
                            Title = "Adding New Correspondence";
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
        public ICommand SaveCorrespondenceCommand
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
                            if (validate_correspondence())
                            {
                                string message = "";

                                if (_isUpdate)
                                {
                                    CrmData.SaveCorrespondence(SelectedCorrespondence);
                                    message = "Correspondence successfully updated.";
                                }
                                else
                                {
                                    CrmData.AddCorrespondence(SelectedCorrespondence);
                                    Correspondences = CrmData.get_Correspondence();
                                    SelectedIdx = Correspondences.Count() - 1;
                                    message = "New correspondence successfully created.";
                                }

                                AppData.MessageService.ShowMessage(message, "Manage Correspondence");
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
                            Title = "Adding New Sub Correspondence";
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
                                    message = "Sub Correspondence successfully updated.";
                                }
                                else
                                {
                                    CrmData.AddSubCorrespondence(SelectedSubCorrespondence);
                                    SubCorrespondences = CrmData.get_Sub_Correspondence();
                                    SelectedIdx = SubCorrespondences.Count() - 1;
                                    message = "New Sub Correspondence successfully created.";
                                }

                                AppData.MessageService.ShowMessage(message, "Manage Sub Correspondence");
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

        public ICommand CreateSLACommand
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
                            SelectedSLA = new crmSLAView { Id = 0, code = "", Name = "", Escalated=0, Pre_escalate=0  };
                            Title = "Adding New SLA";
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
        public ICommand SaveSLACommand
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
                            if (validate_sla())
                            {
                                string message = "";

                                if (_isUpdate)
                                {
                                    CrmData.SaveSLA(SelectedSLA);
                                    message = "SLA successfully updated.";
                                }
                                else
                                {
                                    CrmData.AddSLA(SelectedSLA);
                                    SLAs = CrmData.get_SLAs_View();
                                    SelectedIdx = SLAs.Count() - 1;
                                    message = "New SLA successfully created.";
                                }

                                AppData.MessageService.ShowMessage(message, "Manage SLAs");
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

        public AdminViewModel()
        {
        }

        #endregion

        #region Private Methods
        
        private void reset()
        {
            _isUpdate = true;
            ShowCancel = Visibility.Collapsed;
        }
        
        private bool validate_category()
        {        
            if (SelectedCategory.Name == string.Empty) 
            { 
                AppData.MessageService.ShowMessage("Please specify the name of the category you want to create", "No category name",DialogType.Error); 
                return false; 
            } else if (SelectedCategory.code == string.Empty) { 
                AppData.MessageService.ShowMessage("Please specify the code of the category you want to create", "No category code",DialogType.Error); 
                return false; 
            }
        
            return true;
        }

        private bool validate_correspondence()
        {
            if (SelectedCorrespondence.Name == string.Empty) { AppData.MessageService.ShowMessage("Please specify the name of the correspondence you want to create", "No correspondence name", DialogType.Error); return false; }
            else if (SelectedCorrespondence.category_id <= 0) { AppData.MessageService.ShowMessage("Please select the category for the correspondence you want to create", "No category selected", DialogType.Error); return false; }
            else if (SelectedCorrespondence.code == string.Empty) { AppData.MessageService.ShowMessage("Please specify the code for the correspondence you want to create", "No correspondence code", DialogType.Error); return false; }
            else { return true; }
        }

        private bool validate_subcorrespondence()
        {
            if (SelectedSubCorrespondence.Name == string.Empty) { AppData.MessageService.ShowMessage("Please specify the name of the correspondence you want to create", "No correspondence name", DialogType.Error);  return false; }
            else if (SelectedSubCorrespondence.correspondence_id <= 0) { AppData.MessageService.ShowMessage("Please select the associated correspondence of the sub correspondence you want to create", "No correspondence selected", DialogType.Error); return false; }
            else if (SelectedSubCorrespondence.sla_id < 0) { AppData.MessageService.ShowMessage("Please select the associated SLA of the sub correspondence you want to create", "No SLA selected", DialogType.Error); return false; }
            else if (SelectedSubCorrespondence.code == string.Empty) { AppData.MessageService.ShowMessage("Please specify the code of the sub correspondence you want to create", "No sub correspondence code", DialogType.Error); return false; }
            else { return true; }
        }

        private bool validate_sla()
        {
            if (SelectedSLA.Name == string.Empty) { AppData.MessageService.ShowMessage("Please specify the name of the SLA you want to create", "No SLA name", DialogType.Error); return false; }
            else if (SelectedSLA.Pre_escalate < 0) { AppData.MessageService.ShowMessage("Please specify SLA pre escalate timer in hours", "Invalid Pre Escalate Value", DialogType.Error); return false; }
            else if (SelectedSLA.Escalated < 0) { AppData.MessageService.ShowMessage("Please specify SLA escalate timer in hours", "Invalid Escalate Value", DialogType.Error); return false; }
            else if (SelectedSLA.code == string.Empty) { AppData.MessageService.ShowMessage("Please specify a unique code for this SLA", "No SLA Code Specified", DialogType.Error); return false; }
            else { return true; }
        }

        #endregion
    }
}

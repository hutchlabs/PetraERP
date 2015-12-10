using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PetraERP.Shared.Models;

namespace PetraERP.Shared.UI.MessagingService
{
    internal class MessagingService : IMessagingService
    {
        #region IMessagingService Members

        void IMessagingService.ShowProgressMessage(string header, string message)
        {
            _isMessageDialogVisible = true;
            _controller = _metroWindow.ShowProgressAsync(header, message);
        }

        async void IMessagingService.CloseProgressMessage()
        {
            if (_isMessageDialogVisible)
            {
                _isMessageDialogVisible = false;
                if (null != _controller)
                {
                    var controller = await _controller;
                    await controller.CloseAsync();
                }
            }
        }

        void IMessagingService.ShowMessage(string message)
        {
            ShowMessage(message, DefaultHeaders.GetDefaultHeader(DialogType.Message), DialogType.Message);
        }

        void IMessagingService.ShowMessage(string message, string header)
        {
            ShowMessage(message, header, DialogType.Message);
        }

        DialogResponse IMessagingService.ShowMessage(string message, DialogType dialogueType)
        {
            return ShowMessage(message, DefaultHeaders.GetDefaultHeader(dialogueType), dialogueType);
        }

        DialogResponse IMessagingService.ShowMessage(string message, string header, DialogType dialogueType)
        {
            return ShowMessage(message, header, dialogueType);
        }

        void IMessagingService.ShowCustomMessageDialog(string viewKey, ModalDialogViewModelBase viewModel)
        {
            var modalWindow = GetCustomWindow(viewKey, viewModel);
            modalWindow.ShowDialog();
        }

        void IMessagingService.ShowCustomMessage(string viewKey, ModalDialogViewModelBase viewModel)
        {
            var modalWindow = GetCustomWindow(viewKey, viewModel);
            modalWindow.Show();
        }

        void IMessagingService.RegisterFlyout(FlyoutViewModelBase flyoutViewModelBase)
        {
            RegisterFlyout(flyoutViewModelBase);
        }

        #endregion

        #region Private Methods

        private Window GetCustomWindow(string viewKey, ModalDialogViewModelBase viewModel)
        {
            if (string.IsNullOrEmpty(viewKey))
                throw new ArgumentException("viewKey");
            if (null == viewModel)
                throw new ArgumentException("viewModel");
            var viewRegistry = ModalViewRegistry.Instance;
            if (!viewRegistry.ContainsKey(viewKey))
                throw new ArgumentException(string.Format("the key is not present in the registry: {0}", viewKey));

            var userControl = viewRegistry.GetViewByKey(viewKey); // Get the UserControl.
            var modalWindow = new ModalCustomMessageDialog
            {
                // Set the content of the window as the user control
                DataContext = viewModel,
                // Set the data context of the window as the ViewModel
                Owner = AppData.AppMainWindow,
                // Set the owner of the modal window to the app window.
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,
                //Set content
                ActualContent = userControl,
                //Adjust height and width
                SizeToContent = SizeToContent.WidthAndHeight,
            };

            modalWindow.Closed += CleanModalWindow;
            return modalWindow;
        }

        private void CleanModalWindow(object sender, EventArgs e)
        {
            var modalWindow = sender as ModalCustomMessageDialog;
            if (null != modalWindow)
            {
                modalWindow.Content = null;
                modalWindow.Owner = null;
                modalWindow.Closed -= CleanModalWindow;
            }
        }

        private DialogResponse ShowMessage(string message, string header, DialogType dialogueType)
        {
            var result = new ModalMessageDialog().ShowMessage(message, header.ToUpper(),
                                                                         dialogueType, _metroWindow);
            return result;
        }

        private void RegisterFlyout(FlyoutViewModelBase flyoutViewModelBase)
        {
            if (flyoutViewModelBase == null) 
                throw new ArgumentNullException("flyoutViewModelBase");
            var flyoutContainer = AppData.AppMainWindow.DataContext as IFlyoutContainer;
            if(flyoutContainer != null && flyoutContainer.Flyouts!=null)
            {
                flyoutContainer.Flyouts.Add(flyoutViewModelBase);
            }
        }

        #endregion

        #region Constructor

        public MessagingService()
        {
            _metroWindow = Application.Current.MainWindow as MetroWindow;
            if (null == _metroWindow)
                throw new Exception("Failed to initialize Messaging Service");
            _metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
        }

        #endregion

        #region Member Variables

        private readonly MetroWindow _metroWindow;
        private Task<ProgressDialogController> _controller;
        private bool _isMessageDialogVisible;//This is required to get rid of any error that might be thrown doe to mahapps metro progress dialog implementation..

        #endregion
    }

    internal static class DefaultHeaders
    {
        #region Static Propeties

        public static string DefaultInformationHeader { get; private set; }
        public static string DefaultErrorHeader { get; private set; }
        public static string DefaultQuestionHeader { get; private set; }

        #endregion

        #region Static Constructor
   
        static DefaultHeaders()
        {
            DefaultInformationHeader = "Information";
            DefaultErrorHeader = "Error";
            DefaultQuestionHeader = "Confirm";
        }

        #endregion

        #region Static Method
    
        public static string GetDefaultHeader(DialogType dialogueType)
        {
            string retValue = null;
            switch (dialogueType)
            {
                case DialogType.Message:
                    retValue = DefaultInformationHeader;
                    break;
                case DialogType.Error:
                    retValue = DefaultErrorHeader;
                    break;
                case DialogType.Question:
                    retValue = DefaultQuestionHeader;
                    break;
            }
            return retValue;
        }
        #endregion

    }
}

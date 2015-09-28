namespace PetraERP.Shared.UI.MessagingService
{
    public interface IMessagingService
    {
        void ShowMessage(string message);

        void ShowMessage(string message, string header);

        DialogResponse ShowMessage(string message, DialogType dialogueType);

        DialogResponse ShowMessage(string message, string header, DialogType dialogueType);

        void ShowProgressMessage(string header, string message);

        void CloseProgressMessage();

        void ShowCustomMessageDialog(string viewKey, ModalDialogViewModelBase viewModel);

        void ShowCustomMessage(string viewKey, ModalDialogViewModelBase viewModel);

        void RegisterFlyout(FlyoutViewModelBase flyoutViewModelBase);
    }
}

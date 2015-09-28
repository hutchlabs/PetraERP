using System;
using System.Windows;

namespace PetraERP.Shared.UI.MessagingService
{
    public partial class ModalCustomMessageDialog
    {
        #region Constructors

        static ModalCustomMessageDialog()
        {
        }

        public ModalCustomMessageDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Static Event Handlers

        private static void OnActualContentPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var modalWindow = source as ModalCustomMessageDialog;
            if (null != modalWindow)
            {
                if (null != modalWindow.ActualContentHolder)
                    modalWindow.ActualContentHolder.Content = e.NewValue;
            }
        }

        #endregion

        public object ActualContent
        {
            get { return GetValue(ActualContentProperty); }
            set { SetValue(ActualContentProperty, value); }
        }
     
        public static readonly DependencyProperty ActualContentProperty = DependencyProperty.Register("ActualContent", typeof(object), typeof(ModalCustomMessageDialog), new PropertyMetadata(null, OnActualContentPropertyChanged));

        #region Base class overrides

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataContextProperty)
            {
                var oldViewModel = e.OldValue as ModalDialogViewModelBase;
                if (null != oldViewModel)
                    oldViewModel.DialogResultSelected -= OnViewNotified;
                var newViewModel = e.NewValue as ModalDialogViewModelBase;
                if (null != newViewModel)
                    newViewModel.DialogResultSelected += OnViewNotified;
            }
            else
            {
                base.OnPropertyChanged(e);
            }
        }

        #endregion

        #region Private Helpers

        private void OnViewNotified(object sender, System.EventArgs e)
        {
            var viewModel = DataContext as ModalDialogViewModelBase;
            if (null != viewModel)
                viewModel.DialogResultSelected -= OnViewNotified;
            Close(); 
        }

        #endregion
    }
}


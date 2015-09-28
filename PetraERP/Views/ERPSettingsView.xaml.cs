namespace PetraERP.Views
{
    public partial class ERPSettingsView
    {
        public ERPSettingsView()
        {
            InitializeComponent();
        }

        private void chx_userfilter_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.chx_userfilter.IsChecked == true)
            {
                viewUsers.SelectAll();
            }
            else
            {
                viewUsers.UnselectAll();
                viewUsers.SelectedIndex = 0;
            }
        }
    }
}

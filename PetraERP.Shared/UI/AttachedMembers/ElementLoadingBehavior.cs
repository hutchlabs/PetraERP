using System.Windows;

namespace PetraERP.Shared.UI.AttachedMembers
{
    public static class ElementLoadingBehavior
    {
        #region InitializeDataContextWhenLoaded

        public static bool GetInitializeDataContextWhenLoaded(FrameworkElement element)
        {
            return (bool)element.GetValue(InitializeDataContextWhenLoadedProperty);
        }

        public static void SetInitializeDataContextWhenLoaded(FrameworkElement element, bool value)
        {
            element.SetValue(InitializeDataContextWhenLoadedProperty, value);
        }

        public static readonly DependencyProperty InitializeDataContextWhenLoadedProperty =
            DependencyProperty.RegisterAttached(
            "InitializeDataContextWhenLoaded",
            typeof(bool),
            typeof(ElementLoadingBehavior),
            new UIPropertyMetadata(false, OnInitializeDataContextWhenLoadedChanged));


        static void OnInitializeDataContextWhenLoadedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var item = depObj as FrameworkElement;
            if (item == null)
                return;
            if (e.NewValue is bool == false)
                return;
            if ((bool)e.NewValue)
                item.Loaded += OnElementLoaded;
            else
                item.Loaded -= OnElementLoaded;
        }

        static void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            if (!ReferenceEquals(sender, e.OriginalSource))
                return;
            var item = e.OriginalSource as FrameworkElement;
            if (item != null)
            {
                var dataContext = item.DataContext as ViewModelBase;
                if(null!=dataContext && !dataContext.IsInitialized)
                {
                    dataContext.Initialize();
                }
            }
        }

        #endregion
    }
}

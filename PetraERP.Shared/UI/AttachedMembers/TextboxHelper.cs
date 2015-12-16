using System.Windows;
using System.Windows.Controls;

namespace PetraERP.Shared.UI.AttachedMembers
{
    /**********************************************************
     * Adapted from:
     * http://www.wpftutorial.net/TextBoxBox.html
     * ********************************************************/
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty TextBoxProperty =
            DependencyProperty.RegisterAttached("TextBox",
            typeof(string), typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, OnTextBoxPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(TextBoxHelper));


        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetTextBox(DependencyObject dp)
        {
            return (string)dp.GetValue(TextBoxProperty);
        }

        public static void SetTextBox(DependencyObject dp, string value)
        {
            dp.SetValue(TextBoxProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void OnTextBoxPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox TextBoxBox = sender as TextBox;
            TextBoxBox.TextChanged -= TextBoxChanged;

            if (!(bool)GetIsUpdating(TextBoxBox))
            {
                TextBoxBox.Text = (string)e.NewValue;
            }
            TextBoxBox.TextChanged += TextBoxChanged;
        }

        private static void Attach(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox TextBoxBox = sender as TextBox;

            if (TextBoxBox == null)
                return;

            if ((bool)e.OldValue)
            {
                TextBoxBox.TextChanged -= TextBoxChanged;
            }

            if ((bool)e.NewValue)
            {
                TextBoxBox.TextChanged += TextBoxChanged;
            }
        }

        private static void TextBoxChanged(object sender, RoutedEventArgs e)
        {
            TextBox TextBoxBox = sender as TextBox;
            SetIsUpdating(TextBoxBox, true);
            SetTextBox(TextBoxBox, TextBoxBox.Text);
            SetIsUpdating(TextBoxBox, false);
        }
    }
}

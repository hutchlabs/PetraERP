using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PetraERP.Shared.UI.AttachedMembers
{
    public static class StyleProperties
    {
        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetTextProperty(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        public static DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(StyleProperties), new FrameworkPropertyMetadata(""));
    }
}

using System.Windows;
using System.Windows.Controls;

namespace WPFDatePicker.Views
{
    public class LastAndThisMonthDatePickerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FillerTemplate { get; set; }
        public DataTemplate LabelTemplate { get; set; }
        public DataTemplate PickButtonTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return FillerTemplate;
            }

            if (item is string)
            {
                return LabelTemplate;
            }

            return PickButtonTemplate;
        }
    }
}

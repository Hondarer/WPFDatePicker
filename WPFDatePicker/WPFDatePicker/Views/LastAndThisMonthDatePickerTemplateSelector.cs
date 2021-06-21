using System;
using System.Windows;
using System.Windows.Controls;
using WPFDatePicker.ViewModels;

namespace WPFDatePicker.Views
{
    /// <summary>
    /// デートピッカーのテンプレート選択機構を提供します。
    /// </summary>
    public class LastAndThisMonthDatePickerTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 空白のテンプレートを取得または設定します。
        /// </summary>
        public DataTemplate FillerTemplate { get; set; }

        /// <summary>
        /// ラベルのテンプレートを取得または設定します。
        /// </summary>
        public DataTemplate LabelTemplate { get; set; }

        /// <summary>
        /// 日付選択ボタンのテンプレートを取得または設定します。
        /// </summary>
        public DataTemplate PickButtonTemplate { get; set; }

        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is LastAndThisMonthDatePickerViewModel.DateViewModel dateViewModel)
            {
                if (dateViewModel.SpecifyDate == default(DateTime))
                {
                    // 日付が未指定のときは、ラベルのテンプレートを返す。
                    return LabelTemplate;
                }
                else
                {
                    // 日付選択ボタンのテンプレートを返す。
                    return PickButtonTemplate;
                }
            }

            // null のときは、空白のテンプレートを返す。
            return FillerTemplate;
        }
    }
}

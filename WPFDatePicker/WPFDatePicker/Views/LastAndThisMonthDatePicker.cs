// 以下を処理すれば、左右キーで選択範囲を変更したり、上下キーで値を変更したりできるはず
// TextBox.Select(Int32, Int32)
// TextBox.SelectionStart
// TextBox.SelectionLength

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WPFDatePicker.Views
{
    /// <summary>
    /// 前月と今月に特化した日付選択部品を提供します。
    /// </summary>
    [TemplatePart(Name = "PART_dateTextBox", Type = typeof(TextBox))]
    public class LastAndThisMonthDatePicker : Control
    {
        /// <summary>
        /// <see cref="LastAndThisMonthDatePicker"/> クラスの静的な初期化をします。
        /// </summary>
        static LastAndThisMonthDatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LastAndThisMonthDatePicker), new FrameworkPropertyMetadata(typeof(LastAndThisMonthDatePicker)));
        }

        public LastAndThisMonthDatePicker()
        {
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            if (Template.FindName("PART_dateTextBox", this) is TextBox dateTextBox)
            {
                // フォーカスを得たときに全選択する
                dateTextBox.GotKeyboardFocus += (sender, e) =>
                {
                    ((TextBox)sender).SelectAll();
                };

                // Enter キーを入力した際に、ソースを更新する
                dateTextBox.KeyDown += (sender, e) =>
                {
                    if (e.Key != Key.Enter)
                    { 
                        return;
                    }
                    BindingExpression be = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                    e.Handled = true;
                };
            }

            base.OnApplyTemplate();
        }
    }
}

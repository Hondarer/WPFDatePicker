// 以下を処理すれば、左右キーで選択範囲を変更したり、上下キーで値を変更したりできるはず
// TextBox.Select(Int32, Int32)
// TextBox.SelectionStart
// TextBox.SelectionLength

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WPFDatePicker.Views
{
    /// <summary>
    /// 前月と今月に特化した日付選択部品を提供します。
    /// </summary>
    [TemplatePart(Name = "PART_selectedDateTextBox", Type = typeof(TextBox))]
    public class LastAndThisMonthDatePicker : Control
    {
        /// <summary>
        /// SelectedDate 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(default(DateTime)));

        /// <summary>
        /// 選択された日付を取得または設定します。
        /// </summary>
        public DateTime SelectedDate
        {
            get
            {
                return (DateTime)GetValue(SelectedDateProperty);
            }
            set
            {
                SetValue(SelectedDateProperty, value);
            }
        }

        /// <summary>
        /// TodayOffset 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TodayOffsetProperty =
            DependencyProperty.Register(nameof(TodayOffset), typeof(TimeSpan?), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(null));

        /// <summary>
        /// 本日を判断する時刻のオフセットを取得または設定します。
        /// </summary>
        public TimeSpan? TodayOffset
        {
            get
            {
                return (TimeSpan?)GetValue(TodayOffsetProperty);
            }
            set
            {
                SetValue(TodayOffsetProperty, value);
            }
        }

        /// <summary>
        /// StartDateOffset 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty StartDateOffsetProperty =
            DependencyProperty.Register(nameof(StartDateOffset), typeof(int), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(-30));

        /// <summary>
        /// 選択可能範囲の開始日のオフセットを取得または設定します。
        /// </summary>
        public int StartDateOffset
        {
            get
            {
                return (int)GetValue(StartDateOffsetProperty); 
            }
            set 
            { 
                SetValue(StartDateOffsetProperty, value); 
            }
        }

        /// <summary>
        /// EndDateOffset 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty EndDateOffsetProperty =
            DependencyProperty.Register(nameof(EndDateOffset), typeof(int), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(1));

        /// <summary>
        /// 選択可能範囲の終了日のオフセットを取得または設定します。
        /// </summary>
        public int EndDateOffset
        {
            get
            {
                return (int)GetValue(EndDateOffsetProperty);
            }
            set
            {
                SetValue(EndDateOffsetProperty, value);
            }
        }

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
            if (Template.FindName("PART_selectedDateTextBox", this) is TextBox dateTextBox)
            {
                // うまく動作してない
                //// フォーカスを得たときに全選択する
                //dateTextBox.GotFocus += (sender, e) =>
                //{
                //    ((TextBox)sender).SelectAll();
                //};

                // Enter キーを入力した際に、ソースを更新する
                dateTextBox.KeyDown += (sender, e) =>
                {
                    if (e.Key != Key.Enter)
                    { 
                        return;
                    }

                    BindingExpression be = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();

                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next)
                    {
                        Wrapped = true
                    };
                    ((TextBox)sender).MoveFocus(request);

                    e.Handled = true;
                };
            }

            base.OnApplyTemplate();
        }
    }
}

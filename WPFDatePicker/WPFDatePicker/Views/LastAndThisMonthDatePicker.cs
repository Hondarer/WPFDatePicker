using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using WPFDatePicker.ViewModels;

namespace WPFDatePicker.Views
{
    /// <summary>
    /// 前月と今月に特化した日付選択部品を提供します。
    /// </summary>
    [TemplatePart(Name = "PART_selectedDateTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_shortcutPanel", Type = typeof(Button))]
    public class LastAndThisMonthDatePicker : Control
    {
        #region 依存関係プロパティ

        /// <summary>
        /// SelectedDate 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(default(DateTime)));

        /// <summary>
        /// 選択された日付を取得します。
        /// </summary>
        public DateTime SelectedDate
        {
            get
            {
                return (DateTime)GetValue(SelectedDateProperty);
            }
            internal set
            {
                SetValue(SelectedDateProperty, value);
            }
        }

        /// <summary>
        /// TodayOffset 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TodayOffsetProperty =
            DependencyProperty.Register(nameof(TodayOffset), typeof(TimeSpan?), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// 本日を判断する時刻のオフセットを取得または設定します。規定値は <see cref="TimeSpan.Zero"/> です。
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
        /// 選択可能範囲の開始日のオフセットを取得または設定します。規定値は -30 です。
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
        /// 選択可能範囲の終了日のオフセットを取得または設定します。規定値は 1(明日)です。
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
        /// DefaultSelectDateOffset 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty DefaultSelectDateOffsetProperty =
            DependencyProperty.Register(nameof(DefaultSelectDateOffset), typeof(int), typeof(LastAndThisMonthDatePicker), new PropertyMetadata(0));

        /// <summary>
        /// 既定の選択日のオフセットを取得または設定します。規定値は 0 です。
        /// </summary>
        public int DefaultSelectDateOffset
        {
            get
            {
                return (int)GetValue(DefaultSelectDateOffsetProperty);
            }
            set
            {
                SetValue(DefaultSelectDateOffsetProperty, value);
            }
        }

        #endregion

        #region コンストラクター

        /// <summary>
        /// <see cref="LastAndThisMonthDatePicker"/> クラスの静的な初期化をします。
        /// </summary>
        static LastAndThisMonthDatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LastAndThisMonthDatePicker), new FrameworkPropertyMetadata(typeof(LastAndThisMonthDatePicker)));
        }

        #endregion

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            // 部品の配置されたウインドウの Closed イベントを購読する
            Window.GetWindow(this).Closed -= ParentWindow_Closed; // 複数回の呼び出しで購読が多重登録されないように
            Window.GetWindow(this).Closed += ParentWindow_Closed;

            // 時刻の変化イベントを購読する
            DateTimeManager.Instance.CurrentDateTimeChanged -= DateTimeManager_CurrentDateTimeChanged; // 複数回の呼び出しで購読が多重登録されないように
            DateTimeManager.Instance.CurrentDateTimeChanged += DateTimeManager_CurrentDateTimeChanged;

            if (Template.FindName("PART_popup", this) is Popup part_popup)
            {
                // ポップアップの位置を、起点オブジェクトの右下基準にする
                part_popup.CustomPopupPlacementCallback += (Size popupSize, Size targetSize, Point offset) =>
                {
                    return new CustomPopupPlacement[] { new CustomPopupPlacement() { Point = new Point(targetSize.Width - popupSize.Width, targetSize.Height) } };
                };

                // ポップアップを開いたときに、ポップアップ内ににフォーカスを移動する
                part_popup.Opened += (sender, e) =>
                {
                    if (sender is Popup popup)
                    {
                        if (Template.FindName("PART_shortcutPanel", this) is UIElement uiElement)
                        {
                            uiElement.Focus();
                        }
                    }
                };
            }

            if (Template.FindName("PART_selectedDateTextBox", this) is TextBox part_selectedDateTextBox)
            {
                #region フォーカスを得たときに全選択する

                // MEMO: Popup を開いた状態で TextBox にフォーカスを与えると全選択されない。詳細メカニズム未調査。機能に支障がないため、現状通りとしたい。

                part_selectedDateTextBox.MouseDoubleClick += (sender, e) =>
                {
                    if (sender is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                };

                part_selectedDateTextBox.GotKeyboardFocus += (sender, e) =>
                {
                    if (sender is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                };

                part_selectedDateTextBox.PreviewMouseLeftButtonDown += (sender, e) =>
                {
                    if (sender is TextBox textBox)
                    {
                        if (textBox.IsKeyboardFocusWithin == false)
                        {
                            e.Handled = true;
                            textBox.Focus();
                        }
                    }
                };

                #endregion

                // Enter キーを入力した際に、ソースを更新する
                part_selectedDateTextBox.KeyDown += (sender, e) =>
                {
                    if (e.Key != Key.Enter)
                    {
                        return;
                    }

                    BindingExpression be = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();

                    // 曜日表示を行いたいので、フォーカスを移動する
                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Previous)
                    {
                        Wrapped = true
                    };
                    ((TextBox)sender).MoveFocus(request);

                    e.Handled = true;
                };
            }

            base.OnApplyTemplate();
        }

        private void DateTimeManager_CurrentDateTimeChanged(object sender, EventArgs e)
        {
            if (DataContext is LastAndThisMonthDatePickerViewModel lastAndThisMonthDatePickerViewModel)
            {
                lastAndThisMonthDatePickerViewModel.CurrentDateTimeChanged(sender, e);
            }
        }

        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains no event data.</param>
        private void ParentWindow_Closed(object sender, EventArgs e)
        {
            DateTimeManager.Instance.CurrentDateTimeChanged -= DateTimeManager_CurrentDateTimeChanged;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using WPFDatePicker.Commands;

namespace WPFDatePicker.ViewModels
{
    public class LastAndThisMonthDatePickerViewModel : BindableBase
    {
        public class DateViewModel : BindableBase
        {
            public string Description { get; set; }

            public DateTime SpecifyDate { get; set; }

            private bool _isSelected = false;

            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    SetProperty(ref _isSelected, value);
                }
            }
        }

        private DateTime _today;

        public DateTime Today
        {
            get
            {
                return _today;
            }
            private set
            {
                if (SetProperty(ref _today, value.Date) == true)
                {
                    RefreshDatesViewModel();
                }
            }
        }

        private DateTime _startDate;

        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            private set
            {
                SetProperty(ref _startDate, value.Date);
            }
        }

        private DateTime _endDate;

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            private set
            {
                SetProperty(ref _endDate, value.Date);
            }
        }

        private DateTime _thisMonth1st;

        public DateTime ThisMonth1st
        {
            get
            {
                return _thisMonth1st;
            }
            private set
            {
                SetProperty(ref _thisMonth1st, value.Date);
            }
        }

        private DateTime _lastMonth1st;

        public DateTime LastMonth1st
        {
            get
            {
                return _lastMonth1st;
            }
            private set
            {
                SetProperty(ref _lastMonth1st, value.Date);
            }
        }

        /// <summary>
        /// 本日を判断する時刻のオフセットを保持します。
        /// </summary>
        private TimeSpan _todayOffset = TimeSpan.Zero;

        /// <summary>
        /// 本日を判断する時刻のオフセットを取得または設定します。
        /// </summary>
        public TimeSpan TodayOffset
        {
            get
            {
                return _todayOffset;
            }
            set
            {
                if (SetProperty(ref _todayOffset, value) == true)
                {
                    InvalidateToday();
                }
            }
        }

        /// <summary>
        /// 選択可能範囲の開始日のオフセットを保持します。
        /// </summary>
        private int _startDateOffset = -30;

        /// <summary>
        /// 選択可能範囲の開始日のオフセットを取得または設定します。
        /// </summary>
        public int StartDateOffset
        {
            get
            {
                return _startDateOffset;
            }
            set
            {
                if (SetProperty(ref _startDateOffset, value) == true)
                {
                    RefreshDatesViewModel();
                }
            }
        }

        /// <summary>
        /// 選択可能範囲の終了日のオフセットを保持します。
        /// </summary>
        private int _endDateOffset = 1;

        /// <summary>
        /// 選択可能範囲の開始日のオフセットを取得または設定します。
        /// </summary>
        public int EndDateOffset
        {
            get
            {
                return _endDateOffset;
            }
            set
            {
                if (SetProperty(ref _endDateOffset, value) == true)
                {
                    RefreshDatesViewModel();
                }
            }
        }

        /// <summary>
        /// 既定の選択日のオフセットを保持します。
        /// </summary>
        private int _defaultSelectDateOffset = 0;

        /// <summary>
        /// 既定の選択日のオフセットを取得または設定します。
        /// </summary>
        public int DefaultSelectDateOffset
        {
            get
            {
                return _defaultSelectDateOffset;
            }
            set
            {
                if (SetProperty(ref _defaultSelectDateOffset, value) == true)
                {
                    ChangeSelectedDateCore(Today.AddDays(DefaultSelectDateOffset));
                }
            }
        }

        private DateTime _selectedDate;

        public DateTime SelectedDate

        {
            get
            {
                return _selectedDate;
            }
            set
            {
                if(SetProperty(ref _selectedDate, value)==true)
                {
                    OnPropertyChanged(nameof(SelectedDateString));
                }
            }
        }

        public string SelectedDateString
        {
            get
            {
                return string.Format("{0:yyyy/MM/dd}", SelectedDate);
            }
            set
            {
                // 入力された文字列を DateTime として評価する
                if (DateTime.TryParseExact(value,
                    Resources.StringResource.LastAndThisMonthDatePicker_DateParseFormats.Split(','),
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDateTime) == false)
                {
                    // 無効な値の場合にはソースを更新しない
                    // View を元の値に戻すために、PropertyChanged イベントを発行する
                    OnPropertyChanged();
                    return;
                }

                // 選択可能な範囲にあるかチェック
                if ((parsedDateTime < StartDate) || (EndDate < parsedDateTime))
                {
                    // 無効な値の場合にはソースを更新しない
                    // View を元の値に戻すために、PropertyChanged イベントを発行する
                    OnPropertyChanged();
                    return;
                }

                ChangeSelectedDateCore(parsedDateTime);
            }
        }

        public string SelectedDateFullString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{SelectedDate:yyyy/MM/dd(ddd)}");

                DateViewModel selectedShortcut = Shortcuts.Where(s => s.IsSelected).FirstOrDefault();
                if (selectedShortcut != null)
                {
                    sb.Append($" {selectedShortcut.Description}");
                }

                return sb.ToString();
            }
        }

        private void ChangeSelectedDateCore(DateTime specifyDate)
        {
            // 日付以下の情報があれば削除する
            SelectedDate = specifyDate.Date;

            foreach (object vm in LastMonthDays.Union(ThisMonthDays).Union(Shortcuts))
            {
                if (vm is DateViewModel dateViewModel)
                {
                    if (dateViewModel.SpecifyDate == SelectedDate)
                    {
                        dateViewModel.IsSelected = true;
                    }
                    else
                    {
                        dateViewModel.IsSelected = false;
                    }
                }
            }

            OnPropertyChanged(nameof(SelectedDateFullString));
        }

        private bool _popupOpen;

        public bool PopupOpen
        {
            get
            {
                return _popupOpen;
            }
            set
            {
                SetProperty(ref _popupOpen, value);
            }
        }

        public DelegateCommand SpecifyDateCommamnd { get; }

        private List<DateViewModel> _lastMonthDays;

        public List<DateViewModel> LastMonthDays
        {
            get
            {
                return _lastMonthDays;
            }
            set
            {
                SetProperty(ref _lastMonthDays, value);
            }
        }

        private List<DateViewModel> _thisMonthDays;

        public List<DateViewModel> ThisMonthDays
        {
            get
            {
                return _thisMonthDays;
            }
            set
            {
                SetProperty(ref _thisMonthDays, value);
            }
        }

        private List<DateViewModel> _shortcuts;

        public List<DateViewModel> Shortcuts
        {
            get
            {
                return _shortcuts;
            }
            set
            {
                SetProperty(ref _shortcuts, value);
            }
        }

        public LastAndThisMonthDatePickerViewModel()
        {
            SpecifyDateCommamnd = new DelegateCommand(
               parameter =>
               {
                   if (parameter is DateTime specifyDate)
                   {
                       ChangeSelectedDateCore(specifyDate);
                   }
                   else
                   {
                       int offsetDay = 0;
                       if (parameter != null)
                       {
                           try
                           {
                               offsetDay = Convert.ToInt32(parameter);
                           }
                           catch
                           {
                               // NOP
                           }
                       }
                       ChangeSelectedDateCore(Today.AddDays(offsetDay));
                   }

                   // ポップアップを閉じる
                   PopupOpen = false;
               },
               parameter =>
               {
                   DateTime specifyDate;
                   if (parameter is DateTime time)
                   {
                       specifyDate = time;
                   }
                   else
                   {
                       int offsetDay = 0;
                       if (parameter != null)
                       {
                           try
                           {
                               offsetDay = Convert.ToInt32(parameter);
                           }
                           catch
                           {
                               // NOP
                           }
                       }
                       specifyDate = Today.AddDays(offsetDay);
                   }

                   // もし SelectedDate が選択範囲から逸脱していた場合には、実行不可
                   if ((specifyDate < StartDate) || (EndDate < specifyDate))
                   {
                       return false;
                   }

                   return true;
               });

            InvalidateToday();
        }

        public void CurrentDateTimeChanged(object sender, EventArgs e)
        {
            InvalidateToday();
        }

        private void InvalidateToday()
        {
            Today = DateTimeManager.Instance.CurrentDateTime.Add(TodayOffset);
        }

        private void RefreshDatesViewModel()
        {
            #region 開始・終了日の算出

            // 範囲開始日
            StartDate = Today.AddDays(StartDateOffset);

            // 範囲終了日
            DateTime endDate = Today.AddDays(EndDateOffset);

            // 終了日が開始日より速い場合は、開始日に補正
            if (endDate < StartDate)
            {
                endDate = StartDate;
            }

            EndDate = endDate;

            #endregion

            #region 先月1日、今月1日の算出

            // 今月の1日を得る
            ThisMonth1st = Today.AddDays(-Today.Day + 1);

            // 先月の1日を得る
            LastMonth1st = ThisMonth1st.AddMonths(-1);

            #endregion

            #region カレンダーの作成

            List<DateViewModel> _lastMonthDays = new List<DateViewModel>();
            List<DateViewModel> _thisMonthDays = new List<DateViewModel>();

            // 先月のカレンダー作成

            for (int day = 0; day < LastMonth1st.AddMonths(1).AddDays(-1).Day; day++)
            {
                DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = LastMonth1st.AddDays(day) };
                _lastMonthDays.Add(dateViewModel);
            }

            // 今月のカレンダー作成

            for (int day = 0; day < ThisMonth1st.AddMonths(1).AddDays(-1).Day; day++)
            {
                DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = ThisMonth1st.AddDays(day) };
                _thisMonthDays.Add(dateViewModel);
            }

            // 範囲開始日が前々月以前の場合
            if (StartDate <= LastMonth1st.AddDays(-1))
            {
                // 範囲開始日から前々月月末までを先月のカレンダーに追加
                for (DateTime lastLastMonthDay = LastMonth1st.AddDays(-1); StartDate <= lastLastMonthDay; lastLastMonthDay = lastLastMonthDay.AddDays(-1))
                {
                    DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = lastLastMonthDay };

                    _lastMonthDays.Insert(0, dateViewModel);
                }
            }

            // 範囲終了日が翌月以降の場合
            if (ThisMonth1st.AddMonths(1) <= EndDate)
            {
                // 翌月1日から範囲終了日までを今月のカレンダーに追加
                for (DateTime nextMonthDay = ThisMonth1st.AddMonths(1); nextMonthDay <= EndDate; nextMonthDay = nextMonthDay.AddDays(1))
                {
                    DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = nextMonthDay };

                    _thisMonthDays.Add(dateViewModel);
                }
            }

            // カレンダー上の開始曜日の調整

            // 先月
            int lastMonthCalenderDayOfWeek = (int)_lastMonthDays.First().SpecifyDate.DayOfWeek;
            for (int dayOfWeek = 0; dayOfWeek < lastMonthCalenderDayOfWeek; dayOfWeek++) // 日曜 = 0
            {
                _lastMonthDays.Insert(0, null);
            }

            // 今月
            for (int dayOfWeek = 0; dayOfWeek < (int)ThisMonth1st.DayOfWeek; dayOfWeek++) // 日曜 = 0
            {
                _thisMonthDays.Insert(0, null);
            }

            // 曜日ラベルの追加
            for (int dayOfWeek = (int)DayOfWeek.Saturday; dayOfWeek >= (int)DayOfWeek.Sunday; dayOfWeek--)
            {
                _lastMonthDays.Insert(0, new DateViewModel() { Description = CultureInfo.CurrentUICulture.DateTimeFormat.GetShortestDayName((DayOfWeek)dayOfWeek) });
                _thisMonthDays.Insert(0, new DateViewModel() { Description = CultureInfo.CurrentUICulture.DateTimeFormat.GetShortestDayName((DayOfWeek)dayOfWeek) });
            }

            LastMonthDays = _lastMonthDays;
            ThisMonthDays = _thisMonthDays;

            #endregion

            #region ショートカットの作成

            List<DateViewModel> shortcuts = new List<DateViewModel>()
            {
                new DateViewModel(){SpecifyDate=Today.AddDays(1), Description=Resources.StringResource.Tomorrow },
                new DateViewModel(){SpecifyDate=Today, Description=Resources.StringResource.Today },
                new DateViewModel(){SpecifyDate=Today.AddDays(-1), Description=Resources.StringResource.Yesterday },
                new DateViewModel(){SpecifyDate=Today.AddDays(-7), Description=Resources.StringResource.ThisDayLastWeek }
            };

            Shortcuts = shortcuts;

            #endregion

            if ((SelectedDate < StartDate) || (EndDate < SelectedDate))
            {
                // もし SelectedDate が選択範囲から逸脱していた場合には、選択日付を既定の日にする
                ChangeSelectedDateCore(Today.AddDays(DefaultSelectDateOffset));
            }
            else
            {
                // 選択状態の更新
                ChangeSelectedDateCore(SelectedDate);
            }

            // コマンドの実行可否再評価を依頼
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

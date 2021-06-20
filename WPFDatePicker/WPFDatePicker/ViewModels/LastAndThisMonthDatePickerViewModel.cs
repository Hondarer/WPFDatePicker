using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using WPFDatePicker.Commands;

namespace WPFDatePicker.ViewModels
{
    public class LastAndThisMonthDatePickerViewModel : BindableBase
    {
        public class DateViewModel : BindableBase
        {
            public DateTime SpecifyDate { get; set; }

            public bool CanPick { get; set; } = true;
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
        private TimeSpan? _todayOffset;

        /// <summary>
        /// 本日を判断する時刻のオフセットを取得または設定します。
        /// </summary>
        public TimeSpan? TodayOffset
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
        private int _startDateOffset;

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
        private int _endDateOffset;

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

        private DateTime _selectedDate;

        public string SelectedDate
        {
            get
            {
                return string.Format("{0:yyyy/MM/dd(ddd)}", _selectedDate);
            }
            set
            {
                // 無効な値の場合にはソースを更新しない

                // 曜日を取り除く(日付だけ変更することを考慮)
                string targetValue = Regex.Replace(value, @"\(.*\)", string.Empty);

                // 日付として評価する
                // TODO: yyyy/mm/dd, yyyymmdd, yy/mm/dd, yymmdd, mmddなど、きちんと順序だてて評価したほうがいい

                //if (!DateTime.TryParseExact(targetValue, "y/M/d", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDateTime))
                if (!DateTime.TryParse(targetValue, out DateTime parsedDateTime)) // 現在のカルチャで解釈可能なら良いとする
                {
                    OnPropertyChanged();
                    return;
                }

                ChangeDateCore(parsedDateTime);
            }
        }

        private bool ChangeDateCore(DateTime specifyDate)
        {
            // 日付以下の情報があれば削除する
            specifyDate = specifyDate.Date;

            // TODO: 選択可能な範囲になければクリップ

            if (_selectedDate != specifyDate)
            {
                _selectedDate = specifyDate;
                OnPropertyChanged(nameof(SelectedDate));
                return true;
            }

            return false;
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

        public LastAndThisMonthDatePickerViewModel()
        {
            SpecifyDateCommamnd = new DelegateCommand(
               parameter =>
               {
                   if (parameter is DateTime specifyDate)
                   {
                       ChangeDateCore(specifyDate);
                       return;
                   }

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
                   ChangeDateCore(Today.AddDays(offsetDay));
               },
               parameter =>
               {
                   // TODO: 実行可否判定
                   return true;
               });

            DateTimeManager.Instance.CurrentDateTimeChanged += DateTimeManager_CurrentDateTimeChanged;

            InvalidateToday();
        }

        private void DateTimeManager_CurrentDateTimeChanged(object sender, EventArgs e)
        {
            InvalidateToday();
        }

        private void InvalidateToday()
        {
            if (TodayOffset == null)
            {
                Today = DateTimeManager.Instance.CurrentDateTime.Date;
            }
            else
            {
                Today = DateTimeManager.Instance.CurrentDateTime.Add((TimeSpan)TodayOffset).Date;
            }
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

            List<DateViewModel> _lastMonthDays = new List<DateViewModel>();
            List<DateViewModel> _thisMonthDays = new List<DateViewModel>();

            // 先月のカレンダー作成

            for (int day = 0; day < LastMonth1st.AddMonths(1).AddDays(-1).Day; day++)
            {
                DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = LastMonth1st.AddDays(day) };
                if ((dateViewModel.SpecifyDate < StartDate) || (EndDate < dateViewModel.SpecifyDate))
                {
                    dateViewModel.CanPick = false;
                }

                _lastMonthDays.Add(dateViewModel);
            }

            // 今月のカレンダー作成

            for (int day = 0; day < ThisMonth1st.AddMonths(1).AddDays(-1).Day; day++)
            {
                DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = ThisMonth1st.AddDays(day) };
                if ((dateViewModel.SpecifyDate < StartDate) || (EndDate < dateViewModel.SpecifyDate))
                {
                    dateViewModel.CanPick = false;
                }

                _thisMonthDays.Add(dateViewModel);
            }

            // 範囲終了日が前々月以前の場合
            if (StartDate<=LastMonth1st.AddDays(-1))
            {
                // 範囲終了日までを今月のカレンダーに追加
                for (DateTime lastLastMonthDay = LastMonth1st.AddDays(-1); StartDate < lastLastMonthDay; lastLastMonthDay = lastLastMonthDay.AddDays(-1))
                {
                    DateViewModel dateViewModel = new DateViewModel() { SpecifyDate = lastLastMonthDay };

                    _lastMonthDays.Insert(0,dateViewModel);
                }
            }

            // 範囲終了日が翌月以降の場合
            if (ThisMonth1st.AddMonths(1) <= EndDate)
            {
                // 範囲終了日までを今月のカレンダーに追加
                for (DateTime nextMonthDay = ThisMonth1st.AddMonths(1); nextMonthDay < EndDate; nextMonthDay = nextMonthDay.AddDays(1))
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
                _thisMonthDays.Insert(0,null);
            }

            LastMonthDays = _lastMonthDays;
            ThisMonthDays = _thisMonthDays;

            _selectedDate = Today;
            OnPropertyChanged(nameof(SelectedDate));

            // コマンドの実行可否再評価
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

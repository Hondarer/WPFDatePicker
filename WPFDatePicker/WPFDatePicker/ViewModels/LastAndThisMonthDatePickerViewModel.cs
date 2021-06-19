using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WPFDatePicker.Commands;

namespace WPFDatePicker.ViewModels
{
    public class LastAndThisMonthDatePickerViewModel : BindableBase
    {
        public class DateViewModel : BindableBase
        {
            public DateTime? SpecifyDate { get; set; }

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
                if (SetProperty(ref _today, value) == true)
                {
                    RefreshDatesViewModel();
                }
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
                SetProperty(ref _startDateOffset, value);
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
                SetProperty(ref _endDateOffset, value);
            }
        }

        private string _thisMonthHeader;

        public string ThisMonthHeader
        {
            get
            {
                return _thisMonthHeader;
            }
            private set
            {
                SetProperty(ref _thisMonthHeader, value);
            }
        }

        private string _lastMonthHeader;

        public string LastMonthHeader
        {
            get
            {
                return _lastMonthHeader;
            }
            private set
            {
                SetProperty(ref _lastMonthHeader, value);
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
                   // TODO: 実行不可判定
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
            List<DateViewModel> _lastMonthDays = new List<DateViewModel>();
            List<DateViewModel> _thisMonthDays = new List<DateViewModel>();

            for (int dayIndex = 0; dayIndex < (7 * 5); dayIndex++)
            {
                // TODO: 下記はレイアウトテスト用で、設定しているDateTimeは現段階ででたらめ
                //       _today をもとに、カレンダーを生成する
                _lastMonthDays.Add(new DateViewModel() { SpecifyDate = Today.AddDays(dayIndex) });
                _thisMonthDays.Add(new DateViewModel() { SpecifyDate = Today.AddDays(dayIndex) });
            }

            LastMonthDays = _lastMonthDays;
            ThisMonthDays = _thisMonthDays;

            ThisMonthHeader = "This month";
            LastMonthHeader = "Last month";

            _selectedDate = Today;
            OnPropertyChanged(nameof(SelectedDate));
        }
    }
}

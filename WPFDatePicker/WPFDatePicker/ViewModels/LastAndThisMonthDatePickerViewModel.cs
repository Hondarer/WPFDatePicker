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
        }

        // 今日の概念が変わる可能性があるためフィールドで持っておく
        private DateTime _today = DateTime.Today;

        private DateTime _selectedDate;

        public string SelectedDate
        {
            get
            {
                return string.Format("{0:yyyy/MM/dd(ddd)}",_selectedDate);
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

            if(_selectedDate != specifyDate)
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
                   ChangeDateCore(_today.AddDays(offsetDay));
               },
               parameter =>
               {
                   return null;
               });

            RefreshDatesViewModel();
        }

        private void RefreshDatesViewModel()
        {
            List<DateViewModel> _lastMonthDays = new List<DateViewModel>();
            List<DateViewModel> _thisMonthDays = new List<DateViewModel>();

            for (int dayIndex = 0; dayIndex < (7 * 5); dayIndex++)
            {
                // TODO: 下記はレイアウトテスト用で、設定しているDateTimeは現段階ででたらめ
                //       _today をもとに、カレンダーを生成する
                _lastMonthDays.Add(new DateViewModel() { SpecifyDate = _today.AddDays(dayIndex) });
                _thisMonthDays.Add(new DateViewModel() { SpecifyDate = _today.AddDays(dayIndex) });
            }

            LastMonthDays = _lastMonthDays;
            ThisMonthDays = _thisMonthDays;

            _selectedDate = _today;
        }
    }
}

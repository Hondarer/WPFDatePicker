using System;
using System.Text.RegularExpressions;

namespace WPFDatePicker.ViewModels
{
    public class LastAndThisMonthDatePickerViewModel : BindableBase
    {
        private DateTime _selectedDate = DateTime.Today;

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
                //if (!DateTime.TryParseExact(targetValue, "y/M/d", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDateTime))
                if (!DateTime.TryParse(targetValue, out DateTime parsedDateTime)) // 現在のカルチャで解釈可能なら良いとする
                {
                    OnPropertyChanged();
                    return;
                }

                // TODO: 選択可能な範囲になければクリップ

                SetProperty(ref _selectedDate, parsedDateTime);
            }
        }
    }
}

using System;
using System.Windows.Threading;
using WPFDatePicker.Commands;

namespace WPFDatePicker.ViewModels
{
    public class DateTimeManager : BindableBase
    {
        public event EventHandler CurrentDateTimeChanged;

        private static readonly DateTimeManager s_instance = new DateTimeManager();

        public static DateTimeManager Instance
        {
            get
            {
                return s_instance;
            }
        }

        private DispatcherTimer _dispatcherTimer;

        private DateTime _currentDateTime = DateTime.Now;

        public DateTime CurrentDateTime
        {
            get
            {
                return _currentDateTime;
            }
            set
            {
                if (SetProperty(ref _currentDateTime, value) == true)
                {
                    CurrentDateTimeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public DelegateCommand SetCurrentDateTimeCommand { get; private set; }

        private readonly TimeSpan _timerTimeSpan = new TimeSpan(0, 0, 1);

        public DateTimeManager()
        {
            SetCurrentDateTimeCommand = new DelegateCommand(
                parameter =>
                {
                    try
                    {
                        DateTime setDateTime = Convert.ToDateTime(parameter);
                        CurrentDateTime = setDateTime;
                    }
                    catch
                    {
                        // NOP
                    }
                });

            _dispatcherTimer = new DispatcherTimer
            {
                Interval = _timerTimeSpan
            };
            _dispatcherTimer.Tick += Timer_Tick;
            _dispatcherTimer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentDateTime = CurrentDateTime.Add(_timerTimeSpan);
        }
    }
}

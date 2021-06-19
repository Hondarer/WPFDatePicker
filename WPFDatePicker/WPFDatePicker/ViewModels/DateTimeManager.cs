using System;
using System.Windows.Threading;
using WPFDatePicker.Commands;

namespace WPFDatePicker.ViewModels
{
    public class DateTimeManager : BindableBase
    {
        private static readonly DateTimeManager s_instance = new DateTimeManager();

        public static DateTimeManager Instance
        {
            get
            {
                return s_instance;
            }
        }

        private DispatcherTimer _dispatcherTimer;

        private DateTime _now = DateTime.Now;

        public DateTime Now
        {
            get
            {
                return _now;
            }
            set
            {
                SetProperty(ref _now, value);
            }
        }

        public DelegateCommand SetNowCommand { get; private set; }

        private readonly TimeSpan _timerTimeSpan = new TimeSpan(0, 0, 1);

        public DateTimeManager()
        {
            SetNowCommand = new DelegateCommand(
                parameter => 
                {
                    try
                    {
                        DateTime setDateTime = Convert.ToDateTime(parameter);
                        Now = setDateTime;
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
            Now = Now.Add(_timerTimeSpan);
        }
    }
}

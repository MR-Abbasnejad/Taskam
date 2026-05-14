using System;
using System.ComponentModel;

namespace Taskam.Data.Services
{
    public class SystemTimeService : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private DateTime _currentDateTime;

        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            private set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged(nameof(CurrentDateTime));
                    OnPropertyChanged(nameof(CurrentDate));
                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        public DateTime CurrentDate => CurrentDateTime.Date;
        public TimeSpan CurrentTime => CurrentDateTime.TimeOfDay;

        public SystemTimeService()
        {
            CurrentDateTime = DateTime.Now;

            _timer = new System.Timers.Timer(60000); // 1 minute
            _timer.Elapsed += (s, e) => CurrentDateTime = DateTime.Now;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}

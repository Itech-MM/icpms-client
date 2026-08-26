using System;
using System.IO.Ports;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace icpms_client.Utils.Watchers
{
    public class PortWatcher : IDisposable
    {
        private readonly Timer _timer;
        private string[] _lastPorts;
        private bool _isDisposed = false;

        public event Action<string[]> PortsChanged;

        public PortWatcher(double interval = 2000)
        {
            _timer = new Timer(interval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            _lastPorts = SerialPort.GetPortNames();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            CheckForPortChange();
        }

        private void CheckForPortChange()
        {
            // Get the current ports
            var currentPorts = SerialPort.GetPortNames();

            // Check if the list of ports has changed
            if (!currentPorts.SequenceEqual(_lastPorts))
            {
                _lastPorts = currentPorts;
                PortsChanged?.Invoke(currentPorts);
            }
        }

        public void StopWatching()
        {
            if (!_isDisposed)
            {
                _timer.Stop();
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _isDisposed = true;
            }
        }
    }

}
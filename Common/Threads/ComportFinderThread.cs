using System.Collections.ObjectModel;
using icpms_client.Utils.Watchers;

namespace icpms_client.Common.Threads
{
    public abstract class ComportFinderThread
    {
        private readonly PortWatcher _portWatcher;
        private static readonly ObservableCollection<IPortWatcher> Watchers = [];

        // Singleton instance
        private static ComportFinderThread? _instance;

        public static ComportFinderThread Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComportFinderThreadImpl();
                }
                return _instance;
            }
        }

        protected ComportFinderThread()
        {
            _portWatcher = new PortWatcher();
            _portWatcher.PortsChanged += OnPortChanged;
        }

        private void OnPortChanged(string[]? portNames)
        {
            foreach (var watcher in Watchers)
            {
                watcher.OnPortChanged(portNames);
            }
        }

        public static void RegisterWatcher(IPortWatcher watcher)
        {
            if (!Watchers.Contains(watcher))
            {
                Watchers.Add(watcher);
            }
        }

        public static void UnregisterWatcher(IPortWatcher watcher)
        {
            Watchers.Remove(watcher);
        }

        public void Dispose()
        {
            _portWatcher.Dispose();
        }
    }

    public class ComportFinderThreadImpl : ComportFinderThread
    {
    }
}

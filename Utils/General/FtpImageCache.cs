using System.Windows.Media.Imaging;

namespace icpms_client.Utils.General
{
    public class FtpImageCache(int maxItems = 100)
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, LinkedListNode<CacheEntry>> _map = new();
        private readonly LinkedList<CacheEntry> _lruList = new();

        private record CacheEntry(string Key, BitmapImage Image);

        public bool TryGet(string key, out BitmapImage? image)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var node))
                {
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    image = node.Value.Image;
                    return true;
                }

                image = null;
                return false;
            }
        }

        public void Set(string key, BitmapImage image)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var existingNode))
                {
                    _lruList.Remove(existingNode);
                }

                var node = new LinkedListNode<CacheEntry>(new CacheEntry(key, image));
                _lruList.AddFirst(node);
                _map[key] = node;

                while (_lruList.Count > maxItems)
                {
                    var last = _lruList.Last!;
                    _map.Remove(last.Value.Key);
                    _lruList.RemoveLast();
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
                _lruList.Clear();
            }
        }
    }
}
namespace VirtualTexture.Runtime
{
    using System.Collections.Generic;

    // ReSharper disable once InconsistentNaming
    public class LRUCache
    {
        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            this._cache = new Dictionary<int, LinkedListNode<int>>();
            this._list = new LinkedList<int>();
        }
        
        public int capacity { get; private set; }
        public int count { get { return _cache.Count; } }
        public int first { get { return _list.First.Value; } }
        public int last { get { return _list.Last.Value; } }
        
        private Dictionary<int, LinkedListNode<int>> _cache;
        private LinkedList<int> _list;
        
        public void Add(int key)
        {
            if (_cache.ContainsKey(key))
            {
                _list.Remove(_cache[key]);
                _list.AddFirst(_cache[key]);
            }
            else
            {
                if (_cache.Count >= capacity)
                {
                    _cache.Remove(_list.Last.Value);
                    _list.RemoveLast();
                }
                _cache.Add(key, _list.AddFirst(key));
            }
        }
        
        public void Remove(int key)
        {
            if (_cache.ContainsKey(key))
            {
                _list.Remove(_cache[key]);
                _cache.Remove(key);
            }
        }
        
        public void Clear()
        {
            _cache.Clear();
            _list.Clear();
        }
        
        public bool Contains(int key)
        {
            return _cache.ContainsKey(key);
        }
        
        public void Touch(int key)
        {
            if (_cache.ContainsKey(key))
            {
                _list.Remove(_cache[key]);
                _list.AddFirst(_cache[key]);
            }
        }
        
        public int[] ToArray()
        {
            int[] array = new int[_cache.Count];
            _list.CopyTo(array, 0);
            return array;
        }
        
        public IEnumerator<int> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}

namespace VirtualTexture.Runtime
{
    using System.Collections.Generic;

    // ReSharper disable once InconsistentNaming
    public class LRUCache
    {
        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            this._list = new LinkedListNode[this.capacity + 1];
            Reset();
        }

        public readonly int capacity;
        private int _last;
        private LinkedListNode[] _list;

        private int first => this._list[0].next;
        
        /// <summary>
        /// Require a unused tile.
        /// </summary>
        public int Require()
        {
            return Touch(first);
        }
        
        /// <summary>
        /// Info the cache that the tile is being used.
        /// </summary>
        public int Touch(int key)
        {
            if (key != _last)
            {
                // Remove from list
                int prev = _list[key].prev;
                int next = _list[key].next;
                _list[prev].next = next;
                _list[next].prev = prev;
            
                // Add to last
                _list[key].prev = _last;
                _list[_last].next = key;
                _last = key;
            }
            
            return key;
        }

        /// <summary>
        /// Reset Cache.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < this.capacity; i++)
            {
                _list[i].prev = i - 1;
                _list[i].next = i + 1;
            }

            _last = this.capacity - 1;
        }
        
        private struct LinkedListNode
        {
            public int prev;
            public int next;
        }
    }
}

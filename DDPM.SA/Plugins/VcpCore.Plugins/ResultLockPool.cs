using System;
using System.Collections;
using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public class ResultLockPool : IEnumerable<KeyValuePair<Guid, object>>
    {
        private readonly object _ResultLockPoollock = new object();
        private Dictionary<Guid, object> _ResultPool;

        public ResultLockPool()
        {
            _ResultPool ??= new Dictionary<Guid, object>();
        }

        private IEnumerable<KeyValuePair<Guid, object>> Events()
        {
            foreach (var item in _ResultPool)
                yield return item;
        }

        public IEnumerator<KeyValuePair<Guid, object>> GetEnumerator()
        {
            return Events().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Guid guid, object obj)
        {
            lock (_ResultLockPoollock) { _ResultPool.Add(guid, obj); }
        }

        public bool IsEmpty()
        {
            lock (_ResultLockPoollock) { return (_ResultPool.Count == 0); }
        }

        public bool TryGetValue(Guid guid, out object output)
        {
            lock (_ResultLockPoollock) { return (_ResultPool.TryGetValue(guid, out output)); }
        }

        public bool TakeAway(Guid guid, out object output)
        {
            lock (_ResultLockPoollock)
            {
                bool _gb = (_ResultPool.TryGetValue(guid, out output));
                bool _rb = (_ResultPool.Remove(guid));

                return (_gb && _rb);
            }
        }

        public bool Remove(Guid guid)
        {
            lock (_ResultLockPoollock)
            {
                return (_ResultPool.Remove(guid));
            }
        }

        public void Clear()
        {
            lock (_ResultLockPoollock)
            {
                _ResultPool.Clear();
            }
        }

        public bool ContainsKey(Guid guid)
        {
            lock (_ResultLockPoollock)
            {
                return (_ResultPool.ContainsKey(guid));
            }
        }

        public int Count()
        {
            lock (_ResultLockPoollock)
            {
                return (_ResultPool.Count);
            }
        }
    }
}
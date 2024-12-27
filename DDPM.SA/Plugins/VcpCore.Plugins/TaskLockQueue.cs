namespace VcpCore.Plugins
{
    public class LinkedNode<T>
    {
        public T Data { set; get; }
        public LinkedNode<T> Next { set; get; }
    }

    public class LinkedListQueue<T>
    {
        private int _count = 0;
        private LinkedNode<T> _node = null;

        public virtual void Enqueue(T data)
        {
            var node = new LinkedNode<T> { Data = data, Next = null };

            if (_node == null)
                _node = node;
            else
            {
                var ptr = _node;
                while (ptr.Next != null)
                    ptr = ptr.Next;

                ptr.Next = node;
            }
            _count++;
        }

        public virtual bool IsEmpty()
        {
            return (_count == 0);
        }

        public virtual int Count()
        {
            return _count;
        }

        public virtual T Dequeue()
        {
            if (_node == null)
                return default(T);

            var ptr = _node;

            _node = _node.Next;
            _count--;

            return ptr.Data;
        }

        public virtual bool Clear()
        {
            if (_node == null)
            {
                _count = 0;
                return true;
            }
            else
            {
                try
                {
                    _node = null;
                    _count = 0;

                    return true;
                }
                catch { return false; }
                finally
                {
                    _node = null;
                    _count = 0;
                }
            }
        }
    }

    public class TaskLockQueue<T> : LinkedListQueue<T>
    {
        private readonly object _TaskLockQueuelock = new object();

        public override void Enqueue(T data)
        {
            lock (_TaskLockQueuelock) { base.Enqueue(data); }
        }

        public override bool IsEmpty()
        {
            lock (_TaskLockQueuelock) { return base.IsEmpty(); }
        }

        public override T Dequeue()
        {
            lock (_TaskLockQueuelock) { return base.Dequeue(); }
        }

        public override bool Clear()
        {
            lock (_TaskLockQueuelock) { return base.Clear(); }
        }

        public override int Count()
        {
            lock (_TaskLockQueuelock) { return base.Count(); }
        }
    }
}
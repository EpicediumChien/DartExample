using System;
using System.Collections;
using System.Collections.Generic;
using VcpCore.Common;

namespace VcpCore.Plugins
{
    public class TaskLockQueue<T> : IEnumerable<T>
    {
        private readonly object _TaskLockQueuelock = new object();
        private PriorityQueue<T, (Priority, DateTime)> TaskQueue;

        public TaskLockQueue()
        {
            TaskQueue ??= new PriorityQueue<T, (Priority, DateTime)>();
        }

        private IEnumerable<T> Events()
        {
            while (TaskQueue.Count > 0)
            {
                var item = TaskQueue.Dequeue();
                yield return item;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Events().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enqueue(T data, Priority priority)
        {
            lock (_TaskLockQueuelock) { TaskQueue.Enqueue(data, (priority, DateTime.Now)); }
        }

        public bool IsEmpty()
        {
            lock (_TaskLockQueuelock) { return TaskQueue.Count < 1; }
        }

        public T Dequeue()
        {
            lock (_TaskLockQueuelock) { return TaskQueue.Dequeue(); }
        }

        public void Clear()
        {
            lock (_TaskLockQueuelock) { TaskQueue.Clear(); }
        }

        public int Count()
        {
            lock (_TaskLockQueuelock) { return TaskQueue.Count; }
        }
    }
}
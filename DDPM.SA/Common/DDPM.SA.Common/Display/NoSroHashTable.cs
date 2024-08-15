using System;
using System.Collections;

namespace DDPM.SA.Common
{
    [Serializable]
    public class NoSroHashTable : Hashtable
    {
        private ArrayList list = new ArrayList();

        public override void Add(object key, object value)
        {
            base.Add(key, value);
            list.Add(key);
        }

        public override void Clear()
        {
            base.Clear();
            list.Clear();
        }

        public override void Remove(object key)
        {
            base.Remove(key);
            list.Remove(key);
        }

        public override ICollection Keys
        {
            get
            {
                return list;
            }
        }
    }
}
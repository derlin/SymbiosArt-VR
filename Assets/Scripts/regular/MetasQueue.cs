using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace symbiosart.datas
{
    public class MetasQueue : List<ImageMetas>
    {
        private static readonly int MAX_CAPACITY = 100;

        public ImageMetas Dequeue()
        {
            if (Count == 0) return null;

            var metas = this[Count - 1];
            RemoveAt(Count - 1);
            return metas;
        }

        public bool Enqueue(ImageMetas metas)
        {
            if (Contains(metas)) return false;
            Add(metas);
            if (Count > MAX_CAPACITY) RemoveAt(0);
            return true;
        }

        public void EnqueueAll(ICollection<ImageMetas> coll)
        {
            foreach (var metas in coll)
            {
                Enqueue(metas);
            }
        }
    }
}


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Utils
{
    public class BinarySearchMultiSet<T> : ICollection<T> where T : IComparable
    {
        public List<T> list;
        IComparer<T> comparer;

        public BinarySearchMultiSet(IComparer<T> comparer)
        {
            this.comparer = comparer;
            list = new List<T>();
        }

        public BinarySearchMultiSet(ICollection<T> collection, IComparer<T> comparer)
        {
            list = new List<T>(collection);
            this.comparer = comparer;

            list.Sort(comparer);
        }

        public void Add(T item)
        {
            if (list.Count == 0)
            {
                list.Add(item);
                return;
            }

            int index = list.BinarySearch(item, comparer);

            if (index >= 0)
                list.Insert(index, item);
            else
                list.Insert(~index, item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            int index = list.BinarySearch(item, comparer);

            return index >= 0;
        }

        public int IndexOf(T item)
        {
            return list.BinarySearch(item, comparer);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public T ElementAt(int index)
        {
            return list[index];
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Forgeage
{
	public class ObservableList<T> : List<T>
	{
        public class ListChangedEventArgs : EventArgs
        {
            public T item;

            public ListChangedEventArgs(T item)
            {
                this.item = item;
            }
        }

        public delegate void AddedToListDelegate(int position);
        public delegate T RemovedFromListDelegate();

        public event EventHandler OnListChanged;
        public event EventHandler OnListRemoved;
        public event EventHandler OnListAdded;

		public new void Add(T item)
        {
            base.Add(item);
            OnListChanged.Invoke(this, null);
            OnListAdded.Invoke(this, new ListChangedEventArgs(item));
        }

        public new void Remove(T item)
        {
            int index = FindIndex(it => it.Equals(item));
            base.Remove(item);
            OnListChanged.Invoke(this, null);
            OnListRemoved.Invoke(this, new ListChangedEventArgs(item));
        }

        public new void RemoveAt(int index)
        {
            T value = this[index];
            base.RemoveAt(index);
            OnListChanged.Invoke(this, null);
            OnListRemoved.Invoke(this, new ListChangedEventArgs(value));
        }

        public new void Clear()
        {
            //this.ForEach(it => OnListRemoved.Invoke(this, new ListChangedEventArgs(it)));
            base.Clear();
            OnListChanged.Invoke(this, null);
        }
	}
}

﻿/*****************************************************************
*
* Copyright 2011 Dan Beavon
*
* This file is part of OXTail.
*
* OXTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OXTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections;
using OxTailHelpers;
using OxTailHelpers.Compare;

namespace OxTailHelpers
{
    [Serializable]
    public class HighlightCollection<T> : List<T>, IBindingList where T : HighlightItem
    {
        private object _syncRoot = new object();
        private bool _isSorted = false;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;

        public HighlightCollection()
            : base()
        {
        }

        public HighlightCollection(int capacity)
            : base(capacity)
        {
        }

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            ListChangedEventHandler handler = ListChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void FireListChanged(T item)
        {
            if(this.Contains(item))
            {
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, this.IndexOf(item)));
            }
        }

        #region IBindingList

        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        public object AddNew()
        {
            throw new NotSupportedException();
        }

        public bool AllowEdit
        {
            get { return false; }
        }

        public bool AllowNew
        {
            get { return true; }
        }

        public bool AllowRemove
        {
            get { return true; }
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            if (direction == ListSortDirection.Ascending)
            {
                this.Sort(new GenericComparer<T>(Constants.HIGHLIGHT_ITEM_SORT_HEADER, ListSortDirection.Ascending));
            }
            else
            {
                this.Sort(new GenericComparer<T>(Constants.HIGHLIGHT_ITEM_SORT_HEADER, ListSortDirection.Descending));
            }
        }

        public int Find(PropertyDescriptor property, object key)
        {
            return -1;
        }

        public bool IsSorted
        {
            get { return _isSorted; }
        }

        public event ListChangedEventHandler ListChanged;

        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        public void RemoveSort()
        {
            _isSorted = false;
        }

        public ListSortDirection SortDirection
        {
            get { return _sortDirection; }
        }

        public PropertyDescriptor SortProperty
        {
            get { return _sortProperty; }
        }

        public bool SupportsChangeNotification
        {
            get { return true; }
        }

        public bool SupportsSearching
        {
            get { return false; }
        }

        public bool SupportsSorting
        {
            get { return true; }
        }

        public int Add(object value)
        {
            if (value is T)
            {
                base.Add(value as T);
                int index = base.IndexOf(value as T);
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, base.Count - 1));

                return index;
            }

            else
            {
                throw new ArgumentException(LanguageHelper.GetLocalisedText((System.Windows.Application.Current as IApplication), "valueNotCorrectType"), "value");
            }
        }

        public new void Clear()
        {
            base.Clear();
        }

        public bool Contains(object value)
        {
            return base.Contains(value as T);
        }

        public int IndexOf(object value)
        {
            return base.IndexOf(value as T);
        }

        public void Insert(int index, object value)
        {
            base.Insert(index, value as T);
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            int index = base.IndexOf(value as T);
            base.RemoveAt(index);

            for (int i = this.Count - 1; i >= 0; i--)
            {
                T item = (T)this[i];
                item.Order = i;
            }

            // Keep getting exceptions when using the Event on a delete
            // Will handle it by refreshing the datasource in the control
            //OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        public new object this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value as T;
            }
        }

        public void CopyTo(Array array, int index)
        {
            return;   
        }

        public new int Count
        {
            get { return base.Count; }
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { return this._syncRoot; }
        }

        public new System.Collections.IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion IBindingList
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace OxTailLogic.Compare
{
    /// <summary>
    /// Compare to generic objects
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    public class GenericComparer<T> : IComparer<T>
    {
        public string SortColumn {get; private set;}
        public ListSortDirection SortDirection {get; private set;}
        
        /// <summary>
        /// Construct instance
        /// </summary>
        /// <param name="sortColumn">The column to sort by</param>
        /// <param name="sortDirection">The direction of the sort</param>
        public GenericComparer(string sortColumn, ListSortDirection sortDirection)
        {
            this.SortColumn = sortColumn;
            this.SortDirection = sortDirection;
        }
        
        /// <summary>
        /// IComparer implementation
        /// </summary>
        /// <param name="compare">custom Object</param>
        /// <param name="compareTo">custom Object</param>
        /// <returns><see cref="Int32"/></returns>
        public int Compare(T compare, T compareTo)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(SortColumn);
            
            IComparable obj1 = (IComparable)propertyInfo.GetValue(compare, null);
            IComparable obj2 = (IComparable)propertyInfo.GetValue(compareTo, null);

            if (SortDirection == ListSortDirection.Ascending)
            {
                return (obj1.CompareTo(obj2));
            }
            else
            {
                return (obj2.CompareTo(obj1));
            }
        }
 
    }
}

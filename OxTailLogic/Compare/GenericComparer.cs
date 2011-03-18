/*****************************************************************
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
        /// <summary>
        /// The column to sort the collection by
        /// </summary>
        public string SortColumn {get; private set;}

        /// <summary>
        /// The direction to sort the collection by
        /// </summary>
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

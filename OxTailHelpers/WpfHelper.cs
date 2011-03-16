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
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Threading;

namespace OxTailHelpers
{
    public static class WpfHelper
    {
        delegate int ReturnInt();
        delegate Visual ReturnVisual();

        /// <summary>
        /// Gets the visual children of the control
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="referenceVisual"></param>
        /// <returns></returns>
        public static List<T> GetVisualChildren<T>(this Visual referenceVisual) where T : Visual
        {
            List<T> children = new List<T>();
            Visual child = null;
            int count = GetChildCount(referenceVisual);

            for (Int32 i = 0; i < count; i++)
            {
                child = (Visual)referenceVisual.Dispatcher.Invoke(new ReturnVisual(delegate() { return VisualTreeHelper.GetChild(referenceVisual, i) as Visual; }));
                if (child != null)
                {
                    if (child is T)
                    {
                        children.Add(child as T);
                    }
                    List<T> subchildren = GetVisualChildren<T>(child);
                    if (subchildren != null)
                    {
                        children.AddRange(subchildren);
                    }
                }
            }
            return children;
        }

        private static int GetChildCount(Visual referenceVisual)
        {
            int count = (int)referenceVisual.Dispatcher.Invoke(new ReturnInt(delegate() { return VisualTreeHelper.GetChildrenCount(referenceVisual); }));
            return count;
        }
    }
}

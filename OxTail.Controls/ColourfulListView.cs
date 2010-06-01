/*****************************************************************
* This file is part of OxTail.
*
* OxTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OxTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTail.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ColourfulListView : ListView
    {
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item)
        {            
            base.PrepareContainerForItemOverride(element, item);
            
            int index = ItemContainerGenerator.IndexFromContainer(element);
            ListViewItem lvi = element as ListViewItem;

            IColourfulItem p = lvi.Content as IColourfulItem;
            if (p != null)
            {
                lvi.Background = new SolidColorBrush(p.BackColour);
                lvi.Foreground = new SolidColorBrush(p.ForeColour);
            }
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            return base.MeasureOverride(constraint);
        }
    }
}

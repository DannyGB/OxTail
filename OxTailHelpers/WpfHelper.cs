using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace OxTailHelpers
{
    public static class WpfHelper
    {
        public static List<T> GetVisualChildren<T>(this Visual referenceVisual) where T : Visual
        {
            List<T> children = new List<T>();
            Visual child = null;
            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceVisual); i++)
            {
                child = VisualTreeHelper.GetChild(referenceVisual, i) as Visual;
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
    }
}

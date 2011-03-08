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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

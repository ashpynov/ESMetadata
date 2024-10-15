using System.Windows;
using System.Windows.Media;

namespace ESMetadata.Extensions
{
    public static class VisualTreeHelperExtensions
    {
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // If the child is of the requested type, return it.
                if (child is T t)
                {
                    foundChild = t;
                    break;
                }

                // Recursively drill down the tree.
                foundChild = FindChild<T>(child);

                // If the child is found, break so we do not overwrite the found child.
                if (foundChild != null) break;
            }

            return foundChild;
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }

};

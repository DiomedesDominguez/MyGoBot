namespace PGB.WPF.Internals
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    public static class DependencyObjectExtensions
    {
        public static T FindLogicalAncestor<T>(this DependencyObject dependencyObject) where T : class
        {
            var current = dependencyObject;
            do
            {
                current = LogicalTreeHelper.GetParent(current);
            } while (current != null && !(current is T));

            return current as T;
        }

        public static T FindLogicalDescendant<T>(this DependencyObject dependencyObject) where T : class
        {
            var current = dependencyObject;
            do
            {
                current = LogicalTreeHelper.GetChildren(current).OfType<DependencyObject>().FirstOrDefault();
            } while (current != null && !(current is T));

            return current as T;
        }

        public static T FindVisualAncestor<T>(this DependencyObject dependencyObject) where T : class
        {
            var reference = dependencyObject;
            do
            {
                reference = VisualTreeHelper.GetParent(reference);
            } while (reference != null && !(reference is T));

            return reference as T;
        }

        public static T FindVisualDescendant<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            var obj = default(T);
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var childIndex = 0; childIndex < childrenCount; ++childIndex)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, childIndex);
                if (!(child is T))
                {
                    obj = child.FindVisualDescendant<T>();
                    if (obj != null)
                    {
                        break;
                    }
                }
                else
                {
                    obj = (T) child;
                    break;
                }
            }

            return obj;
        }
    }
}
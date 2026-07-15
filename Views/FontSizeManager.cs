using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Applies a modest operator-selected font multiplier while retaining each
    /// element's authored base size. It deliberately changes text only, not the
    /// touch-target dimensions or overall window scale.
    /// </summary>
    public static class FontSizeManager
    {
        private static readonly DependencyProperty BaseFontSizeProperty =
            DependencyProperty.RegisterAttached("BaseFontSize", typeof(double), typeof(FontSizeManager),
                new PropertyMetadata(double.NaN));

        public static double GetScale(string setting)
        {
            switch (setting)
            {
                case "Small": return 0.9;
                case "Large": return 1.1;
                default: return 1.0;
            }
        }

        public static void Apply(DependencyObject root, string setting)
        {
            ApplyRecursive(root, GetScale(setting), new HashSet<DependencyObject>());
        }

        private static void ApplyRecursive(DependencyObject element, double scale,
            HashSet<DependencyObject> visited)
        {
            if (element == null || !visited.Add(element))
            {
                return;
            }

            var textBlock = element as TextBlock;
            if (textBlock != null)
            {
                double baseSize = (double)textBlock.GetValue(BaseFontSizeProperty);
                if (double.IsNaN(baseSize))
                {
                    baseSize = textBlock.FontSize;
                    textBlock.SetValue(BaseFontSizeProperty, baseSize);
                }
                textBlock.FontSize = Math.Max(8, baseSize * scale);
            }
            else if (element is TextBox || element is ComboBox || element is DatePicker || element is DataGrid)
            {
                var control = element as Control;
                if (control != null)
                {
                    double baseSize = (double)control.GetValue(BaseFontSizeProperty);
                    if (double.IsNaN(baseSize))
                    {
                        baseSize = control.FontSize;
                        control.SetValue(BaseFontSizeProperty, baseSize);
                    }
                    control.FontSize = Math.Max(8, baseSize * scale);
                }
            }

            if (element is Visual || element is Visual3D)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(element);
                for (int index = 0; index < childCount; index++)
                {
                    ApplyRecursive(VisualTreeHelper.GetChild(element, index), scale, visited);
                }
            }

            foreach (object child in LogicalTreeHelper.GetChildren(element))
            {
                var dependencyChild = child as DependencyObject;
                if (dependencyChild != null)
                {
                    ApplyRecursive(dependencyChild, scale, visited);
                }
            }
        }
    }
}

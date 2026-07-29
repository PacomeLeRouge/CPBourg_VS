using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Four-point touch verification workflow. It records the average distance
    /// from each requested target centre and persists the result as part of
    /// operator preferences.
    /// </summary>
    public partial class ScreenCalibrationDialog : UserControl
    {
        private readonly Point[] _targets =
        {
            new Point(70, 70),
            new Point(830, 70),
            new Point(830, 430),
            new Point(70, 430),
        };

        private readonly List<double> _errors = new List<double>();
        private int _targetIndex;

        public event EventHandler<ScreenCalibrationResult> Confirmed;

        public ScreenCalibrationDialog()
        {
            InitializeComponent();
        }

        public void Open()
        {
            _errors.Clear();
            _targetIndex = 0;
            MoveTarget();
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
        }

        private void OnTargetClick(object sender, RoutedEventArgs e)
        {
            Point touch = System.Windows.Input.Mouse.GetPosition(CalibrationCanvas);
            Point expected = _targets[_targetIndex];
            double dx = touch.X - expected.X;
            double dy = touch.Y - expected.Y;
            _errors.Add(Math.Sqrt(dx * dx + dy * dy));

            _targetIndex++;
            if (_targetIndex >= _targets.Length)
            {
                double total = 0;
                foreach (double error in _errors)
                {
                    total += error;
                }

                Visibility = Visibility.Collapsed;
                Confirmed?.Invoke(this, new ScreenCalibrationResult(total / _errors.Count));
                return;
            }

            MoveTarget();
        }

        private void MoveTarget()
        {
            Point target = _targets[_targetIndex];
            Canvas.SetLeft(CalibrationTarget, target.X - CalibrationTarget.Width / 2);
            Canvas.SetTop(CalibrationTarget, target.Y - CalibrationTarget.Height / 2);
            CalibrationProgressText.Text = string.Format(
                LocalizationManager.Translate("Target {0} of {1}"),
                _targetIndex + 1, _targets.Length);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}

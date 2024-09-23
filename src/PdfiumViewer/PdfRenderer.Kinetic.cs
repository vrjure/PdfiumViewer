using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PdfiumViewer
{
    partial class PdfRenderer
    {
        // Note: don't change the below fields to Property.
        // Note: Because INotifyPropertyChanged event change the page number!
        private const double DefaultFriction = 0.90;
        private const int InertiaHandlerInterval = 20; // milliseconds
        private const int InertiaMaxAnimationTime = 3000; // milliseconds
        private bool _isMouseDown;
        private Vector _velocity;
        private Point _scrollTarget;
        private Point _scrollStartPoint;
        private Point _scrollStartOffset;
        private Point _previousPoint;

        #region Friction

        /// <summary>
        /// Friction Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty FrictionProperty =
            DependencyProperty.RegisterAttached(nameof(Friction), typeof(double), typeof(PdfRenderer), 
                new FrameworkPropertyMetadata(DefaultFriction));

        public double Friction
        {
            get => (double)GetValue(FrictionProperty);
            set => SetValue(FrictionProperty, value);
        }

        #endregion

        #region EnableKinetic

        /// <summary>
        /// EnableKinetic Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty EnableKineticProperty = DependencyProperty.Register(nameof(EnableKinetic), typeof(bool), typeof(PdfRenderer), new FrameworkPropertyMetadata(false));

        public bool EnableKinetic
        {
            get => (bool)GetValue(EnableKineticProperty);
            set
            {
                SetValue(EnableKineticProperty, value);
            }
        }

        #endregion

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (EnableKinetic && IsMouseOver)
            {
                Cursor = Cursors.ScrollAll;
                // Save starting point, used later when
                // determining how much to scroll.
                _velocity = new Vector();
                _scrollStartPoint = e.GetPosition(this);
                _scrollStartOffset = new Point(HorizontalOffset, VerticalOffset);
                _isMouseDown = true;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (EnableKinetic && _isMouseDown)
            {
                var currentPoint = e.GetPosition(this);
                // Determine the new amount to scroll.
                _scrollTarget = GetScrollTarget(currentPoint);
                InertiaHandleMouseMove();
                // Scroll to the new position.
                ScrollToHorizontalOffset(_scrollTarget.X);
                ScrollToVerticalOffset(_scrollTarget.Y);
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (EnableKinetic)
            {
                Cursor = Cursors.Hand;
                _isMouseDown = false;
                InertiaHandleMouseUp();
            }
        }

        private Point GetScrollTarget(Point currentPoint)
        {
            var delta = new Point(_scrollStartPoint.X - currentPoint.X, _scrollStartPoint.Y - currentPoint.Y);
            var scrollTarget = new Point(_scrollStartOffset.X + delta.X, _scrollStartOffset.Y + delta.Y);

            if (scrollTarget.Y < 0)
            {
                scrollTarget.Y = 0;
            }

            if (scrollTarget.Y > ScrollableHeight)
            {
                scrollTarget.Y = ScrollableHeight;
            }

            return scrollTarget;
        }

        private void InertiaHandleMouseMove()
        {
            var currentPoint = Mouse.GetPosition(this);
            _velocity = _previousPoint - currentPoint;
            _previousPoint = currentPoint;
        }

        private async void InertiaHandleMouseUp()
        {
            for (var i = 0; i < InertiaMaxAnimationTime / InertiaHandlerInterval; i++)
            {
                if (_isMouseDown || _velocity.Length <= 1 || Environment.TickCount64 - MouseWheelUpdateTime < InertiaHandlerInterval * 2)
                    break;

                ScrollToHorizontalOffset(_scrollTarget.X);
                ScrollToVerticalOffset(_scrollTarget.Y);
                _scrollTarget.X += _velocity.X;
                _scrollTarget.Y += _velocity.Y;
                _velocity *= Friction;
                await Task.Delay(InertiaHandlerInterval);
            }
        }
    }
}

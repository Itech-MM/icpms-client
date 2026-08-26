using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace icpms_client.UserControls.UI.Reusable.ProgressBar
{
    public partial class CircularProgressBar
    {
        private bool _isIndeterminateAnimationActive;
        private DateTime _lastRenderTime;
        private double _currentArcLength = 90;
        private double _targetArcLength = 90;
        private double _arcLengthVelocity;
        private Random _random = new Random();
        private DispatcherTimer? _arcChangeTimer;

        public CircularProgressBar()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnIsVisibleChanged;
            SizeChanged += OnSizeChanged;
            UpdateArcThickness();
            SetupArcChangeTimer();
        }

        private void SetupArcChangeTimer()
        {
            _arcChangeTimer = new DispatcherTimer();
            _arcChangeTimer.Tick += OnArcChangeTimerTick;
            _arcChangeTimer.Interval = TimeSpan.FromMilliseconds(1500); // Change every 1.5 seconds
        }

        private void OnArcChangeTimerTick(object? sender, EventArgs e)
        {
            if (!IsIndeterminate || !IsVisible) return;
            
            // Generate a new random arc length between 30° and 150°
            _targetArcLength = 30 + _random.NextDouble() * 120;
            
            // Adjust interval for next change - random between 1-3 seconds
            _arcChangeTimer!.Interval = TimeSpan.FromMilliseconds(1000 + _random.Next(2000));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsIndeterminate)
            {
                UpdateIndeterminateArc();
            }
            else
            {
                UpdateProgress();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateAnimationState();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopIndeterminateAnimation();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            if (IsIndeterminate && IsVisible)
            {
                StartIndeterminateAnimation();
            }
            else
            {
                StopIndeterminateAnimation();
            }
        }

        private void StartIndeterminateAnimation()
        {
            if (_isIndeterminateAnimationActive) return;
            
            _isIndeterminateAnimationActive = true;
            _lastRenderTime = DateTime.Now;
            CompositionTarget.Rendering += OnRenderingFrame;
            _arcChangeTimer?.Start();
        }

        private void StopIndeterminateAnimation()
        {
            if (!_isIndeterminateAnimationActive) return;
            
            _isIndeterminateAnimationActive = false;
            CompositionTarget.Rendering -= OnRenderingFrame;
            _arcChangeTimer?.Stop();
        }

        private void OnRenderingFrame(object? sender, EventArgs e)
        {
            if (!_isIndeterminateAnimationActive || IndeterminateRotate == null) return;

            var currentTime = DateTime.Now;
            var elapsed = (currentTime - _lastRenderTime).TotalSeconds;
            _lastRenderTime = currentTime;

            // Smoothly transition to target arc length
            if (Math.Abs(_currentArcLength - _targetArcLength) > 0.5)
            {
                _currentArcLength = SmoothDamp(
                    _currentArcLength, 
                    _targetArcLength, 
                    ref _arcLengthVelocity, 
                    0.3, // Smoothing time
                    elapsed
                );
                UpdateIndeterminateArc();
            }

            // Adjust rotation speed based on arc length - shorter arcs look better with faster rotation
            double rotationSpeed = 180 + (90 - _currentArcLength); 
            IndeterminateRotate.Angle = (IndeterminateRotate.Angle + rotationSpeed * elapsed) % 360;
        }

        // SmoothDamp function for fluid transitions
        private double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double deltaTime)
        {
            double num = 2.0 / smoothTime;
            double num2 = num * deltaTime;
            double num3 = 1.0 / (1.0 + num2 + 0.48 * num2 * num2 + 0.235 * num2 * num2 * num2);
            double num4 = current - target;
            double num5 = target;
            double num6 = (currentVelocity + num * num4) * deltaTime;
            currentVelocity = (currentVelocity - num * num6) * num3;
            double num7 = target + (num4 + num6) * num3;
            
            if ((num5 - current > 0.0) == (num7 > num5))
            {
                num7 = num5;
                currentVelocity = (num7 - num5) / deltaTime;
            }
            
            return num7;
        }

        private void UpdateIndeterminateArc()
        {
            if (IndeterminatePath == null) return;
            
            double radius = 40 - (ArcThickness / 2);
            double startAngle = 0;
            double endAngle = _currentArcLength;
            
            Point startPoint = CalculatePoint(radius, startAngle);
            Point endPoint = CalculatePoint(radius, endAngle);

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false
            };

            ArcSegment arc = new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = endAngle - startAngle > 180
            };

            figure.Segments.Add(arc);
            geometry.Figures.Add(figure);
            IndeterminatePath.Data = geometry;
        }

        private Point CalculatePoint(double radius, double angle)
        {
            double radians = (angle - 90) * Math.PI / 180; // Offset by -90 to start at top
            double x = 50 + radius * Math.Cos(radians);
            double y = 50 + radius * Math.Sin(radians);
            return new Point(x, y);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(CircularProgressBar), 
                new PropertyMetadata(0.0, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CircularProgressBar)d;
            control.UpdateProgress();
        }

        public bool ShowText
        {
            get => (bool)GetValue(ShowTextProperty);
            set => SetValue(ShowTextProperty, value);
        }

        public static readonly DependencyProperty ShowTextProperty =
            DependencyProperty.Register("ShowText", typeof(bool), typeof(CircularProgressBar), 
                new PropertyMetadata(false, OnShowTextChanged));

        private static void OnShowTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CircularProgressBar)d;
            control.ProgressText.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(CircularProgressBar), 
                new PropertyMetadata(false, OnIndeterminateChanged));

        private static void OnIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CircularProgressBar)d;
            bool isIndeterminate = (bool)e.NewValue;
            
            control.ProgressPath.Visibility = isIndeterminate ? Visibility.Collapsed : Visibility.Visible;
            control.IndeterminatePath.Visibility = isIndeterminate ? Visibility.Visible : Visibility.Collapsed;
            control.ProgressText.Visibility = isIndeterminate ? Visibility.Collapsed : control.ShowText ? Visibility.Visible : Visibility.Collapsed;
            
            if (isIndeterminate)
            {
                // Initialize with random arc length
                control._currentArcLength = 30 + control._random.NextDouble() * 120;
                control._targetArcLength = control._currentArcLength;
                control.UpdateIndeterminateArc();
            }
            else
            {
                control.UpdateProgress();
            }
            
            control.UpdateAnimationState();
        }

        public double ArcThickness
        {
            get => (double)GetValue(ArcThicknessProperty);
            set => SetValue(ArcThicknessProperty, value);
        }

        public static readonly DependencyProperty ArcThicknessProperty =
            DependencyProperty.Register("ArcThickness", typeof(double), typeof(CircularProgressBar), 
                new PropertyMetadata(5.0, OnArcThicknessChanged));

        private static void OnArcThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CircularProgressBar)d;
            control.UpdateArcThickness();
        }

        private void UpdateArcThickness()
        {
            if (BackgroundCircle != null)
                BackgroundCircle.StrokeThickness = ArcThickness;
            
            if (ProgressPath != null)
                ProgressPath.StrokeThickness = ArcThickness;
            
            if (IndeterminatePath != null)
                IndeterminatePath.StrokeThickness = ArcThickness;
        }

        private void UpdateProgress()
        {
            if (IsIndeterminate) return;

            double radius = 40 - (ArcThickness / 2);
            double endAngle = Math.Min(Value, 100) / 100 * 360;
            
            // Calculate dynamic arc length based on progress
            double arcLength = 90; // Standard arc
            
            // Start with short arc that grows
            if (Value < 10) arcLength = 30 + Value * 6;
            
            // End with shrinking arc
            if (Value > 90) arcLength = 30 + (100 - Value) * 6;
            
            double startAngle = Math.Max(0, endAngle - arcLength);

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = CalculatePoint(radius, startAngle),
                IsClosed = false
            };

            ArcSegment arc = new ArcSegment
            {
                Point = CalculatePoint(radius, endAngle),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = endAngle - startAngle > 180
            };

            figure.Segments.Add(arc);
            geometry.Figures.Add(figure);
            ProgressPath.Data = geometry;
            
            if (ShowText)
            {
                ProgressText.Text = $"{Value:0}%";
            }
        }
    }
}
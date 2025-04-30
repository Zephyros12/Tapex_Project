using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Media.Imaging;
using System;


namespace Tapex_Project.Views.Controls
{
    public partial class ZoomableImage : UserControl
    {
        // 바인딩 가능한 이미지 소스 프로퍼티
        public static readonly StyledProperty<Bitmap?> SourceProperty =
            AvaloniaProperty.Register<ZoomableImage, Bitmap?>(nameof(Source));

        public Bitmap? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        const double MinScale = 1.0, MaxScale = 5.0;
        readonly ScaleTransform _scale = new ScaleTransform(MinScale, MinScale);
        readonly TranslateTransform _translate = new TranslateTransform();
        Point? _lastPanPoint;

        public ZoomableImage()
        {
            InitializeComponent();
            SetupTransformsAndEvents();
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);

        private void SetupTransformsAndEvents()
        {
            // 1) TransformGroup 구성
            var tg = new TransformGroup();
            tg.Children.Add(_scale);
            tg.Children.Add(_translate);
            this.FindControl<Image>("Image").RenderTransform = tg;
            this.FindControl<Image>("Image").RenderTransformOrigin =
                new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

            // 2) 이벤트 훅업 (Container가 투명 배경으로 입력 모두 받음)
            var container = this.FindControl<Border>("Container");
            container.PointerWheelChanged += OnPointerWheelChanged;
            container.PointerPressed += OnPointerPressed;
            container.PointerMoved += OnPointerMoved;
            container.PointerReleased += OnPointerReleased;

            // 3) ESC 키로 리셋 (포커스 가능하게)
            container.Focusable = true;
            container.AttachedToVisualTree += (_, __) => container.Focus();
            container.KeyDown += OnKeyDown;
        }

        private void OnPointerWheelChanged(object? s, PointerWheelEventArgs e)
        {
            var container = this.FindControl<Border>("Container");
            var img = this.FindControl<Image>("Image");
            var pos = e.GetPosition(container);

            // 줌 중심 설정
            if (container.Bounds.Width > 0 && container.Bounds.Height > 0)
                img.RenderTransformOrigin = new RelativePoint(
                    pos.X / container.Bounds.Width,
                    pos.Y / container.Bounds.Height,
                    RelativeUnit.Relative);

            // 확대/축소 계산
            double factor = e.Delta.Y > 0 ? 1.1 : 0.9;
            double newScale = Math.Clamp(_scale.ScaleX * factor, MinScale, MaxScale);
            double actual = newScale / _scale.ScaleX;
            _scale.ScaleX *= actual;
            _scale.ScaleY *= actual;

            // 스케일이 기본이면 리셋, 아니면 패닝 한계값 적용
            if (_scale.ScaleX <= MinScale)
            {
                _translate.X = _translate.Y = 0;
                img.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }
            else
            {
                ClampTranslation(container);
            }

            e.Handled = true;
        }

        private void OnPointerPressed(object? s, PointerPressedEventArgs e)
        {
            if (_scale.ScaleX > MinScale &&
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _lastPanPoint = e.GetPosition(this.FindControl<Border>("Container"));
            }
        }

        private void OnPointerMoved(object? s, PointerEventArgs e)
        {
            if (_lastPanPoint.HasValue)
            {
                var container = this.FindControl<Border>("Container");
                var current = e.GetPosition(container);
                _translate.X += current.X - _lastPanPoint.Value.X;
                _translate.Y += current.Y - _lastPanPoint.Value.Y;
                ClampTranslation(container);
                _lastPanPoint = current;
            }
        }

        private void OnPointerReleased(object? s, PointerReleasedEventArgs e)
        {
            _lastPanPoint = null;
        }

        private void OnKeyDown(object? s, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _scale.ScaleX = _scale.ScaleY = MinScale;
                _translate.X = _translate.Y = 0;
                this.FindControl<Image>("Image").RenderTransformOrigin =
                    new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                e.Handled = true;
            }
        }

        private void ClampTranslation(Border container)
        {
            double cw = container.Bounds.Width;
            double ch = container.Bounds.Height;
            double sw = cw * _scale.ScaleX;
            double sh = ch * _scale.ScaleY;
            double maxX = Math.Max((sw - cw) / 2, 0);
            double maxY = Math.Max((sh - ch) / 2, 0);

            _translate.X = Math.Clamp(_translate.X, -maxX, maxX);
            _translate.Y = Math.Clamp(_translate.Y, -maxY, maxY);
        }
    }
}

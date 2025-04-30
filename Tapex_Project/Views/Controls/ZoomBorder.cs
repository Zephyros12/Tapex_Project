// Views/Controls/ZoomBorder.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using System;

namespace Tapex_Project.Views.Controls
{
    public class ZoomBorder : Border
    {
        private const double MinScale = 1.0;
        private const double MaxScale = 5.0;

        private readonly ScaleTransform _scale;
        private readonly TranslateTransform _translate;
        private Point? _panStart;
        private bool _hasFitted = false;    // 한 번만 자동 Fit

        public ZoomBorder()
        {
            Background = Brushes.Transparent;
            ClipToBounds = true;

            _scale = new ScaleTransform(MinScale, MinScale);
            _translate = new TranslateTransform();
            var tg = new TransformGroup();
            tg.Children.Add(_scale);
            tg.Children.Add(_translate);
            RenderTransform = tg;
            RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

            // Layout이 결정된 후 한 번만 Fit 호출
            LayoutUpdated += (s, e) =>
            {
                if (!_hasFitted &&
                    Bounds.Width > 0 && Bounds.Height > 0 &&
                    Child is Control content &&
                    content.Bounds.Width > 0 && content.Bounds.Height > 0)
                {
                    // 컨테이너에 딱 맞추기
                    double fit = Math.Min(
                        Bounds.Width / content.Bounds.Width,
                        Bounds.Height / content.Bounds.Height);
                    // 최소 이하로는 확대 금지
                    fit = Math.Clamp(fit, MinScale, MaxScale);

                    _scale.ScaleX = _scale.ScaleY = fit;
                    _translate.X = _translate.Y = 0;
                    _hasFitted = true;
                }
            };

            PointerWheelChanged += OnWheel;
            PointerPressed += OnPressed;
            PointerMoved += OnMoved;
            PointerReleased += OnReleased;
            KeyDown += OnKeyDown;

            Focusable = true;
            AttachedToVisualTree += (_, __) => Focus();
        }

        private void OnWheel(object? s, PointerWheelEventArgs e)
        {
            var pos = e.GetPosition(this);
            RenderTransformOrigin = new RelativePoint(
                pos.X / Bounds.Width,
                pos.Y / Bounds.Height,
                RelativeUnit.Relative);

            double factor = e.Delta.Y > 0 ? 1.1 : 0.9;
            double newScale = Math.Clamp(_scale.ScaleX * factor, MinScale, MaxScale);
            double real = newScale / _scale.ScaleX;
            _scale.ScaleX *= real;
            _scale.ScaleY *= real;

            if (_scale.ScaleX <= MinScale)
            {
                _translate.X = _translate.Y = 0;
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }
            else
                Clamp();

            e.Handled = true;
        }

        private void OnPressed(object? s, PointerPressedEventArgs e)
        {
            if (_scale.ScaleX > MinScale &&
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _panStart = e.GetPosition(this);
            }
        }

        private void OnMoved(object? s, PointerEventArgs e)
        {
            if (_panStart.HasValue)
            {
                var pt = e.GetPosition(this);
                _translate.X += pt.X - _panStart.Value.X;
                _translate.Y += pt.Y - _panStart.Value.Y;
                Clamp();
                _panStart = pt;
            }
        }

        private void OnReleased(object? s, PointerReleasedEventArgs e)
            => _panStart = null;

        private void OnKeyDown(object? s, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _scale.ScaleX = _scale.ScaleY = MinScale;
                _translate.X = _translate.Y = 0;
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                e.Handled = true;
            }
        }

        private void Clamp()
        {
            double w = Bounds.Width;
            double h = Bounds.Height;
            double sw = w * _scale.ScaleX;
            double sh = h * _scale.ScaleY;
            double mx = Math.Max((sw - w) / 2, 0);
            double my = Math.Max((sh - h) / 2, 0);

            _translate.X = Math.Clamp(_translate.X, -mx, mx);
            _translate.Y = Math.Clamp(_translate.Y, -my, my);
        }
    }
}

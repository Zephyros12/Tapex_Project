using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Tapex_Project.Views
{
    public class ZoomBorder : Decorator
    {
        // 최소/최대 줌 스케일 설정
        private const double MinScale = 1;
        private const double MaxScale = 5.0;

        private readonly ScaleTransform _scale = new ScaleTransform(MinScale, MinScale);
        private readonly TranslateTransform _translate = new TranslateTransform();
        private Point? _lastPanPoint;

        public ZoomBorder()
        {
            // 스케일 및 패닝 Transform 그룹 설정
            RenderTransform = new TransformGroup
            {
                Children = new Transforms
                {
                    _scale,
                    _translate
                }
            };

            // 키보드 이벤트를 받을 수 있도록 포커스 가능 설정
            Focusable = true;

            // 이벤트 핸들러 등록
            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
            KeyDown += OnKeyDown;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                RenderTransformOrigin = new RelativePoint(
                    pos.X / Bounds.Width,
                    pos.Y / Bounds.Height,
                    RelativeUnit.Relative
                );
            }

            // 줌 인/아웃 계산
            double factor = e.Delta.Y > 0 ? 1.1 : 0.9;
            double newScale = Math.Clamp(_scale.ScaleX * factor, MinScale, MaxScale);
            double actualFactor = newScale / _scale.ScaleX;

            _scale.ScaleX *= actualFactor;
            _scale.ScaleY *= actualFactor;

            // 최소 스케일일 때 리셋, 그 외엔 패닝 범위 클램프
            if (_scale.ScaleX <= MinScale)
            {
                _translate.X = 0;
                _translate.Y = 0;
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }
            else
            {
                ClampTranslation();
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _scale.ScaleX > MinScale)
            {
                _lastPanPoint = e.GetPosition(this);
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_lastPanPoint.HasValue && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var pos = e.GetPosition(this);
                var dx = pos.X - _lastPanPoint.Value.X;
                var dy = pos.Y - _lastPanPoint.Value.Y;

                const double panSpeed = 1.5;
                _translate.X += dx * panSpeed;
                _translate.Y += dy * panSpeed;

                ClampTranslation();

                _lastPanPoint = pos;
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _lastPanPoint = null;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // 줌과 패닝 초기화
                _scale.ScaleX = MinScale;
                _scale.ScaleY = MinScale;
                _translate.X = 0;
                _translate.Y = 0;
                // 중앙 리셋
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                e.Handled = true;
            }
        }

        private void ClampTranslation()
        {
            double containerW = Bounds.Width;
            double containerH = Bounds.Height;
            double scaledW = containerW * _scale.ScaleX;
            double scaledH = containerH * _scale.ScaleY;

            double maxTransX = Math.Max((scaledW - containerW) / 2, 0);
            double maxTransY = Math.Max((scaledH - containerH) / 2, 0);

            _translate.X = Math.Clamp(_translate.X, -maxTransX, maxTransX);
            _translate.Y = Math.Clamp(_translate.Y, -maxTransY, maxTransY);
        }
    }
}
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Tapex_Project.Models;

namespace Tapex_Project.Views.Controls
{
    public class DefectOverlay : Canvas
    {
        // 1) 선택된 검출 결과
        public static readonly StyledProperty<DefectResult?> SelectedResultProperty =
            AvaloniaProperty.Register<DefectOverlay, DefectResult?>(
                nameof(SelectedResult),
                default(DefectResult?));

        public DefectResult? SelectedResult
        {
            get => GetValue(SelectedResultProperty);
            set => SetValue(SelectedResultProperty, value);
        }

        // 2) ViewModel에서 바인딩할 스케일 팩터
        public static readonly StyledProperty<double> ScaleProperty =
            AvaloniaProperty.Register<DefectOverlay, double>(
                nameof(Scale),
                1.0);

        /// <summary>
        /// FullMat → DisplayBitmap 비율 (예: 0.25 등)
        /// </summary>
        public double Scale
        {
            get => GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        // Dust만 주변을 더 크게 그리기 위한 패딩 (원래 픽셀 단위)
        private const double DustPadding = 5.0;

        public DefectOverlay()
        {
            // 속성 변경 시 다시 그리기
            this.GetObservable(SelectedResultProperty).Subscribe(_ => Update());
            this.GetObservable(ScaleProperty).Subscribe(_ => Update());

            // 초기 한 번
            Update();
        }

        private void Update()
        {
            // 1) 기존 이미지 자식만 보존
            var images = Children.OfType<Image>().ToList();
            Children.Clear();
            foreach (var img in images)
                Children.Add(img);

            // 2) 선택된 불량이 있으면 한 개만 그린다
            if (SelectedResult is { } d)
            {
                var shape = CreateShape(d);
                Children.Add(shape);
            }
        }

        private Shape CreateShape(DefectResult d)
        {
            // 타입별 색상
            var brush = d.Type switch
            {
                DefectType.Bubble => Brushes.Red,
                DefectType.Dust => Brushes.Blue,
                DefectType.Scratch => Brushes.Green,
                DefectType.Crack => Brushes.Orange,
                _ => Brushes.Gray
            };

            // 원래 픽셀 좌표 → 뷰어 픽셀 좌표
            double x = d.X * Scale;
            double y = d.Y * Scale;
            double w = d.Width * Scale;
            double h = d.Height * Scale;

            // Dust는 주변 패딩(원래 픽셀 단위)을 스케일 적용해서 더 크게
            if (d.Type == DefectType.Dust)
            {
                var pad = DustPadding * Scale;
                x -= pad;
                y -= pad;
                w += pad * 4;
                h += pad * 4;
            }

            var rect = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = brush,
                StrokeThickness = 2,
                Width = w,
                Height = h
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            return rect;
        }
    }
}

using System;
using System.Collections.Generic;
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

        // 전체 검사 결과 바인딩용
        public static readonly StyledProperty<IEnumerable<DefectResult>> AllResultsProperty =
            AvaloniaProperty.Register<DefectOverlay, IEnumerable<DefectResult>>(
                nameof(AllResults),
                Enumerable.Empty<DefectResult>());

        public IEnumerable<DefectResult> AllResults
        {
            get => GetValue(AllResultsProperty);
            set => SetValue(AllResultsProperty, value);
        }

        // 전체 보기 토글
        public static readonly StyledProperty<bool> ShowAllProperty =
            AvaloniaProperty.Register<DefectOverlay, bool>(
                nameof(ShowAll),
                false);

        public bool ShowAll
        {
            get => GetValue(ShowAllProperty);
            set => SetValue(ShowAllProperty, value);
        }

        public DefectOverlay()
        {
            // 속성 변경 시 다시 그리기
            this.GetObservable(SelectedResultProperty).Subscribe(_ => Update());
            this.GetObservable(ScaleProperty).Subscribe(_ => Update());
            this.GetObservable(AllResultsProperty).Subscribe(_ => Update());
            this.GetObservable(ShowAllProperty).Subscribe(_ => Update());

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

            // 2) 체크박스에 따라 모두 또는 선택된 것만 그리기
            if (ShowAll)
            {
                foreach (var d in AllResults)
                    Children.Add(CreateShape(d));
            }
            else if (SelectedResult is { } d)
            {
                Children.Add(CreateShape(d));
            }

            // 3) 선택된 하나만 코너 마커로 강조
            if (ShowAll && SelectedResult is { } sel)
            {
                foreach (var marker in CreateHighlight(sel))
                    Children.Add(marker);
            }
        }

        // 기존 사각형을 그리는 부분 (변경 없음)
        private Shape CreateShape(DefectResult d)
        {
            var brush = d.Type switch
            {
                DefectType.Bubble => Brushes.Red,
                DefectType.Dust => Brushes.Blue,
                DefectType.Scratch => Brushes.Yellow,
                DefectType.Crack => Brushes.Orange,
                _ => Brushes.Gray
            };

            double x = d.X * Scale;
            double y = d.Y * Scale;
            double w = d.Width * Scale;
            double h = d.Height * Scale;

            if (d.Type == DefectType.Dust)
            {
                var pad = DustPadding * Scale;
                x -= pad;
                y -= pad;
                w += pad * 50;
                h += pad * 50;
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

        // 선택된 ROI 위에 반투명 채우기 + 코너 마커(빨간 선) 생성
        private IEnumerable<Shape> CreateHighlight(DefectResult sel)
        {
            double x = sel.X * Scale;
            double y = sel.Y * Scale;
            double w = sel.Width * Scale;
            double h = sel.Height * Scale;

            // (1) 반투명 흰색 채우기
            var overlay = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.White) { Opacity = 0.2 },
                Width = w,
                Height = h
            };
            Canvas.SetLeft(overlay, x);
            Canvas.SetTop(overlay, y);
            yield return overlay;

            // (2) 코너 마커 길이: 20% or 최소 6px
            double markerLen = Math.Max(Math.Min(w, h) * 0.2, 6);
            var pen = new Pen(Brushes.White, 4);

            // top-left
            yield return LineAt(x, y, x + markerLen, y, pen);
            yield return LineAt(x, y, x, y + markerLen, pen);

            // top-right
            yield return LineAt(x + w, y, x + w - markerLen, y, pen);
            yield return LineAt(x + w, y, x + w, y + markerLen, pen);

            // bottom-left
            yield return LineAt(x, y + h, x + markerLen, y + h, pen);
            yield return LineAt(x, y + h, x, y + h - markerLen, pen);

            // bottom-right
            yield return LineAt(x + w, y + h, x + w - markerLen, y + h, pen);
            yield return LineAt(x + w, y + h, x + w, y + h - markerLen, pen);
        }

        // 편의 함수: 절대좌표 Line 생성
        private static Line LineAt(double x1, double y1, double x2, double y2, Pen pen)
        {
            return new Line
            {
                StartPoint = new Point(x1, y1),
                EndPoint = new Point(x2, y2),
                Stroke = pen.Brush,
                StrokeThickness = pen.Thickness
            };
        }
    }
}

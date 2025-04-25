using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;

namespace Tapex_Project.Models
{
    /// <summary>
    /// 전체 검사 흐름을 수행하는 Orchestrator (병렬 타일 기반)
    /// </summary>
    public sealed class Detector
    {
        private const int TileSize = 2048;
        private const int Overlap = 128;

        private readonly ISubDetector[] _subDetectors;
        private readonly IProcessingOutputService _outputService;

        public Detector(ISubDetector[] subDetectors,
                        IProcessingOutputService outputService)
        {
            _subDetectors = subDetectors;
            _outputService = outputService;
        }

        /// <summary>
        /// 컬러 Mat을 받아서 그레이 변환 → 타일 분할 → 서브-검출기 실행 → 결과 필터링
        /// </summary>
        public IReadOnlyList<DefectResult> Run(Mat srcColor, DetectionConfig cfg)
        {
            // 출력 디렉터리 초기화
            _outputService.Initialize();

            // 1) Gray 변환
            using var gray = new Mat();
            CvInvoke.CvtColor(srcColor, gray, ColorConversion.Bgr2Gray);

            // 2) (디버깅용) 전체 Gray 저장
            _outputService.SaveMat("00_gray.png", gray);

            // 3) 타일 분할
            var rects = CreateTiles(gray.Width, gray.Height);

            // 4) 병렬 타일 검사
            var bag = new ConcurrentBag<DefectResult>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
            };

            Parallel.ForEach(
                Partitioner.Create(rects, EnumerablePartitionerOptions.NoBuffering),
                options,
                rect =>
                {
                    using var tile = new Mat(gray, rect);

                    foreach (var sub in _subDetectors)
                    {
                        foreach (var r in sub.Run(tile, cfg))
                        {
                            bag.Add(new DefectResult
                            {
                                X = r.X + rect.X,
                                Y = r.Y + rect.Y,
                                Width = r.Width,
                                Height = r.Height,
                                Score = r.Score,
                                Type = r.Type,
                                DistanceFromEdge = r.DistanceFromEdge
                            });
                        }
                    }
                });

            // 5) mm→픽셀 환산 후 필터링
            var allResults = bag.ToArray();
            int minPx = (int)Math.Ceiling(
                cfg.MinDefectSizeMm * 1000.0   // mm → μm
                / cfg.PixelSizeMicrometer      // μm 당 픽셀 수
            );

            var filtered = allResults
                .Where(r => Math.Max(r.Width, r.Height) >= minPx)
                .ToArray();

            return filtered;
        }

        private static IEnumerable<System.Drawing.Rectangle> CreateTiles(int width, int height)
        {
            for (int y = 0; y < height; y += TileSize - Overlap)
                for (int x = 0; x < width; x += TileSize - Overlap)
                {
                    int w = Math.Min(TileSize, width - x);
                    int h = Math.Min(TileSize, height - y);
                    yield return new System.Drawing.Rectangle(x, y, w, h);
                }
        }
    }
}

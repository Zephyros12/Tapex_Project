using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;
using Avalonia.Media.Imaging;
using Emgu.CV.Structure;

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
        /// 컬러 Mat을 받아서
        ///  1) 그레이 변환
        ///  2) 타일 분할
        ///  3) 서브-검출기 실행
        ///  4) 상세 정보(밝기/선명도/크기/프리뷰) 추가
        ///  5) 반환
        /// </summary>
        public IReadOnlyList<DefectResult> Run(Mat srcColor, DetectionConfig cfg)
        {
            // 0) 출력 디렉터리 초기화
            _outputService.Initialize();

            // 1) 그레이 변환
            using var gray = new Mat();
            CvInvoke.CvtColor(srcColor, gray, ColorConversion.Bgr2Gray);
            _outputService.SaveMat("00_gray.png", gray);

            // 2) 타일 분할
            var rects = CreateTiles(gray.Width, gray.Height);

            // 3) 병렬로 서브-검출기 실행
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
                                DistanceFromEdge = r.DistanceFromEdge,
                                Score = r.Score,
                                Type = r.Type
                            });
                        }
                    }
                });

            // 4) 기본 결과 수집
            var rawResults = bag.ToArray();
            var detailed = new List<DefectResult>(rawResults.Length);

            // 5) 각 결과에 밝기/선명도/크기(mm)/프리뷰 추가
            foreach (var r in rawResults)
            {
                var rect = new System.Drawing.Rectangle(
                    x: (int)Math.Round(r.X), 
                    y: (int)Math.Round(r.Y), 
                    width: (int)Math.Round(r.Width), 
                    height: (int)Math.Round(r.Height));

                // 밝기: ROI 그레이 평균
                using var roiGray = new Mat(gray, rect);
                var meanBri = CvInvoke.Mean(roiGray).V0;

                // 선명도: Laplacian 분산
                using var lap = new Mat();
                CvInvoke.Laplacian(roiGray, lap, DepthType.Cv64F);
                var meanLap = new MCvScalar();
                var stddevLap = new MCvScalar();
                CvInvoke.MeanStdDev(lap, ref meanLap, ref stddevLap);
                var varLap = stddevLap.V0 * stddevLap.V0;
                // 크기(mm)
                double sizeMm = Math.Max(r.Width, r.Height)
                                * cfg.PixelSizeMicrometer
                                / 1000.0;

                // 프리뷰 이미지: 컬러 ROI → PNG 스트림 → Avalonia.Bitmap
                using var roiColor = new Mat(srcColor, rect);
                var preview = ConvertMatToBitmap(roiColor);

                detailed.Add(new DefectResult
                {
                    X = r.X,
                    Y = r.Y,
                    Width = r.Width,
                    Height = r.Height,
                    DistanceFromEdge = r.DistanceFromEdge,
                    Score = r.Score,
                    Type = r.Type,
                    Brightness = meanBri,
                    Sharpness = varLap,
                    SizeMm = sizeMm,
                    PreviewImage = preview
                });
            }

            return detailed;
        }

        /// <summary>
        /// Mat을 PNG로 인코딩해 Avalonia.Bitmap으로 반환
        /// </summary>
        private static Bitmap ConvertMatToBitmap(Mat mat)
        {
            var vb = new VectorOfByte();
            CvInvoke.Imencode(".png", mat, vb);
            using var ms = new MemoryStream(vb.ToArray());
            ms.Seek(0, SeekOrigin.Begin);
            return new Bitmap(ms);
        }

        /// <summary>
        /// 이미지 크기에 맞춰 타일 영역 생성 (겹침 포함)
        /// </summary>
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

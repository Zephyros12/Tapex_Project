using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;
using Emgu.CV.Structure;

namespace Tapex_Project.Models
{
    public sealed class Detector
    {
        private const int TileSize = 2048;
        private const int Overlap = 128;

        private readonly ISubDetector[] _subDetectors;
        private readonly IProcessingOutputService _outputService;
        private readonly IPreprocessingService _preprocessor;

        public Detector(ISubDetector[] subDetectors,
                        IProcessingOutputService outputService,
                        IPreprocessingService preprocessor)
        {
            _subDetectors = subDetectors;
            _outputService = outputService;
            _preprocessor = preprocessor;
        }

        public IReadOnlyList<DefectResult> Run(Mat srcColor, DetectionConfig cfg)
        {
            _outputService.Initialize();

            // ─ 전체 영상 전처리 결과 저장 ─
            using var grayPreview = new Mat();
            CvInvoke.CvtColor(srcColor, grayPreview, ColorConversion.Bgr2Gray);
            _outputService.SaveMat("Full_01_Gray.png", grayPreview);

            // 전체 마스크 (Blob) 생성 및 저장
            var fullMask = _preprocessor.ExtractLargestBlobMask(grayPreview);
            _outputService.SaveMat("Full_02_Mask.png", fullMask);

            // Dust용 이진화 저장
            using var fullDustBin = new Mat();
            CvInvoke.Threshold(
                grayPreview, fullDustBin,
                cfg.Dust.Threshold, 255,
                ThresholdType.Binary);
            _outputService.SaveMat("Full_03_DustBinary.png", fullDustBin);

            // Scratch/Crack용 Canny 에지 저장
            using var fullEdges = new Mat();
            CvInvoke.Canny(
                grayPreview, fullEdges,
                cfg.Scratch.CannyThreshold1,
                cfg.Scratch.CannyThreshold2);
            _outputService.SaveMat("Full_04_Canny.png", fullEdges);

            // 스켈레톤화 저장
            using var fullSkel = SkeletonUtils.Thinning(fullEdges);
            _outputService.SaveMat("Full_05_Skeleton.png", fullSkel);


            // 1) 그레이 변환
            using var gray = new Mat();
            CvInvoke.CvtColor(srcColor, gray, ColorConversion.Bgr2Gray);
            _outputService.SaveMat("00_gray.png", gray);

            // 2) 타일 리스트 미리 생성
            var allRects = CreateTiles(gray.Width, gray.Height).ToList();

            // 3) 병렬 검출
            var bag = new ConcurrentBag<DefectResult>();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
            };

            Parallel.ForEach(
                Partitioner.Create(allRects, EnumerablePartitionerOptions.NoBuffering),
                options,
                rect =>
                {
                    try
                    {
                        var safeTileRect = Rectangle.Intersect(new Rectangle(0, 0, gray.Width, gray.Height), rect);
                        if (safeTileRect.Width <= 0 || safeTileRect.Height <= 0)
                            return;

                        using var tile = new Mat(gray, safeTileRect);

                        foreach (var sub in _subDetectors)
                        {
                            _outputService.SaveMat($"{sub.GetType().Name}_Input_{safeTileRect.X}_{safeTileRect.Y}.png", tile);

                            try
                            {
                                foreach (var r in sub.Run(tile, cfg))
                                {
                                    bag.Add(new DefectResult
                                    {
                                        X = r.X + safeTileRect.X,
                                        Y = r.Y + safeTileRect.Y,
                                        Width = r.Width,
                                        Height = r.Height,
                                        DistanceFromEdge = r.DistanceFromEdge,
                                        Score = r.Score,
                                        Type = r.Type,
                                        PreprocessedImage = r.PreprocessedImage
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                // 필요에 따라 로그 남기기
                                Console.WriteLine($"[Error] {sub.GetType().Name} on {safeTileRect}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] creating tile {rect}: {ex.Message}");
                    }
                });

            // 4) 상세 정보 추가
            var rawResults = bag.ToArray();

            double pxToMm = cfg.PixelSizeMicrometer / 1000.0;

            rawResults = rawResults.Where(r =>
            {
                double wMm = r.Width * pxToMm;
                double hMm = r.Height * pxToMm;
                return wMm >= cfg.MinDefectWidthMm &&
                       wMm <= cfg.MaxDefectWidthMm &&
                       hMm >= cfg.MinDefectHeightMm &&
                       hMm <= cfg.MaxDefectHeightMm;
            }).ToArray();

            var detailed = new List<DefectResult>(rawResults.Length);

            foreach (var r in rawResults)
            {
                // 원본 좌표 사각형
                var rawRect = new Rectangle(
                    x: (int)Math.Round(r.X),
                    y: (int)Math.Round(r.Y),
                    width: (int)Math.Round(r.Width),
                    height: (int)Math.Round(r.Height));

                // 전체 이미지 범위와 교차
                var grayBounds = new Rectangle(0, 0, gray.Width, gray.Height);
                var colorBounds = new Rectangle(0, 0, srcColor.Width, srcColor.Height);
                var safeGrayRect = Rectangle.Intersect(grayBounds, rawRect);
                var safeColorRect = Rectangle.Intersect(colorBounds, rawRect);
                if (safeGrayRect.Width <= 0 || safeGrayRect.Height <= 0 ||
                    safeColorRect.Width <= 0 || safeColorRect.Height <= 0)
                {
                    // 유효하지 않은 ROI는 건너뛰기
                    continue;
                }

                // 밝기
                using var roiGray = new Mat(gray, safeGrayRect);
                double meanBri = CvInvoke.Mean(roiGray).V0;

                // 선명도
                using var lap = new Mat();
                CvInvoke.Laplacian(roiGray, lap, DepthType.Cv64F);
                var meanLap = new MCvScalar();
                var stddevLap = new MCvScalar();
                CvInvoke.MeanStdDev(lap, ref meanLap, ref stddevLap);
                double varLap = stddevLap.V0 * stddevLap.V0;

                // 크기 (mm)
                double sizeMm = Math.Max(r.Width, r.Height)
                                * cfg.PixelSizeMicrometer
                                / 1000.0;

                // 프리뷰
                const int previewPaddingPx = 50;
                int x0 = Math.Max(rawRect.X - previewPaddingPx, 0);
                int y0 = Math.Max(rawRect.Y - previewPaddingPx, 0);
                int x1 = Math.Min(rawRect.Right + previewPaddingPx, srcColor.Width);
                int y1 = Math.Min(rawRect.Bottom + previewPaddingPx, srcColor.Height);

                int w = x1 - x0;
                int h = y1 - y0;
                if (w <= 0 || h <= 0)
                    continue;

                var contextRect = new Rectangle(x0, y0, w, h);
                using var contextMat = new Mat(srcColor, contextRect);

                var overlayRect = new Rectangle(
                    rawRect.X - x0, rawRect.Y - y0, rawRect.Width, rawRect.Height);
                CvInvoke.Rectangle(contextMat, overlayRect, new MCvScalar(0, 255, 255), thickness: 2);

                var preview = ConvertMatToBitmap(contextMat);

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
                    PixelSizeMicrometer = cfg.PixelSizeMicrometer,
                    PreviewImage = preview,
                    PreprocessedImage = r.PreprocessedImage,

                });
            }

            return detailed;
        }

        private static AvaloniaBitmap ConvertMatToBitmap(Mat mat)
        {
            using var vb = new VectorOfByte();
            CvInvoke.Imencode(".png", mat, vb);
            using var ms = new MemoryStream(vb.ToArray());
            ms.Seek(0, SeekOrigin.Begin);
            return new AvaloniaBitmap(ms);
        }

        private static IEnumerable<Rectangle> CreateTiles(int width, int height)
        {
            for (int y = 0; y < height; y += TileSize - Overlap)
            {
                for (int x = 0; x < width; x += TileSize - Overlap)
                {
                    int w = Math.Min(TileSize, width - x);
                    int h = Math.Min(TileSize, height - y);
                    yield return new Rectangle(x, y, w, h);
                }
            }
        }
    }
}

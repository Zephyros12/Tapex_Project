using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Tapex_Project.Models.Detection;

namespace Tapex_Project.Models
{
    /// <summary>
    /// 전체 검사 흐름을 수행하는 Orchestrator
    /// - 전역 평탄화
    /// - 반원 자동 검출 및 ROI 마스킹
    /// - 타일 기반 병렬 서브-검출
    /// </summary>
    public sealed class Detector
    {
        private const int TileSize = 2048;
        private const int Overlap = 128;

        private readonly ISubDetector[] _subDetectors =
        {
            new BubbleDetector(),
            //new ScratchDetector(),
            //new DustDetector(),
            //new CrackDetector()
        };

        /// <summary>
        /// Bitmap → Mat 변환 후, 단계별 이미지 저장(디버깅) 및 병렬 서브-검출 실행
        /// </summary>
        public IReadOnlyList<DefectResult> Run(Bitmap bmp, DetectionConfig cfg)
        {
            // 디버깅용: 폴더 생성
            var baseDir = Path.Combine(AppContext.BaseDirectory, "ProcessingOutputs");
            Directory.CreateDirectory(baseDir);
            var runDir = Path.Combine(baseDir, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(runDir);

            // 파일 저장 헬퍼
            void SaveMat(string name, Mat m) => CvInvoke.Imwrite(Path.Combine(runDir, name), m);

            // 1) Bitmap → BGR Mat
            using var srcColor = BitmapToMat(bmp);
            SaveMat("00_srcColor.png", srcColor);

            // 2) BGR → Gray
            using var gray = new Mat();
            CvInvoke.CvtColor(srcColor, gray, ColorConversion.Bgr2Gray);
            SaveMat("01_gray.png", gray);

            // 3) 글로벌 평탄화
            int globalPx = (int)Math.Round(UnitHelper.MmToPx(cfg.Circle.FlattenRadiusMm));
            using var flat = Preprocess.FlattenBackground(gray, globalPx);
            SaveMat("02_flatGlobal.png", flat);

            // 4) 반원 검출 및 ROI 마스킹
            var circle = SemiCircleLocator.DetectClippedCircle(flat)
                         ?? throw new InvalidOperationException("반원 검출 실패: Circle 파라미터 조정 필요");
            using var mask = RoiMask.CreateSemicircleMask(srcColor.Width, srcColor.Height, circle);
            SaveMat("03_mask.png", mask);

            using var roi = new Mat();
            CvInvoke.BitwiseAnd(flat, flat, roi, mask);
            SaveMat("04_roiGray.png", roi);

            // 5) 타일 기반 병렬 서브-검출
            int width = roi.Width;
            int height = roi.Height;

            var rects = new List<Rectangle>();
            for (int y = 0; y < height; y += TileSize - Overlap)
                for (int x = 0; x < width; x += TileSize - Overlap)
                {
                    int w = Math.Min(TileSize, width - x);
                    int h = Math.Min(TileSize, height - y);
                    rects.Add(new Rectangle(x, y, w, h));
                }

            var resultsBag = new ConcurrentBag<DefectResult>();
            Parallel.ForEach(rects, rect =>
            {
                using var tileMat = new Mat(roi, rect);
                // 디버깅용: 타일 저장
                SaveMat($"TILE_{rect.X}_{rect.Y}.png", tileMat);

                foreach (var det in _subDetectors)
                {
                    var list = det.Run(tileMat, cfg);
                    foreach (var r in list)
                    {
                        resultsBag.Add(new DefectResult
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

            return resultsBag.ToArray();
        }

        /// <summary>
        /// Bitmap → Emgu CV Mat 변환 헬퍼
        /// </summary>
        private static Mat BitmapToMat(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms);
            var data = ms.ToArray();
            var mat = new Mat();
            CvInvoke.Imdecode(data, ImreadModes.Color, mat);
            return mat;
        }
    }
}

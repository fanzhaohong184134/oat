using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using dsat.DataProcessing.Calibration;

namespace dsat.DataProcessing.PostProcessing
{
    public sealed class NorthDirectionImageAnnotatorResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string OutputDirectory { get; set; }
        public string ManifestCsvPath { get; set; }
    }

    public static class NorthDirectionImageAnnotator
    {
        private const int ExifUserCommentTag = 0x9286;

        public static NorthDirectionImageAnnotatorResult AnnotateFrames(
            System.Collections.Generic.IEnumerable<SyncedFrame> frames,
            CalibrationConfig config,
            string outputDirectory,
            bool drawDetectedCircleOutline,
            bool metadataOnlyMode)
        {
            if (frames == null) throw new ArgumentNullException("frames");
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentNullException("outputDirectory");

            Directory.CreateDirectory(outputDirectory);
            var result = new NorthDirectionImageAnnotatorResult { OutputDirectory = outputDirectory };
            var csv = new StringBuilder();
            csv.AppendLine("index,timestamp,file_name,source_path,output_path,north_deg,origin_x,origin_y,circle_detected,status,message");
            int rowIndex = 0;

            foreach (var frame in frames)
            {
                try
                {
                    if (frame == null || frame.Camera == null || frame.InterpolatedImu == null)
                    {
                        result.FailedCount++;
                        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "{0},,,,,,,,,FAIL,Invalid frame or missing camera/imu data", rowIndex++));
                        continue;
                    }

                    string src = frame.Camera.FilePath;
                    if (string.IsNullOrWhiteSpace(src) || !File.Exists(src))
                    {
                        result.FailedCount++;
                        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "{0},{1},\"{2}\",\"{3}\",,,,,FAIL,Source image not found", rowIndex++,
                            frame.Camera.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                            EscapeCsv(frame.Camera.FileName),
                            EscapeCsv(src ?? string.Empty)));
                        continue;
                    }

                    using (var original = new Bitmap(src))
                    using (var canvas = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb))
                    using (var g = Graphics.FromImage(canvas))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(original, 0, 0, original.Width, original.Height);

                        PointF origin;
                        float detectedRadius;
                        bool circleDetected = TryDetectCircleCenter(original, out origin, out detectedRadius);
                        if (!circleDetected)
                        {
                            origin = new PointF(canvas.Width / 2f, canvas.Height / 2f);
                            detectedRadius = Math.Max(18f, Math.Min(canvas.Width, canvas.Height) * 0.05f);
                        }

                        double northDeg = NormalizeDegrees(-(frame.InterpolatedImu.AngleZ + config.MagneticDeclination + config.PsiOffset));
                        if (!metadataOnlyMode)
                        {
                            DrawNorthArrow(g, canvas.Width, canvas.Height, northDeg, origin, detectedRadius, circleDetected, drawDetectedCircleOutline);
                        }

                        string outputPath = GetOutputPath(outputDirectory, frame.Camera.FileName, result.SuccessCount);
                        SaveJpegWithNorthMetadata(canvas, outputPath, northDeg, origin, circleDetected, metadataOnlyMode);
                        result.SuccessCount++;

                        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "{0},{1},\"{2}\",\"{3}\",\"{4}\",{5:F3},{6:F1},{7:F1},{8},OK,",
                            rowIndex++,
                            frame.Camera.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                            EscapeCsv(frame.Camera.FileName),
                            EscapeCsv(src),
                            EscapeCsv(outputPath),
                            northDeg,
                            origin.X,
                            origin.Y,
                            circleDetected ? "Y" : "N"));
                    }
                }
                catch
                {
                    result.FailedCount++;
                    csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},,,,,,,,,FAIL,Unexpected error during image annotation", rowIndex++));
                }
            }

            result.ManifestCsvPath = Path.Combine(
                outputDirectory,
                "north_direction_manifest_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".csv");
            File.WriteAllText(result.ManifestCsvPath, csv.ToString(), Encoding.UTF8);

            return result;
        }

        private static void DrawNorthArrow(Graphics g, int width, int height, double northDeg, PointF origin, float detectedRadius, bool circleDetected, bool drawDetectedCircleOutline)
        {
            float cx = origin.X;
            float cy = origin.Y;
            float radius = Math.Max(30f, Math.Min(width, height) * 0.18f);
            double rad = northDeg * Math.PI / 180.0;
            float tipX = cx + (float)(radius * Math.Sin(rad));
            float tipY = cy - (float)(radius * Math.Cos(rad));

            using (var halo = new Pen(Color.FromArgb(180, 0, 0, 0), 7f))
            using (var pen = new Pen(Color.FromArgb(245, 24, 24), 4f))
            using (var ring = new Pen(Color.FromArgb(210, 255, 255, 255), 2f))
            using (var brush = new SolidBrush(Color.FromArgb(245, 24, 24)))
            using (var font = new Font("Microsoft YaHei UI", Math.Max(11f, Math.Min(width, height) * 0.03f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (var textBrush = new SolidBrush(Color.White))
            {
                // Origin ring: circle center if detected, otherwise image center fallback.
                g.DrawEllipse(ring, cx - 6f, cy - 6f, 12f, 12f);
                if (circleDetected && drawDetectedCircleOutline)
                {
                    float rr = Math.Max(10f, Math.Min(40f, detectedRadius));
                    using (var cp = new Pen(Color.FromArgb(180, 255, 208, 0), 2f))
                    {
                        g.DrawEllipse(cp, cx - rr, cy - rr, rr * 2f, rr * 2f);
                    }
                }
                g.DrawLine(halo, cx, cy, tipX, tipY);
                g.DrawLine(pen, cx, cy, tipX, tipY);

                var ux = tipX - cx;
                var uy = tipY - cy;
                var len = (float)Math.Sqrt(ux * ux + uy * uy);
                if (len < 1f) len = 1f;
                ux /= len;
                uy /= len;

                float arrowSize = Math.Max(10f, radius * 0.16f);
                var leftX = tipX - arrowSize * ux + arrowSize * 0.55f * uy;
                var leftY = tipY - arrowSize * uy - arrowSize * 0.55f * ux;
                var rightX = tipX - arrowSize * ux - arrowSize * 0.55f * uy;
                var rightY = tipY - arrowSize * uy + arrowSize * 0.55f * ux;
                g.FillPolygon(brush, new[]
                {
                    new PointF(tipX, tipY),
                    new PointF(leftX, leftY),
                    new PointF(rightX, rightY)
                });

                string label = "N " + northDeg.ToString("F1", CultureInfo.InvariantCulture) + "°";
                var labelSize = g.MeasureString(label, font);
                float tx = tipX + 6f;
                float ty = tipY - labelSize.Height - 4f;
                if (tx + labelSize.Width > width - 4f) tx = width - labelSize.Width - 4f;
                if (ty < 2f) ty = 2f;

                using (var bg = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                {
                    g.FillRectangle(bg, tx - 2f, ty - 1f, labelSize.Width + 4f, labelSize.Height + 2f);
                }
                g.DrawString(label, font, textBrush, tx, ty);
            }
        }

        private static void SaveJpegWithNorthMetadata(Bitmap bitmap, string outputPath, double northDeg, PointF origin, bool circleDetected, bool metadataOnlyMode)
        {
            var codec = GetJpegCodec();
            if (codec != null)
            {
                try
                {
                    var quality = new EncoderParameters(1);
                    quality.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
                    bitmap.Save(outputPath, codec, quality);
                }
                catch
                {
                    bitmap.Save(outputPath, ImageFormat.Jpeg);
                }
            }
            else
            {
                bitmap.Save(outputPath, ImageFormat.Jpeg);
            }

            // Best-effort EXIF write with no interruption to processing.
            TryWriteExifUserComment(outputPath, northDeg, origin, circleDetected, metadataOnlyMode);
        }

        private static void TryWriteExifUserComment(string outputPath, double northDeg, PointF origin, bool circleDetected, bool metadataOnlyMode)
        {
            string payload = string.Format(CultureInfo.InvariantCulture,
                "NorthDirectionDeg={0:F3};OriginX={1:F1};OriginY={2:F1};CircleDetected={3};MetadataOnly={4}",
                northDeg,
                origin.X,
                origin.Y,
                circleDetected ? "Y" : "N",
                metadataOnlyMode ? "Y" : "N");
            byte[] exif = System.Text.Encoding.ASCII.GetBytes("ASCII\0\0\0" + payload + "\0");

            try
            {
                using (var image = Image.FromFile(outputPath))
                {
                    PropertyItem template = null;
                    foreach (var p in image.PropertyItems)
                    {
                        template = p;
                        break;
                    }
                    if (template == null)
                        return;

                    var item = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
                    item.Id = ExifUserCommentTag;
                    item.Type = 7;
                    item.Len = exif.Length;
                    item.Value = exif;

                    image.SetPropertyItem(item);

                    var codec = GetJpegCodec();
                    if (codec != null)
                    {
                        var quality = new EncoderParameters(1);
                        quality.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
                        image.Save(outputPath, codec, quality);
                    }
                    else
                    {
                        image.Save(outputPath, ImageFormat.Jpeg);
                    }
                }
            }
            catch
            {
            }
        }

        private static string GetOutputPath(string outputDir, string originalFileName, int index)
        {
            string safeName = string.IsNullOrWhiteSpace(originalFileName)
                ? "frame_" + index.ToString("D4", CultureInfo.InvariantCulture) + ".jpg"
                : Path.GetFileNameWithoutExtension(originalFileName) + "_north.jpg";
            if (!safeName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                safeName += ".jpg";
            return Path.Combine(outputDir, safeName);
        }

        private static double NormalizeDegrees(double deg)
        {
            deg %= 360.0;
            if (deg < 0) deg += 360.0;
            return deg;
        }

        private static string EscapeCsv(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\"\"");
        }

        private static bool TryDetectCircleCenter(Bitmap original, out PointF center, out float radius)
        {
            center = new PointF(original.Width / 2f, original.Height / 2f);
            radius = 0f;

            const int detectMaxSide = 960;
            double scale = 1.0;
            int w = original.Width;
            int h = original.Height;
            if (Math.Max(w, h) > detectMaxSide)
            {
                scale = detectMaxSide / (double)Math.Max(w, h);
                w = Math.Max(64, (int)Math.Round(original.Width * scale));
                h = Math.Max(64, (int)Math.Round(original.Height * scale));
            }

            using (var small = new Bitmap(w, h, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(small))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.DrawImage(original, 0, 0, w, h);

                byte[,] gray = BuildGray(small);
                int threshold = OtsuThreshold(gray, w, h);

                CircleCandidate best = FindBestCircle(gray, w, h, threshold, true);
                CircleCandidate alt = FindBestCircle(gray, w, h, threshold, false);

                CircleCandidate winner = PickBetter(best, alt);
                if (winner == null || winner.Score < 0.52f)
                    return false;

                float invScale = (float)(1.0 / scale);
                center = new PointF(winner.CenterX * invScale, winner.CenterY * invScale);
                radius = winner.Radius * invScale;
                return true;
            }
        }

        private sealed class CircleCandidate
        {
            public float CenterX;
            public float CenterY;
            public float Radius;
            public float Score;
            public int Area;
        }

        private static CircleCandidate PickBetter(CircleCandidate a, CircleCandidate b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return a.Score >= b.Score ? a : b;
        }

        private static CircleCandidate FindBestCircle(byte[,] gray, int w, int h, int threshold, bool darkTarget)
        {
            bool[,] mask = new bool[w, h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte g = gray[x, y];
                    mask[x, y] = darkTarget ? (g < threshold) : (g > threshold);
                }
            }

            bool[,] visited = new bool[w, h];
            int minArea = Math.Max(120, (w * h) / 1800);
            int maxArea = (w * h) / 2;

            CircleCandidate best = null;
            var q = new Queue<Point>();
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (visited[x, y] || !mask[x, y]) continue;

                    int area = 0;
                    long sumX = 0, sumY = 0;
                    int minX = x, maxX = x, minY = y, maxY = y;
                    int perimeter = 0;

                    visited[x, y] = true;
                    q.Enqueue(new Point(x, y));

                    while (q.Count > 0)
                    {
                        Point p = q.Dequeue();
                        area++;
                        sumX += p.X;
                        sumY += p.Y;
                        if (p.X < minX) minX = p.X;
                        if (p.X > maxX) maxX = p.X;
                        if (p.Y < minY) minY = p.Y;
                        if (p.Y > maxY) maxY = p.Y;

                        bool boundary = false;
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = p.X + dx[i];
                            int ny = p.Y + dy[i];
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                boundary = true;
                                continue;
                            }
                            if (!mask[nx, ny])
                            {
                                boundary = true;
                                continue;
                            }
                            if (!visited[nx, ny])
                            {
                                visited[nx, ny] = true;
                                q.Enqueue(new Point(nx, ny));
                            }
                        }
                        if (boundary) perimeter++;
                    }

                    if (area < minArea || area > maxArea || perimeter < 20)
                        continue;

                    float bw = (maxX - minX + 1);
                    float bh = (maxY - minY + 1);
                    if (bw < 8 || bh < 8)
                        continue;

                    float ratio = bw / bh;
                    float ratioScore = 1f - Math.Min(1f, Math.Abs(ratio - 1f));
                    if (ratioScore < 0.72f)
                        continue;

                    float fill = area / (bw * bh);
                    float fillScore = 1f - Math.Min(1f, Math.Abs(fill - 0.785f) / 0.5f);

                    float circularity = (float)(4.0 * Math.PI * area / (perimeter * perimeter));
                    float circScore = Math.Max(0f, Math.Min(1f, circularity));

                    float score = circScore * 0.52f + ratioScore * 0.28f + fillScore * 0.20f;
                    score += Math.Min(0.2f, area / (float)(w * h));

                    if (best == null || score > best.Score)
                    {
                        best = new CircleCandidate
                        {
                            CenterX = (float)sumX / area,
                            CenterY = (float)sumY / area,
                            Radius = (float)Math.Sqrt(area / Math.PI),
                            Score = score,
                            Area = area
                        };
                    }
                }
            }

            return best;
        }

        private static byte[,] BuildGray(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            byte[,] gray = new byte[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    gray[x, y] = (byte)((c.R * 299 + c.G * 587 + c.B * 114) / 1000);
                }
            }
            return gray;
        }

        private static int OtsuThreshold(byte[,] gray, int w, int h)
        {
            int[] hist = new int[256];
            int total = w * h;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    hist[gray[x, y]]++;
                }
            }

            double sum = 0;
            for (int i = 0; i < 256; i++) sum += i * hist[i];

            double sumB = 0;
            int wB = 0;
            int wF;
            double maxVar = 0;
            int threshold = 127;

            for (int t = 0; t < 256; t++)
            {
                wB += hist[t];
                if (wB == 0) continue;
                wF = total - wB;
                if (wF == 0) break;

                sumB += t * hist[t];
                double mB = sumB / wB;
                double mF = (sum - sumB) / wF;
                double varBetween = wB * wF * (mB - mF) * (mB - mF);

                if (varBetween > maxVar)
                {
                    maxVar = varBetween;
                    threshold = t;
                }
            }
            return threshold;
        }

        private static ImageCodecInfo GetJpegCodec()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == "image/jpeg") return codec;
            }
            return null;
        }

    }
}

using System;
using System.Drawing;

namespace dsat.DataProcessing.Calibration
{
    /// <summary>
    /// 检测图像中最显著的直线，用于航向标定时识别已知方向参考线。
    /// 使用 Sobel 边缘检测 + 简化 Hough 变换。
    /// </summary>
    public static class ReferenceLineDetector
    {
        public struct DetectedLine
        {
            public PointF P1;
            public PointF P2;
            public int Votes;
        }

        /// <summary>
        /// 尝试检测图像中最强的直线。成功返回 true，line 包含端点。
        /// </summary>
        public static bool TryDetect(Bitmap image, out DetectedLine line)
        {
            line = default(DetectedLine);
            if (image == null) return false;

            int w = image.Width;
            int h = image.Height;

            byte[,] gray = ToGrayscale(image);
            byte[,] edges = SobelEdge(gray, w, h);
            return HoughLines(edges, w, h, out line);
        }

        private static byte[,] ToGrayscale(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            byte[,] gray = new byte[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    gray[y, x] = (byte)((c.B * 114 + c.G * 587 + c.R * 299) / 1000);
                }
            }

            return gray;
        }

        private static byte[,] SobelEdge(byte[,] gray, int w, int h)
        {
            byte[,] edges = new byte[h, w];
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int gx = -gray[y - 1, x - 1] + gray[y - 1, x + 1]
                           - 2 * gray[y, x - 1] + 2 * gray[y, x + 1]
                           - gray[y + 1, x - 1] + gray[y + 1, x + 1];

                    int gy = -gray[y - 1, x - 1] - 2 * gray[y - 1, x] - gray[y - 1, x + 1]
                           + gray[y + 1, x - 1] + 2 * gray[y + 1, x] + gray[y + 1, x + 1];

                    int mag = (int)Math.Sqrt(gx * gx + gy * gy);
                    edges[y, x] = mag > 128 ? (byte)255 : (byte)0;
                }
            }
            return edges;
        }

        private static bool HoughLines(byte[,] edges, int w, int h, out DetectedLine best)
        {
            best = default(DetectedLine);

            int diagLen = (int)Math.Sqrt(w * w + h * h) + 1;
            const int thetaSteps = 180;
            int[,] accumulator = new int[thetaSteps, 2 * diagLen];

            double[] cosTable = new double[thetaSteps];
            double[] sinTable = new double[thetaSteps];
            for (int t = 0; t < thetaSteps; t++)
            {
                double angle = t * Math.PI / thetaSteps;
                cosTable[t] = Math.Cos(angle);
                sinTable[t] = Math.Sin(angle);
            }

            // 对大图进行2x降采样加速
            int step = (w > 1000 || h > 800) ? 2 : 1;

            for (int y = 0; y < h; y += step)
            {
                for (int x = 0; x < w; x += step)
                {
                    if (edges[y, x] == 0) continue;

                    for (int t = 0; t < thetaSteps; t++)
                    {
                        int rho = (int)(x * cosTable[t] + y * sinTable[t]) + diagLen;
                        if (rho >= 0 && rho < 2 * diagLen)
                            accumulator[t, rho]++;
                    }
                }
            }

            int bestTheta = 0, bestRho = 0, bestVotes = 0;
            for (int t = 0; t < thetaSteps; t++)
            {
                for (int r = 0; r < 2 * diagLen; r++)
                {
                    if (accumulator[t, r] > bestVotes)
                    {
                        bestVotes = accumulator[t, r];
                        bestTheta = t;
                        bestRho = r;
                    }
                }
            }

            int minVotes = Math.Max(30, Math.Min(w, h) / 8);
            if (bestVotes < minVotes) return false;

            double theta = bestTheta * Math.PI / thetaSteps;
            double rhoVal = bestRho - diagLen;
            double cosT = Math.Cos(theta);
            double sinT = Math.Sin(theta);

            // 将 (rho, theta) 转为图像内两端点
            if (Math.Abs(sinT) > 0.001)
            {
                float x0 = 0, y0 = (float)((rhoVal - x0 * cosT) / sinT);
                float x1 = w - 1, y1 = (float)((rhoVal - x1 * cosT) / sinT);

                ClipToImage(ref x0, ref y0, ref x1, ref y1, w, h);

                best.P1 = new PointF(x0, y0);
                best.P2 = new PointF(x1, y1);
            }
            else
            {
                float x = (float)(rhoVal / cosT);
                best.P1 = new PointF(x, 0);
                best.P2 = new PointF(x, h - 1);
            }

            best.Votes = bestVotes;
            return true;
        }

        private static void ClipToImage(ref float x0, ref float y0, ref float x1, ref float y1, int w, int h)
        {
            if (y0 < 0) { x0 = x0 + (0 - y0) * (x1 - x0) / (y1 - y0); y0 = 0; }
            if (y0 >= h) { x0 = x0 + (h - 1 - y0) * (x1 - x0) / (y1 - y0); y0 = h - 1; }
            if (y1 < 0) { x1 = x1 + (0 - y1) * (x0 - x1) / (y0 - y1); y1 = 0; }
            if (y1 >= h) { x1 = x1 + (h - 1 - y1) * (x0 - x1) / (y0 - y1); y1 = h - 1; }
        }

        /// <summary>
        /// 在图像上绘制检测到的线段（红色，3px）。返回新 Bitmap。
        /// </summary>
        public static Bitmap DrawLine(Bitmap source, PointF p1, PointF p2)
        {
            Bitmap result = new Bitmap(source);
            using (Graphics g = Graphics.FromImage(result))
            using (Pen pen = new Pen(Color.Red, 3f))
            {
                g.DrawLine(pen, p1, p2);

                using (Brush dot = new SolidBrush(Color.Lime))
                {
                    g.FillEllipse(dot, p1.X - 5, p1.Y - 5, 10, 10);
                    g.FillEllipse(dot, p2.X - 5, p2.Y - 5, 10, 10);
                }
            }
            return result;
        }
    }
}

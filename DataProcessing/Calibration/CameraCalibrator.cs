using System;

namespace dsat.DataProcessing.Calibration
{
    public class CameraCalibrator
    {
        public CalibrationConfig CalibrateFromImages(string imageDirectory, int boardWidth, int boardHeight, double squareSize)
        {
            throw new NotImplementedException(
                "相机内参标定需要OpenCV库支持。请使用Python OpenCV标定工具生成camera_intrinsics.json，然后通过CalibrationConfig.Load()加载。");
        }

        public static CalibrationConfig CreateManualConfig(
            double fx, double fy, double cx, double cy,
            double k1, double k2, double p1, double p2,
            int imageWidth, int imageHeight)
        {
            return new CalibrationConfig
            {
                Fx = fx,
                Fy = fy,
                Cx = cx,
                Cy = cy,
                K1 = k1,
                K2 = k2,
                P1 = p1,
                P2 = p2,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight
            };
        }
    }
}


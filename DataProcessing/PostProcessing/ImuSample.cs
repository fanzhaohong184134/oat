using System;

namespace dsat.DataProcessing.PostProcessing
{
    public class ImuSample
    {
        public DateTime Timestamp { get; set; }
        public double AccX { get; set; }
        public double AccY { get; set; }
        public double AccZ { get; set; }
        public double GyroX { get; set; }
        public double GyroY { get; set; }
        public double GyroZ { get; set; }
        public double AngleX { get; set; }
        public double AngleY { get; set; }
        public double AngleZ { get; set; }
        public double MagX { get; set; }
        public double MagY { get; set; }
        public double MagZ { get; set; }
        public double Q0 { get; set; }
        public double Q1 { get; set; }
        public double Q2 { get; set; }
        public double Q3 { get; set; }
    }
}


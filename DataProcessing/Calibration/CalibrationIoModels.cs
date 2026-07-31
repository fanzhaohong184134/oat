using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Wit.Example_BWT901BLE.DataProcessing.Calibration
{
    [DataContract]
    public class CameraCalibrationInput
    {
        [DataMember] public string DeviceId { get; set; }
        [DataMember] public double Fx { get; set; }
        [DataMember] public double Fy { get; set; }
        [DataMember] public double Cx { get; set; }
        [DataMember] public double Cy { get; set; }
        [DataMember] public double K1 { get; set; }
        [DataMember] public double K2 { get; set; }
        [DataMember] public double P1 { get; set; }
        [DataMember] public double P2 { get; set; }
        [DataMember] public int ImageWidth { get; set; }
        [DataMember] public int ImageHeight { get; set; }
    }

    [DataContract]
    public class CameraCalibrationOutput
    {
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public string ConfigPath { get; set; }
        [DataMember] public double Fx { get; set; }
        [DataMember] public double Fy { get; set; }
        [DataMember] public double Cx { get; set; }
        [DataMember] public double Cy { get; set; }
        [DataMember] public double K1 { get; set; }
        [DataMember] public double K2 { get; set; }
        [DataMember] public double P1 { get; set; }
        [DataMember] public double P2 { get; set; }
        [DataMember] public int ImageWidth { get; set; }
        [DataMember] public int ImageHeight { get; set; }
    }

    [DataContract]
    public class MountingCalibrationInput
    {
        [DataMember] public string DeviceId { get; set; }
        [DataMember] public double ImuAngleX { get; set; }
        [DataMember] public double ImuAngleY { get; set; }
        [DataMember] public double UActual { get; set; }
        [DataMember] public double VActual { get; set; }
        [DataMember] public double Fx { get; set; }
        [DataMember] public double Fy { get; set; }
        [DataMember] public double Cx { get; set; }
        [DataMember] public double Cy { get; set; }
    }

    [DataContract]
    public class MountingCalibrationOutput
    {
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public string ConfigPath { get; set; }
        [DataMember] public double DeltaPitch { get; set; }
        [DataMember] public double DeltaRoll { get; set; }
    }

    [DataContract]
    public class HeadingCalibrationInput
    {
        [DataMember] public string DeviceId { get; set; }
        [DataMember] public bool HasKnownReference { get; set; }
        [DataMember] public double ImuAngleZ { get; set; }
        [DataMember] public double MagneticDeclination { get; set; }
        [DataMember] public double CurrentPsiOffset { get; set; }
        [DataMember] public double U1 { get; set; }
        [DataMember] public double V1 { get; set; }
        [DataMember] public double U2 { get; set; }
        [DataMember] public double V2 { get; set; }
        [DataMember] public double KnownAzimuth { get; set; }
        [DataMember] public double DeltaPitch { get; set; }
        [DataMember] public double DeltaRoll { get; set; }
        [DataMember] public double Fx { get; set; }
        [DataMember] public double Fy { get; set; }
        [DataMember] public double Cx { get; set; }
        [DataMember] public double Cy { get; set; }
    }

    [DataContract]
    public class HeadingCalibrationOutput
    {
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Mode { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public string ConfigPath { get; set; }
        [DataMember] public double CurrentPsiOffset { get; set; }
        [DataMember] public double NewPsiOffset { get; set; }
        [DataMember] public double PredictedAzimuth { get; set; }
        [DataMember] public double Error { get; set; }
        [DataMember] public bool Updated { get; set; }
    }

    public static class CalibrationJsonUtil
    {
        public static void SaveToFile<T>(T data, string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = File.Create(filePath))
            {
                serializer.WriteObject(stream, data);
            }
        }

        public static T LoadFromFile<T>(string filePath)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = File.OpenRead(filePath))
            {
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}

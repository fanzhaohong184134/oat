using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace dsat.DataProcessing.Calibration
{
    [DataContract]
    public class CalibrationConfig
    {
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
        [DataMember] public double PsiOffset { get; set; }
        [DataMember] public double DeltaPitch { get; set; }
        [DataMember] public double DeltaRoll { get; set; }
        [DataMember] public double MagneticDeclination { get; set; }
        [DataMember] public double HeightH { get; set; }
        [DataMember] public double GThreshold { get; set; } = 0.005;
        [DataMember] public double OmegaThreshold { get; set; } = 0.3;
        [DataMember] public int MinStableFrames { get; set; } = 3;

        public void Save(string filePath)
        {
            var serializer = new DataContractJsonSerializer(typeof(CalibrationConfig));
            using (var stream = File.Create(filePath))
            {
                serializer.WriteObject(stream, this);
            }
        }

        public static CalibrationConfig Load(string filePath)
        {
            var serializer = new DataContractJsonSerializer(typeof(CalibrationConfig));
            using (var stream = File.OpenRead(filePath))
            {
                return (CalibrationConfig)serializer.ReadObject(stream);
            }
        }
    }
}


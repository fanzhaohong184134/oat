using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace CalibrationBench.UI.Acquisition
{
    /// <summary>读取 calibration_config.json 中的相机内参(供真实采集使用)。</summary>
    public static class ConfigLoader
    {
        [DataContract]
        private sealed class ConfigMirror
        {
#pragma warning disable 0649 // 由反序列化赋值
            [DataMember] public double Fx, Fy, Cx, Cy, K1, K2, P1, P2;
            [DataMember] public int ImageWidth, ImageHeight;
#pragma warning restore 0649
        }

        /// <summary>返回 config 中的内参(扁平顶层字段)；文件不存在/无内参/解析失败时返回 null。</summary>
        public static CameraIntrinsics LoadIntrinsics(string configPath)
        {
            try
            {
                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath)) return null;
                using (var fs = File.OpenRead(configPath))
                {
                    var ser = new DataContractJsonSerializer(typeof(ConfigMirror));
                    var c = (ConfigMirror)ser.ReadObject(fs);
                    if (c == null || c.Fx <= 0) return null;
                    return new CameraIntrinsics
                    {
                        Fx = c.Fx, Fy = c.Fy, Cx = c.Cx, Cy = c.Cy,
                        K1 = c.K1, K2 = c.K2, P1 = c.P1, P2 = c.P2,
                        ImageWidth = c.ImageWidth, ImageHeight = c.ImageHeight
                    };
                }
            }
            catch { return null; }
        }
    }
}

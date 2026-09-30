using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CalibrationEngine.Models
{
    // ===================== 通用 =====================

    /// <summary>相机内参与畸变（Step 0A 产物，供 0D/现场算法使用）。</summary>
    [DataContract]
    public sealed class CameraIntrinsics
    {
        [DataMember] public double Fx;          // 焦距(像素) X
        [DataMember] public double Fy;          // 焦距(像素) Y
        [DataMember] public double Cx;          // 主点 X(像素)
        [DataMember] public double Cy;          // 主点 Y(像素)
        [DataMember] public double K1;          // 径向畸变
        [DataMember] public double K2;          // 径向畸变
        [DataMember] public double P1;          // 切向畸变
        [DataMember] public double P2;          // 切向畸变
        [DataMember] public int ImageWidth;     // 图像宽(像素)
        [DataMember] public int ImageHeight;    // 图像高(像素)
    }

    /// <summary>汇总校准配置（写入 calibration_config.json）。
    /// 结构与 dsat 后处理的 CalibrationConfig 完全一致(扁平)，保证后处理可直接解析计算。
    /// 额外字段 DeviceId/AlphaBoard/CalibratedAtUtc 仅供工装留痕，dsat 读取时忽略。</summary>
    [DataContract]
    public sealed class CalibrationConfig
    {
        // 相机内参(顶层，与 dsat 一致)
        [DataMember] public double Fx;
        [DataMember] public double Fy;
        [DataMember] public double Cx;
        [DataMember] public double Cy;
        [DataMember] public double K1;
        [DataMember] public double K2;
        [DataMember] public double P1;
        [DataMember] public double P2;
        [DataMember] public int ImageWidth;
        [DataMember] public int ImageHeight;
        // 标定量
        [DataMember] public double PsiOffset;           // Step 0C 航向偏差(°)
        [DataMember] public double DeltaPitch;          // Step 0B 安装角 pitch(°)
        [DataMember] public double DeltaRoll;           // Step 0B 安装角 roll(°)
        [DataMember] public double MagneticDeclination; // 磁偏角 D(°)
        [DataMember] public double HeightH;             // 相机到地面高度 H(mm)，现场测量后填入
        // 稳态阈值(与 dsat 默认一致)
        [DataMember] public double GThreshold = 0.005;
        [DataMember] public double OmegaThreshold = 0.3;
        [DataMember] public int MinStableFrames = 3;
        // 工装留痕(dsat 忽略)
        [DataMember] public string DeviceId;
        [DataMember] public double AlphaBoard;          // 靶标板 X 轴真方位(°)
        [DataMember] public string CalibratedAtUtc;
    }

    // ===================== Step 0A 相机内参 =====================

    /// <summary>Step 0A 输入：外部内参标定结果（或人工确认值）。</summary>
    [DataContract]
    public sealed class Step0AInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public CameraIntrinsics Intrinsics;
        [DataMember] public double ReprojRms = -1;      // 重投影 RMS(px)，<0 表示未提供
        [DataMember] public double RmsLimitPx = 0.5;    // 合格阈值
    }

    [DataContract]
    public sealed class Step0AOutput
    {
        [DataMember] public bool Passed;
        [DataMember] public string Message;
        [DataMember] public CameraIntrinsics Intrinsics;
        [DataMember] public double ReprojRms;
    }

    // ===================== Step 0B 安装角 =====================

    /// <summary>安装角单帧样本。优先用 PnP 给出的相机光轴倾角；</summary>
    [DataContract]
    public sealed class MountingSample
    {
        [DataMember] public double PitchCamDeg;   // 相机光轴 pitch(由 PnP, 相对世界铅垂)
        [DataMember] public double RollCamDeg;    // 相机光轴 roll
        [DataMember] public double ImuAngleYDeg;  // IMU pitch = AngleY
        [DataMember] public double ImuAngleXDeg;  // IMU roll  = AngleX
    }

    [DataContract]
    public sealed class Step0BInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public List<MountingSample> Samples;
        [DataMember] public double ToleranceDeg = 0.05; // 各次偏差合格阈值
    }

    [DataContract]
    public sealed class Step0BOutput
    {
        [DataMember] public bool Passed;
        [DataMember] public string Message;
        [DataMember] public double DeltaPitch;      // 稳健均值(°)
        [DataMember] public double DeltaRoll;
        [DataMember] public double MaxDeviationDeg; // 各次相对均值最大偏差
        [DataMember] public int SampleCount;
    }

    // ===================== Step 0C 航向 =====================

    /// <summary>航向单方位样本（该方位多帧已平均）。</summary>
    [DataContract]
    public sealed class HeadingSample
    {
        [DataMember] public double NominalAzimuthDeg; // 名义方位(仅记录)
        [DataMember] public double ThetaImgBoardDeg;  // 图像中板 X_B 相对相机 X 轴方位
        [DataMember] public double ImuAngleZDeg;      // IMU 航向 AngleZ(磁北)
    }

    [DataContract]
    public sealed class Step0CInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public double AlphaBoardDeg;         // 板真方位
        [DataMember] public double MagneticDeclinationDeg;// D
        [DataMember] public List<HeadingSample> Samples;  // ≥8 方位
        [DataMember] public double ToleranceDeg = 0.1;    // 残差合格阈值
    }

    [DataContract]
    public sealed class HeadingPerAzimuth
    {
        [DataMember] public double NominalAzimuthDeg;
        [DataMember] public double PsiOffsetDeg;   // 该方位反算 ψ_offset
        [DataMember] public double ResidualDeg;    // 相对总均值残差
    }

    [DataContract]
    public sealed class Step0COutput
    {
        [DataMember] public bool Passed;
        [DataMember] public string Message;
        [DataMember] public double PsiOffsetDeg;   // 圆均值
        [DataMember] public double ResidualDeg;    // 最大残差
        [DataMember] public List<HeadingPerAzimuth> PerAzimuth;
    }

    // ===================== Step 0D 综合验证 =====================

    /// <summary>验证单帧：含 PnP 真值与简化算法所需量。</summary>
    [DataContract]
    public sealed class VerifyFrame
    {
        [DataMember] public double NominalAzimuthDeg;
        [DataMember] public double UCorrPx;      // 畸变校正后靶心像素 u
        [DataMember] public double VCorrPx;      // 畸变校正后靶心像素 v
        [DataMember] public double H_mm;         // 相机到板面高度
        [DataMember] public double ImuAngleXDeg; // roll
        [DataMember] public double ImuAngleYDeg; // pitch
        [DataMember] public double ImuAngleZDeg; // yaw(磁北)
        [DataMember] public double OffsetPnpE_mm;// PnP 真值 E
        [DataMember] public double OffsetPnpN_mm;// PnP 真值 N
    }

    [DataContract]
    public sealed class Step0DInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public CameraIntrinsics Intrinsics;
        [DataMember] public double DeltaPitch;
        [DataMember] public double DeltaRoll;
        [DataMember] public double PsiOffsetDeg;
        [DataMember] public double MagneticDeclinationDeg;
        [DataMember] public double AlphaBoardDeg;
        [DataMember] public List<VerifyFrame> Frames;
        [DataMember] public double SigmaRotLimit_mm = 0.3;
        [DataMember] public double ClosureLimit_mm = 0.5;
        [DataMember] public double ClosureMeanLimit_mm = 0.3;
    }

    [DataContract]
    public sealed class VerifyPerAzimuth
    {
        [DataMember] public double NominalAzimuthDeg;
        [DataMember] public int FrameCount;
        [DataMember] public double OffsetSysE_mm;
        [DataMember] public double OffsetSysN_mm;
        [DataMember] public double OffsetPnpE_mm;
        [DataMember] public double OffsetPnpN_mm;
        [DataMember] public double Closure_mm;         // |sys - pnp|
        [DataMember] public double MagHeadingErrDeg;   // 磁航向误差
    }

    [DataContract]
    public sealed class Step0DOutput
    {
        [DataMember] public bool Passed;
        [DataMember] public string Message;
        [DataMember] public double SigmaRot_mm;    // 旋转一致性散布
        [DataMember] public double ClosureMax_mm;  // 各方位最大闭合偏差
        [DataMember] public double ClosureMean_mm; // 均值
        [DataMember] public double PredictedErr_mm;// 计入 δψ 余量的预测
        [DataMember] public List<VerifyPerAzimuth> PerAzimuth;
    }
}

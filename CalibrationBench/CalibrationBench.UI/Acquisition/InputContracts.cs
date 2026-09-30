using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CalibrationBench.UI.Acquisition
{
    // 引擎输入 JSON 的界面侧镜像契约(字段名与 CalibrationEngine 输入一致)。
    // 通过进程 + JSON 边界与引擎解耦。

    [DataContract]
    public sealed class CameraIntrinsics
    {
        [DataMember] public double Fx, Fy, Cx, Cy, K1, K2, P1, P2;
        [DataMember] public int ImageWidth, ImageHeight;
    }

    [DataContract]
    public sealed class MountingSample
    {
        [DataMember] public double PitchCamDeg, RollCamDeg, ImuAngleYDeg, ImuAngleXDeg;
    }

    [DataContract]
    public sealed class Step0BInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public List<MountingSample> Samples;
        [DataMember] public double ToleranceDeg;
    }

    [DataContract]
    public sealed class HeadingSample
    {
        [DataMember] public double NominalAzimuthDeg, ThetaImgBoardDeg, ImuAngleZDeg;
    }

    [DataContract]
    public sealed class Step0CInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public double AlphaBoardDeg, MagneticDeclinationDeg;
        [DataMember] public List<HeadingSample> Samples;
        [DataMember] public double ToleranceDeg;
    }

    [DataContract]
    public sealed class VerifyFrame
    {
        [DataMember] public double NominalAzimuthDeg, UCorrPx, VCorrPx, H_mm;
        [DataMember] public double ImuAngleXDeg, ImuAngleYDeg, ImuAngleZDeg;
        [DataMember] public double OffsetPnpE_mm, OffsetPnpN_mm;
    }

    [DataContract]
    public sealed class Step0DInput
    {
        [DataMember] public string DeviceId;
        [DataMember] public CameraIntrinsics Intrinsics;
        [DataMember] public double DeltaPitch, DeltaRoll, PsiOffsetDeg, MagneticDeclinationDeg, AlphaBoardDeg;
        [DataMember] public List<VerifyFrame> Frames;
        [DataMember] public double SigmaRotLimit_mm, ClosureLimit_mm, ClosureMeanLimit_mm;
    }
}

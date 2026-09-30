# 生成 0C/0D 样例输入(成员按字母序，保证 DataContractJsonSerializer 兼容)
$ErrorActionPreference = "Stop"
$dir = "e:\git\test\oat\CalibrationBench\samples"
$d2r = [math]::PI/180.0

# ---- 0C ----
$alpha = 30.0; $D = -6.5; $psiTrue = 12.34
$noise0C = @(0.00,0.02,-0.03,0.01,-0.02,0.03,-0.01,0.02)
$sb = New-Object System.Text.StringBuilder
[void]$sb.Append('{"AlphaBoardDeg":30.0,"DeviceId":"AT1","MagneticDeclinationDeg":-6.5,"Samples":[')
$phis = 0,45,90,135,180,225,270,315
for($i=0;$i -lt $phis.Count;$i++){
  $phi=$phis[$i]
  $angleZ=$phi
  $theta = $alpha - ($angleZ + $D) - $psiTrue + $noise0C[$i]
  if($i -gt 0){[void]$sb.Append(',')}
  [void]$sb.Append('{"ImuAngleZDeg":'+$angleZ+',"NominalAzimuthDeg":'+$phi+',"ThetaImgBoardDeg":'+([math]::Round($theta,4))+'}')
}
[void]$sb.Append('],"ToleranceDeg":0.1}')
[System.IO.File]::WriteAllText("$dir\0C_input.json",$sb.ToString(),(New-Object System.Text.UTF8Encoding($false)))

# ---- 0D ----
$fx=2200.0;$fy=2200.0;$cx=1224.0;$cy=1024.0;$Hmm=400.0
$E=10.0;$N=5.0
$noiseU = @(0.00,0.10,-0.08,0.05,-0.06,0.09,-0.04,0.07)  # 像素级小噪声
$sb2 = New-Object System.Text.StringBuilder
[void]$sb2.Append('{"AlphaBoardDeg":30.0,"ClosureLimit_mm":0.5,"ClosureMeanLimit_mm":0.3,"DeltaPitch":0.0,"DeltaRoll":0.0,"DeviceId":"AT1","Frames":[')
$first=$true
for($i=0;$i -lt $phis.Count;$i++){
  $phi=$phis[$i]
  $angleZ=$phi
  $h = ($angleZ + $D + $psiTrue)*$d2r
  $dx = [math]::Cos($h)*$E - [math]::Sin($h)*$N
  $dy = [math]::Sin($h)*$E + [math]::Cos($h)*$N
  $u = $cx + $dx*$fx/$Hmm + $noiseU[$i]
  $v = $cy + $dy*$fy/$Hmm
  if(-not $first){[void]$sb2.Append(',')}; $first=$false
  [void]$sb2.Append('{"H_mm":400.0,"ImuAngleXDeg":0.0,"ImuAngleYDeg":0.0,"ImuAngleZDeg":'+$angleZ+',"NominalAzimuthDeg":'+$phi+',"OffsetPnpE_mm":10.0,"OffsetPnpN_mm":5.0,"UCorrPx":'+([math]::Round($u,4))+',"VCorrPx":'+([math]::Round($v,4))+'}')
}
[void]$sb2.Append('],"Intrinsics":{"Cx":1224.0,"Cy":1024.0,"Fx":2200.0,"Fy":2200.0,"ImageHeight":2048,"ImageWidth":2448,"K1":0.0,"K2":0.0,"P1":0.0,"P2":0.0},"MagneticDeclinationDeg":-6.5,"PsiOffsetDeg":12.34,"SigmaRotLimit_mm":0.3}')
[System.IO.File]::WriteAllText("$dir\0D_input.json",$sb2.ToString(),(New-Object System.Text.UTF8Encoding($false)))

Write-Output "0C/0D 样例已生成"

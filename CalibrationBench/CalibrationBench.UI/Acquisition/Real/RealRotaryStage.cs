using System;
using System.IO.Ports;
using System.Threading;

namespace CalibrationBench.UI.Acquisition.Real
{
    /// <summary>
    /// 串口旋转台(可用实现)。协议见方案文档 §4.6：
    ///   MOVE,045.0\r\n → OK ；LOCK\r\n → OK ；STAT?\r\n → SETTLED,1 ；ANG?\r\n → ANG,045.02
    /// 若你的转台协议不同，改这里的命令帧即可。
    /// </summary>
    public sealed class RealRotaryStage : IRotaryStage, IDisposable
    {
        private readonly SerialPort _port;
        private double _az;

        public RealRotaryStage(string portName, int baud = 115200)
        {
            _port = new SerialPort(portName, baud) { NewLine = "\r\n", ReadTimeout = 3000, WriteTimeout = 3000 };
            _port.Open();
        }

        public double CurrentAzimuthDeg { get { return _az; } }

        public void MoveTo(double azimuthDeg)
        {
            _az = azimuthDeg;
            SendExpectOk(string.Format("MOVE,{0:F1}", azimuthDeg));
            // 等待到位
            int waited = 0;
            while (!IsSettled() && waited < 20000) { Thread.Sleep(100); waited += 100; }
        }

        public void Lock() { SendExpectOk("LOCK"); }
        public void Unlock() { SendExpectOk("UNLOCK"); }

        public bool IsSettled()
        {
            string r = Send("STAT?");
            return r != null && r.Trim().EndsWith("1");
        }

        private void SendExpectOk(string cmd)
        {
            string r = Send(cmd);
            if (r == null || !r.StartsWith("OK"))
                throw new InvalidOperationException("旋转台命令失败: " + cmd + " → " + (r ?? "无响应"));
        }

        private string Send(string cmd)
        {
            _port.WriteLine(cmd);
            try { return _port.ReadLine(); } catch (TimeoutException) { return null; }
        }

        public void Dispose() { if (_port != null && _port.IsOpen) _port.Close(); }
    }
}

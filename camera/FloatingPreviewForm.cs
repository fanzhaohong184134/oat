using System;
using System.Drawing;
using System.Windows.Forms;

namespace Wit.Example_BWT901BLE.Camera
{
    /// <summary>
    /// 浮动窗口 - 显示相机实时预览画面
    /// </summary>
    public class FloatingPreviewForm : Form
    {
        private PictureBox _pictureBox;
        private Label _statusLabel;
        private Image _currentImage;

        public FloatingPreviewForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "相机预览 Camera Preview";
            this.Size = new Size(640, 520);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new Size(320, 280);

            // 图片显示
            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.None
            };

            this.Controls.Add(_pictureBox);
            this.Controls.Add(_statusLabel);

            this.ResumeLayout(false);
        }

        /// <summary>
        /// 更新预览图像（线程安全）
        /// </summary>
        public void UpdateImage(Image image)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new Action(() => UpdateImage(image)));
                }
                catch { }
                return;
            }

            try
            {
                // 释放旧图像
                if (_currentImage != null)
                {
                    _currentImage.Dispose();
                }

                _currentImage = new Bitmap(image);
                _pictureBox.Image = _currentImage;
            }
            catch { }
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        public void UpdateStatus(string status)
        {
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 点击关闭时隐藏而不是关闭
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_currentImage != null)
                {
                    _currentImage.Dispose();
                    _currentImage = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace dsat.CalibrationPanels
{
    public class CalibrationPanelBaseForm : Form
    {
        private static readonly Color ThemePanelBackground = Color.FromArgb(248, 250, 247);
        private static readonly Color ThemeAppBackground = Color.FromArgb(233, 238, 236);
        private static readonly Color ThemeTitle = Color.FromArgb(30, 56, 77);
        private static readonly Color ThemeText = Color.FromArgb(35, 52, 64);
        private static readonly Color ThemeButton = Color.FromArgb(48, 94, 127);
        private static readonly Color ThemeAccentButton = Color.FromArgb(190, 96, 28);
        private static readonly Color ThemeBorder = Color.FromArgb(102, 124, 143);
        private static readonly Color ThemeInputBack = Color.FromArgb(254, 255, 252);

        protected readonly TableLayoutPanel MainLayout;
        protected readonly TableLayoutPanel InputGrid;
        protected readonly TextBox ResultTextBox;
        protected readonly Label StatusLabel;
        protected readonly Button RunButton;
        protected readonly Button ConfirmButton;
        protected readonly Button ClosePanelButton;

        public CalibrationPanelBaseForm(string title, string intro)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(640, 560);
            Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = ThemeAppBackground;
            ForeColor = ThemeText;

            MainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10),
                BackColor = ThemeAppBackground
            };
            MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            MainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            MainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(MainLayout);

            var introLabel = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = ThemeTitle,
                Text = intro
            };
            MainLayout.Controls.Add(introLabel, 0, 0);

            var inputGroup = new GroupBox
            {
                Text = "输入参数",
                Dock = DockStyle.Fill,
                BackColor = ThemePanelBackground,
                ForeColor = ThemeTitle,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
            };
            MainLayout.Controls.Add(inputGroup, 0, 1);

            InputGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoScroll = true,
                Padding = new Padding(8),
                BackColor = ThemePanelBackground
            };
            InputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            InputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            inputGroup.Controls.Add(InputGrid);

            var resultGroup = new GroupBox
            {
                Text = "结果与说明",
                Dock = DockStyle.Fill,
                BackColor = ThemePanelBackground,
                ForeColor = ThemeTitle,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
            };
            MainLayout.Controls.Add(resultGroup, 0, 2);

            ResultTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ThemeInputBack,
                ForeColor = ThemeText,
                Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point)
            };
            resultGroup.Controls.Add(ResultTextBox);

            StatusLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.DimGray,
                AutoSize = true,
                Text = "等待执行..."
            };
            MainLayout.Controls.Add(StatusLabel, 0, 3);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };
            MainLayout.Controls.Add(buttonPanel, 0, 4);

            ConfirmButton = new Button { Text = "确认并关闭", Enabled = false, Width = 110 };
            ConfirmButton.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            StyleActionButton(ConfirmButton, true);
            buttonPanel.Controls.Add(ConfirmButton);

            RunButton = new Button { Text = "执行标定", Width = 100 };
            StyleActionButton(RunButton, false);
            buttonPanel.Controls.Add(RunButton);

            ClosePanelButton = new Button { Text = "取消", Width = 80 };
            ClosePanelButton.Click += (s, e) => Close();
            StyleActionButton(ClosePanelButton, false);
            buttonPanel.Controls.Add(ClosePanelButton);
        }

        protected TextBox AddInputRow(string label, string defaultValue = "")
        {
            int row = InputGrid.RowCount++;
            InputGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var labelCtrl = new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 3, 3)
            };
            var textBox = new TextBox
            {
                Text = defaultValue,
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ThemeInputBack,
                ForeColor = ThemeText,
                Font = Font
            };

            InputGrid.Controls.Add(labelCtrl, 0, row);
            InputGrid.Controls.Add(textBox, 1, row);
            return textBox;
        }

        protected Label AddInfoRow(string label, string value = "")
        {
            int row = InputGrid.RowCount++;
            InputGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var labelCtrl = new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 3, 3)
            };

            var valueLabel = new Label
            {
                Text = value,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 3, 3)
            };

            InputGrid.Controls.Add(labelCtrl, 0, row);
            InputGrid.Controls.Add(valueLabel, 1, row);
            return valueLabel;
        }

        protected CheckBox AddCheckRow(string label, bool defaultChecked)
        {
            int row = InputGrid.RowCount++;
            InputGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var check = new CheckBox
            {
                Text = label,
                Checked = defaultChecked,
                AutoSize = true,
                ForeColor = ThemeText,
                Dock = DockStyle.Top
            };

            InputGrid.Controls.Add(check, 1, row);
            return check;
        }

        protected void SetStatus(string text, bool success)
        {
            StatusLabel.Text = text;
            StatusLabel.ForeColor = success ? Color.DarkGreen : Color.DarkRed;
        }

        private void StyleActionButton(Button button, bool accent)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = ThemeBorder;
            button.BackColor = accent ? ThemeAccentButton : ThemeButton;
            button.ForeColor = Color.White;
            button.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            button.UseVisualStyleBackColor = false;
        }
    }
}


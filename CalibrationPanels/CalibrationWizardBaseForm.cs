using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using dsat.DataProcessing.Calibration;

namespace dsat.CalibrationPanels
{
    public class WizardStep
    {
        public string Title { get; set; }
        public string Instruction { get; set; }
        public Panel Content { get; set; }
        public Action OnEnter { get; set; }
        public Func<bool> OnValidate { get; set; }
    }

    public class CalibrationWizardBaseForm : Form
    {
        protected static readonly Color ThemePanelBackground = Color.FromArgb(248, 250, 247);
        protected static readonly Color ThemeAppBackground = Color.FromArgb(233, 238, 236);
        protected static readonly Color ThemeTitle = Color.FromArgb(30, 56, 77);
        protected static readonly Color ThemeText = Color.FromArgb(35, 52, 64);
        protected static readonly Color ThemeButton = Color.FromArgb(48, 94, 127);
        protected static readonly Color ThemeAccentButton = Color.FromArgb(190, 96, 28);
        protected static readonly Color ThemeBorder = Color.FromArgb(102, 124, 143);
        protected static readonly Color ThemeInputBack = Color.FromArgb(254, 255, 252);
        protected static readonly Color ThemeSuccess = Color.FromArgb(53, 122, 58);

        private readonly List<WizardStep> _steps = new List<WizardStep>();
        private int _currentIndex = -1;
        private readonly Label _stepIndicator;
        private readonly Label _instructionLabel;
        private readonly Panel _contentArea;
        private readonly Label _statusLabel;
        private readonly Button _prevButton;
        private readonly Button _nextButton;
        private readonly Button _cancelButton;

        protected int StepCount => _steps.Count;
        protected int CurrentStepIndex => _currentIndex;
        protected Button NextButton => _nextButton;
        protected Button PrevButton => _prevButton;

        public CalibrationWizardBaseForm(string title)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(720, 640);
            MinimumSize = new Size(620, 520);
            Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = ThemeAppBackground;
            ForeColor = ThemeText;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12),
                BackColor = ThemeAppBackground
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(mainLayout);

            _stepIndicator = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = ThemeTitle,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainLayout.Controls.Add(_stepIndicator, 0, 0);

            _instructionLabel = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ForeColor = ThemeText,
                Padding = new Padding(0, 2, 0, 6),
                MaximumSize = new Size(680, 0)
            };
            mainLayout.Controls.Add(_instructionLabel, 0, 1);

            _contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemePanelBackground,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            mainLayout.Controls.Add(_contentArea, 0, 2);

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };
            mainLayout.Controls.Add(_statusLabel, 0, 3);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            mainLayout.Controls.Add(buttonPanel, 0, 4);

            _cancelButton = new Button { Text = "取消", Width = 80, Height = 32 };
            _cancelButton.Click += (s, e) => Close();
            StyleButton(_cancelButton, false);
            buttonPanel.Controls.Add(_cancelButton);

            _nextButton = new Button { Text = "下一步 >>", Width = 110, Height = 32 };
            _nextButton.Click += (s, e) => GoNext();
            StyleButton(_nextButton, true);
            buttonPanel.Controls.Add(_nextButton);

            _prevButton = new Button { Text = "<< 上一步", Width = 110, Height = 32, Enabled = false };
            _prevButton.Click += (s, e) => GoPrevious();
            StyleButton(_prevButton, false);
            buttonPanel.Controls.Add(_prevButton);
        }

        protected void AddStep(WizardStep step)
        {
            _steps.Add(step);
            if (step.Content != null)
            {
                step.Content.Dock = DockStyle.Fill;
                step.Content.Visible = false;
                _contentArea.Controls.Add(step.Content);
            }
        }

        protected void StartWizard()
        {
            if (_steps.Count > 0)
                GoToStep(0);
        }

        protected void GoToStep(int index)
        {
            if (index < 0 || index >= _steps.Count) return;

            if (_currentIndex >= 0 && _currentIndex < _steps.Count)
            {
                var old = _steps[_currentIndex];
                if (old.Content != null) old.Content.Visible = false;
            }

            _currentIndex = index;
            var step = _steps[_currentIndex];

            _stepIndicator.Text = string.Format("步骤 {0}/{1}:  {2}", _currentIndex + 1, _steps.Count, step.Title);
            _instructionLabel.Text = step.Instruction ?? "";

            if (step.Content != null) step.Content.Visible = true;

            _prevButton.Enabled = _currentIndex > 0;
            _nextButton.Text = _currentIndex == _steps.Count - 1 ? "完成" : "下一步 >>";

            step.OnEnter?.Invoke();
        }

        private void GoNext()
        {
            if (_currentIndex < 0 || _currentIndex >= _steps.Count) return;

            var current = _steps[_currentIndex];
            if (current.OnValidate != null && !current.OnValidate())
                return;

            if (_currentIndex == _steps.Count - 1)
            {
                OnWizardFinish();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                GoToStep(_currentIndex + 1);
            }
        }

        private void GoPrevious()
        {
            if (_currentIndex > 0)
                GoToStep(_currentIndex - 1);
        }

        protected virtual void OnWizardFinish() { }

        protected void SetStatus(string text, bool success)
        {
            _statusLabel.Text = text;
            _statusLabel.ForeColor = success ? Color.DarkGreen : Color.DarkRed;
        }

        protected void SetStatusNeutral(string text)
        {
            _statusLabel.Text = text;
            _statusLabel.ForeColor = Color.DimGray;
        }

        protected static void StyleButton(Button button, bool accent)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(102, 124, 143);
            button.BackColor = accent ? Color.FromArgb(190, 96, 28) : Color.FromArgb(48, 94, 127);
            button.ForeColor = Color.White;
            button.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            button.UseVisualStyleBackColor = false;
        }

        protected static CalibrationConfig LoadConfigSafe(string configPath)
        {
            try
            {
                if (File.Exists(configPath))
                    return CalibrationConfig.Load(configPath);
            }
            catch { }
            return new CalibrationConfig();
        }

        protected Label CreateInfoLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = ThemeText,
                Margin = new Padding(6, 4, 6, 2)
            };
        }

        protected TextBox CreateReadOnlyBox(string text, int width = 200)
        {
            return new TextBox
            {
                Text = text,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 238),
                ForeColor = ThemeText,
                Font = Font,
                Width = width
            };
        }

        protected TextBox CreateEditBox(string defaultValue = "", int width = 200)
        {
            return new TextBox
            {
                Text = defaultValue,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ThemeInputBack,
                ForeColor = ThemeText,
                Font = Font,
                Width = width
            };
        }

        protected TextBox CreateResultBox()
        {
            return new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ThemeInputBack,
                ForeColor = ThemeText,
                Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point),
                Dock = DockStyle.Fill
            };
        }

        protected static Panel BuildKvPanel(params Tuple<string, Control>[] rows)
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(8)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            foreach (var row in rows)
            {
                int r = grid.RowCount++;
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var lbl = new Label
                {
                    Text = row.Item1,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(3, 6, 3, 3)
                };
                grid.Controls.Add(lbl, 0, r);
                row.Item2.Anchor = AnchorStyles.Left;
                row.Item2.Margin = new Padding(0, 4, 0, 2);
                grid.Controls.Add(row.Item2, 1, r);
            }

            var panel = new Panel { AutoScroll = true };
            panel.Controls.Add(grid);
            return panel;
        }
    }
}

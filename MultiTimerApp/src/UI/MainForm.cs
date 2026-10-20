using System;
using System.Drawing;
using System.Windows.Forms;
using MultiTimerApp;

namespace MultiTimerApp.UI
{
    public partial class MainForm : Form
    {
        private Panel _mainPanel;
        private StopwatchTab _stopwatchTab;
        private ReminderTab _reminderTab;
        private StreakTab _streakTab;

        private Button _btnStopwatches;
        private Button _btnReminders;
        private Button _btnStreaks;
        private TimerManager _timerManager;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "MultiTimer App";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // Left navigation
            var navPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = Color.LightGray
            };

            _btnStopwatches = new Button { Text = "Stopwatches", Dock = DockStyle.Top, Height = 50 };
            _btnReminders = new Button { Text = "Reminders", Dock = DockStyle.Top, Height = 50 };
            _btnStreaks = new Button { Text = "Streaks", Dock = DockStyle.Top, Height = 50 };

            _btnStopwatches.Click += (s, e) => ShowTab(_stopwatchTab);
            _btnReminders.Click += (s, e) => ShowTab(_reminderTab);
            _btnStreaks.Click += (s, e) => ShowTab(_streakTab);

            navPanel.Controls.AddRange(new Control[] { _btnStreaks, _btnReminders, _btnStopwatches });

            // Main area
            _mainPanel = new Panel { Dock = DockStyle.Fill };
            // Tab data
            _timerManager = new TimerManager();
            try
            {
                _timerManager.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading timers: {ex.Message}\n\n{ex.StackTrace}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            // Tabs
            _stopwatchTab = new StopwatchTab(_timerManager);
            _reminderTab = new ReminderTab(_timerManager);
            _streakTab = new StreakTab(_timerManager);

            _mainPanel.Controls.AddRange(new Control[] { _stopwatchTab, _reminderTab, _streakTab });

            Controls.Add(_mainPanel);
            Controls.Add(navPanel);

            ShowTab(_stopwatchTab);

            // Save on close
            this.FormClosing += MainForm_FormClosing;
        }
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                _timerManager.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving timers: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowTab(UserControl tab)
        {
            foreach (Control c in _mainPanel.Controls)
                c.Visible = false;
            tab.Dock = DockStyle.Fill;
            tab.Visible = true;
            tab.BringToFront();
        }
    }
}

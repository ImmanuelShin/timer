using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MultiTimerApp.Timers;

namespace MultiTimerApp.UI
{
    public class StreakTab : UserControl
    {
        private ListView _streakList;
        private ListBox _historyList;
        private Label _currentLabel, _longestLabel;
        private TextBox _nameInput;
        private Button _btnStart, _btnBreak, _btnRestart;

        private TimerManager _manager;
        private readonly List<StreakTracker> _streaks = new();
        private StreakTracker? _selected;
        private System.Windows.Forms.Timer _uiTimer;

        public StreakTab(TimerManager manager)
        {
            _manager = manager;
            InitializeUI();
            LoadStreaks();
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            // Sidebar - streaks list
            var sidebar = new Panel { Dock = DockStyle.Right, Width = 250, BackColor = Color.WhiteSmoke };
            _streakList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            _streakList.Columns.Add("Name", 120);
            _streakList.Columns.Add("Current", 100);
            _streakList.SelectedIndexChanged += OnStreakSelected;
            sidebar.Controls.Add(_streakList);

            // Main area
            _currentLabel = new Label
            {
                Text = "Current: 00:00:00",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 20, FontStyle.Bold)
            };

            _longestLabel = new Label
            {
                Text = "Longest: 00:00:00",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 14)
            };

            _nameInput = new TextBox
            {
                Dock = DockStyle.Top,
                PlaceholderText = "Streak name..."
            };

            _btnStart = new Button { Text = "Start New Streak", Dock = DockStyle.Top, Height = 40 };
            _btnBreak = new Button { Text = "Break Streak", Dock = DockStyle.Top, Height = 40 };
            _btnRestart = new Button { Text = "Restart Streak", Dock = DockStyle.Top, Height = 40 };

            _btnStart.Click += StartStreak_Click;
            _btnBreak.Click += BreakStreak_Click;
            _btnRestart.Click += RestartStreak_Click;

            // History box
            _historyList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 10)
            };

            Controls.Add(_historyList);
            Controls.Add(_btnRestart);
            Controls.Add(_btnBreak);
            Controls.Add(_btnStart);
            Controls.Add(_nameInput);
            Controls.Add(_longestLabel);
            Controls.Add(_currentLabel);
            Controls.Add(sidebar);

            _uiTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
        }

        private void StartStreak_Click(object? sender, EventArgs e)
        {
            string name = string.IsNullOrWhiteSpace(_nameInput.Text) ? "Streak" : _nameInput.Text;
            var streak = new StreakTracker { Name = name };
            streak.Start();

            _streaks.Add(streak);
            _manager.AddTimer(streak);
            _manager.Save();
            RefreshStreakList();
        }

        private void BreakStreak_Click(object? sender, EventArgs e)
        {
            _selected?.BreakStreak();
            RefreshStreakList();
            RefreshHistory();
        }

        private void RestartStreak_Click(object? sender, EventArgs e)
        {
            _selected?.Restart();
            RefreshStreakList();
            RefreshHistory();
        }

        private void OnStreakSelected(object? sender, EventArgs e)
        {
            if (_streakList.SelectedItems.Count == 0)
                return;

            var selectedName = _streakList.SelectedItems[0].Text;
            _selected = _streaks.FirstOrDefault(s => s.Name == selectedName);

            RefreshLabels();
            RefreshHistory();
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            if (_selected != null)
            {
                _currentLabel.Text = $"Current: {_selected.GetElapsed():hh\\:mm\\:ss}";
                _longestLabel.Text = $"Longest: {_selected.GetLongestStreak():hh\\:mm\\:ss}";
            }

            UpdateListView();
        }

        private void UpdateListView()
        {
            foreach (ListViewItem item in _streakList.Items)
            {
                var streak = _streaks.FirstOrDefault(s => s.Name == item.Text);
                if (streak == null) continue;

                string elapsed = streak.GetElapsed().ToString(@"hh\:mm\:ss");
                if (item.SubItems[1].Text != elapsed)
                    item.SubItems[1].Text = elapsed;
            }
        }

        private void RefreshStreakList()
        {
            _streakList.BeginUpdate();
            _streakList.Items.Clear();

            foreach (var s in _streaks)
            {
                var item = new ListViewItem(new[]
                {
                    s.Name,
                    s.GetElapsed().ToString(@"hh\:mm\:ss")
                });
                _streakList.Items.Add(item);
            }

            _streakList.EndUpdate();
        }

        private void RefreshLabels()
        {
            if (_selected == null) return;

            _currentLabel.Text = $"Current: {_selected.GetElapsed():hh\\:mm\\:ss}";
            _longestLabel.Text = $"Longest: {_selected.GetLongestStreak():hh\\:mm\\:ss}";
        }

        private void RefreshHistory()
        {
            _historyList.Items.Clear();
            if (_selected == null) return;

            foreach (var date in _selected.BreakHistory.OrderByDescending(d => d))
            {
                _historyList.Items.Add($"Broke on {date:G}");
            }
        }
        private void LoadStreaks()
        {
            _streaks.Clear();
            _streaks.AddRange(_manager.Timers.OfType<StreakTracker>());
            RefreshStreakList();
        }

    }
}

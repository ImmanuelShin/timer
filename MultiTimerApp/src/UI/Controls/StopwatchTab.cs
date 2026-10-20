using System;
using System.Drawing;
using System.Windows.Forms;
using MultiTimerApp.Timers;

namespace MultiTimerApp.UI
{
    public class StopwatchTab : UserControl
    {
        private TimerManager _manager;
        private ListView _stopwatchList;
        private Label _timeLabel;
        private Button _btnStart, _btnStop, _btnReset;

        private ClassicStopwatch? _current;
        private System.Windows.Forms.Timer _uiTimer;

        public StopwatchTab(TimerManager manager)
        {
            _manager = manager;
            InitializeUI();
            LoadStopwatches();
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            // Sidebar
            var sidebar = new Panel { Dock = DockStyle.Right, Width = 250, BackColor = Color.WhiteSmoke };
            _stopwatchList = new ListView { Dock = DockStyle.Fill, View = View.Details };
            _stopwatchList.Columns.Add("Name", 120);
            _stopwatchList.Columns.Add("Elapsed", 100);
            sidebar.Controls.Add(_stopwatchList);

            // Main display
            _timeLabel = new Label
            {
                Text = "00:00:00.000",
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 28, FontStyle.Bold)
            };

            _btnStart = new Button { Text = "Start", Dock = DockStyle.Top, Height = 40 };
            _btnStop = new Button { Text = "Stop", Dock = DockStyle.Top, Height = 40 };
            _btnReset = new Button { Text = "Reset", Dock = DockStyle.Top, Height = 40 };

            _btnStart.Click += (s, e) => { _current ??= new ClassicStopwatch { Name = "Stopwatch1" }; _current.Start(); };
            _btnStop.Click += (s, e) => _current?.Stop();
            _btnReset.Click += (s, e) => { _current?.Reset(); _timeLabel.Text = "00:00:00.000"; };

            Controls.Add(_btnReset);
            Controls.Add(_btnStop);
            Controls.Add(_btnStart);
            Controls.Add(_timeLabel);
            Controls.Add(sidebar);

            // Timer to refresh display
            _uiTimer = new System.Windows.Forms.Timer { Interval = 200 };
            _uiTimer.Tick += (s, e) =>
            {
                if (_current != null)
                {
                    _timeLabel.Text = _current.GetElapsed().ToString(@"hh\:mm\:ss\.fff");
                }
            };
            _uiTimer.Start();
        }
        private void LoadStopwatches()
        {
            _stopwatchList.Items.Clear();
            foreach (var timer in _manager.Timers.OfType<ClassicStopwatch>())
            {
                var item = new ListViewItem(new[] { timer.Name, timer.GetElapsed().ToString(@"hh\:mm\:ss\.fff") });
                _stopwatchList.Items.Add(item);
            }
        }
    }
}

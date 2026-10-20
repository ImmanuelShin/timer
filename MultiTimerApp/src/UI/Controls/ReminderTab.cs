using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MultiTimerApp.Timers;

namespace MultiTimerApp.UI
{
    public class ReminderTab : UserControl
    {
        private ListView _reminderList;
        private Label _timeLabel;
        private NumericUpDown _minutesInput;
        private TextBox _nameInput;
        private Button _btnStart;
        private Button _btnStop;
        private Button _btnEdit;
        private Button _btnDelete;
        private System.Windows.Forms.Timer _uiTimer;

        private TimerManager _manager;
        private readonly List<ReminderTimer> _reminders = new();
        private ReminderTimer? _selected;

        public ReminderTab(TimerManager manager)
        {
            _manager = manager;
            InitializeUI();
            LoadReminders();
        }
        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            // --- Sidebar (list of reminders)
            var sidebar = new Panel { Dock = DockStyle.Right, Width = 250, BackColor = Color.WhiteSmoke };

            _reminderList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            _reminderList.Columns.Add("Name", 120);
            _reminderList.Columns.Add("Remaining", 100);
            _reminderList.SelectedIndexChanged += ReminderList_SelectedIndexChanged;

            sidebar.Controls.Add(_reminderList);

            // --- Main area
            _timeLabel = new Label
            {
                Text = "00:00:00",
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 28, FontStyle.Bold)
            };

            _nameInput = new TextBox
            {
                Dock = DockStyle.Top,
                PlaceholderText = "Reminder name..."
            };

            _minutesInput = new NumericUpDown
            {
                Dock = DockStyle.Top,
                Minimum = 1,
                Maximum = 1440,
                Value = 5
            };

            _btnStart = new Button { Text = "Start New Reminder", Dock = DockStyle.Top, Height = 40 };
            _btnStop = new Button { Text = "Stop", Dock = DockStyle.Top, Height = 40 };
            _btnEdit = new Button { Text = "Edit", Dock = DockStyle.Top, Height = 40 };
            _btnDelete = new Button { Text = "Delete", Dock = DockStyle.Top, Height = 40 };

            _btnStart.Click += StartReminder_Click;
            _btnStop.Click += StopReminder_Click;
            _btnEdit.Click += EditReminder_Click;
            _btnDelete.Click += DeleteReminder_Click;

            Controls.Add(_btnEdit);
            Controls.Add(_btnStop);
            Controls.Add(_btnStart);
            Controls.Add(_btnDelete);
            Controls.Add(_minutesInput);
            Controls.Add(_nameInput);
            Controls.Add(_timeLabel);
            Controls.Add(sidebar);

            _uiTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
        }

        
        private void StartReminder_Click(object? sender, EventArgs e)
        {
            string name = string.IsNullOrWhiteSpace(_nameInput.Text) ? "Reminder" : _nameInput.Text;
            var reminder = new ReminderTimer { Name = name };
            reminder.Start(TimeSpan.FromMinutes((double)_minutesInput.Value));

            _reminders.Add(reminder);
            _manager.AddTimer(reminder);
            _manager.Save();
            RefreshReminderList();
        }

        private void StopReminder_Click(object? sender, EventArgs e)
        {
            if (_selected != null)
            {
                _selected.Stop();
                RefreshReminderList();
            }
        }

        private void EditReminder_Click(object? sender, EventArgs e)
        {
            if (_selected != null)
            {
                _selected.Name = _nameInput.Text;
                _selected.DurationTicks = TimeSpan.FromMinutes((double)_minutesInput.Value).Ticks;
                if (_selected.IsRunning)
                    _selected.Start(TimeSpan.FromMinutes((double)_minutesInput.Value));

                RefreshReminderList();
            }
        }

        private void DeleteReminder_Click(object? sender, EventArgs e)
        {
            if (_selected != null)
            {
                _reminders.Remove(_selected);
                RefreshReminderList();
            }
        }

        private void ReminderList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_reminderList.SelectedItems.Count == 0)
                return;

            var selectedName = _reminderList.SelectedItems[0].Text;
            _selected = _reminders.FirstOrDefault(r => r.Name == selectedName);

            if (_selected != null)
            {
                _nameInput.Text = _selected.Name;
                _minutesInput.Value = (decimal)TimeSpan.FromTicks(_selected.DurationTicks).TotalMinutes;
            }
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            // Update time label for the selected reminder
            if (_selected != null)
            {
                _timeLabel.Text = _selected.IsExpired
                    ? "Expired!"
                    : _selected.GetRemaining().ToString(@"hh\:mm\:ss");
            }

            UpdateListViewItems();
        }

        private void UpdateListViewItems()
        {
            // Update each list item in place (no clearing/recreating)
            foreach (ListViewItem item in _reminderList.Items)
            {
                var reminder = _reminders.FirstOrDefault(r => r.Name == item.Text);
                if (reminder == null) continue;

                string remaining = reminder.IsExpired
                    ? "Expired"
                    : reminder.GetRemaining().ToString(@"hh\:mm\:ss");

                // Only update text if it changed
                if (item.SubItems[1].Text != remaining)
                    item.SubItems[1].Text = remaining;
            }
        }

        private void RefreshReminderList()
        {
            // Preserve selection
            string? selectedName = _reminderList.SelectedItems.Count > 0
                ? _reminderList.SelectedItems[0].Text
                : null;

            // Add new items that don’t exist yet
            foreach (var r in _reminders)
            {
                if (!_reminderList.Items.Cast<ListViewItem>().Any(i => i.Text == r.Name))
                {
                    var item = new ListViewItem(new[]
                    {
                        r.Name,
                        r.IsExpired ? "Expired" : r.GetRemaining().ToString(@"hh\:mm\:ss")
                    });
                    _reminderList.Items.Add(item);
                }
            }

            // Remove items that no longer exist
            foreach (ListViewItem item in _reminderList.Items.Cast<ListViewItem>().ToList())
            {
                if (!_reminders.Any(r => r.Name == item.Text))
                    _reminderList.Items.Remove(item);
            }

            // Reselect previously selected item
            if (selectedName != null)
            {
                foreach (ListViewItem item in _reminderList.Items)
                {
                    if (item.Text == selectedName)
                    {
                        item.Selected = true;
                        item.Focused = true;
                        break;
                    }
                }
            }
        }
        private void LoadReminders()
        {
            _reminders.Clear();
            _reminders.AddRange(_manager.Timers.OfType<ReminderTimer>());
            RefreshReminderList();
        }
    }
}

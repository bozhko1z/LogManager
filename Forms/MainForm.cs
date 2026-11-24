using LogManager.Factories;
using LogManager.Managers;
using LogManager.Models;
using LogManager.Strategies;

namespace LogManager.Forms;

public sealed class MainForm : Form
{
    private readonly ComboBox _typeFilter = new();
    private readonly ComboBox _newType = new();
    private readonly TextBox _message = new();
    private readonly TextBox _search = new();
    private readonly ListView _list = new();
    private readonly ToolStripStatusLabel _status = new();

    public MainForm()
    {
        Text = "Log Manager";
        Width = 1050;
        Height = 650;
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        BuildLayout();
        RefreshLogs();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var heading = new Label
        {
            Text = "LOG MANAGER", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(heading, 0, 0);

        var addPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(0), Margin = new Padding(0) };
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        addPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        addPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        addPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        _newType.DropDownStyle = ComboBoxStyle.DropDownList;
        _newType.DataSource = Enum.GetValues<LogType>();
        _message.Dock = DockStyle.Fill;
        _message.PlaceholderText = "Enter log message...";
        var addButton = new Button { Text = "Add log", Dock = DockStyle.Fill, Margin = new Padding(3) };
        addButton.Click += (_, _) => AddLog();

        addPanel.Controls.Add(new Label { Text = "Log type", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        addPanel.Controls.Add(new Label { Text = "Message", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft }, 1, 0);
        addPanel.Controls.Add(_newType, 0, 1);
        addPanel.Controls.Add(_message, 1, 1);
        addPanel.Controls.Add(addButton, 2, 1);
        var reload = new Button { Text = "Refresh", Dock = DockStyle.Fill, Margin = new Padding(3) };
        reload.Click += (_, _) => RefreshLogs();
        var openFileButton = new Button{Text = "Open Log File", Margin = new Padding(3), Width = 130, Height = 60 };
        openFileButton.Click += (_, _) => OpenLogFile();
        addPanel.Controls.Add(openFileButton, 2, 2);
        addPanel.SetColumnSpan(openFileButton, 2);
        addPanel.Controls.Add(reload, 3, 1);
        root.Controls.Add(addPanel, 0, 1);

        var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        _typeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _typeFilter.Width = 130;
        _typeFilter.Items.Add("All types");
        _typeFilter.Items.AddRange(Enum.GetNames<LogType>().Cast<object>().ToArray());
        _typeFilter.SelectedIndex = 0;
        _typeFilter.SelectedIndexChanged += (_, _) => RefreshLogs();
        _search.Width = 240;
        _search.PlaceholderText = "Search message...";
        var searchButton = new Button { Text = "Search", Height = 32 };
        searchButton.Click += (_, _) => RefreshLogs();
        var clearButton = new Button { Text = "Clear", Height = 32 };
        clearButton.Click += (_, _) => { _search.Clear(); _typeFilter.SelectedIndex = 0; RefreshLogs(); };
        filterPanel.Controls.Add(new Label { Text = "Filter:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        filterPanel.Controls.Add(_typeFilter);
        filterPanel.Controls.Add(_search);
        filterPanel.Controls.Add(searchButton);
        filterPanel.Controls.Add(clearButton);
        root.Controls.Add(filterPanel, 0, 2);

        _list.Dock = DockStyle.Fill;
        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.GridLines = true;
        _list.Columns.Add("Date and time", 180);
        _list.Columns.Add("Type", 110);
        _list.Columns.Add("Message", 650);
        root.Controls.Add(_list, 0, 3);

        var footer = new StatusStrip();
        _status.Text = "Ready";
        footer.Items.Add(_status);
        Controls.Add(footer);
    }

    private void AddLog()
    {
        try
        {
            var type = (LogType)_newType.SelectedItem!;
            var entry = LogFactory.Create(type, _message.Text);
            LogManager.Managers.LogManager.Instance.WriteLog(entry);
            _message.Clear();
            RefreshLogs();
            _status.Text = $"Saved to: {LogManager.Managers.LogManager.Instance.FilePath}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cannot save log", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshLogs()
    {
        try
        {
            IEnumerable<LogEntry> logs = LogManager.Managers.LogManager.Instance.ReadLogs();

            if (_typeFilter.SelectedIndex > 0)
            {
                var type = Enum.Parse<LogType>(_typeFilter.SelectedItem!.ToString()!);
                logs = new TypeFilterStrategy(type).Filter(logs);
            }

            logs = new SearchFilterStrategy(_search.Text).Filter(logs);

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (var log in logs)
            {
                var item = new ListViewItem(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(log.Type.ToString().ToUpperInvariant());
                item.SubItems.Add(log.Message);
                if (log.Type == LogType.Error) item.ForeColor = Color.Firebrick;
                else if (log.Type == LogType.Warning) item.ForeColor = Color.DarkOrange;
                else if (log.Type == LogType.Success) item.ForeColor = Color.ForestGreen;
                _list.Items.Add(item);
            }
            _list.EndUpdate();
            _status.Text = $"{_list.Items.Count} log(s) | {LogManager.Managers.LogManager.Instance.FilePath}";
        }
        catch (Exception ex)
        {
            _status.Text = "Read error: " + ex.Message;
        }
    }

    private void OpenLogFile()
    {
        using OpenFileDialog dialog = new OpenFileDialog();

        dialog.Filter = "Log files (*.jsonl)|*.jsonl|All files (*.*)|*.*";
        dialog.Title = "Open Log File";

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var logs = LogManager.Managers.LogManager
                    .Instance.ReadLogsFromFile(dialog.FileName);

                _list.Items.Clear();

                foreach (var log in logs)
                {
                    var item = new ListViewItem(
                        log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

                    item.SubItems.Add(log.Type.ToString().ToUpperInvariant());
                    item.SubItems.Add(log.Message);

                    _list.Items.Add(item);
                }

                _status.Text = $"Loaded {logs.Count} logs from {dialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "File Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

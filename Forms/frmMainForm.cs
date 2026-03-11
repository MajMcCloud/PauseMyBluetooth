using PauseMyBluetooth.APIs;
using PauseMyBluetooth.Models;
using System.ComponentModel;
using System.Reflection;

namespace PauseMyBluetooth;

public partial class frmMainForm : Form
{
    private readonly BindingList<BluetoothDevice> _devices = new();
    private System.Windows.Forms.Timer? _refreshTimer;

    public frmMainForm()
    {
        InitializeComponent();
        SetupDataGridView();
        SetupRefreshTimer();
        _ = RefreshDevicesAsync();

        lblVersion.Text = $"{Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
    }

    // -----------------------------------------------------------------------
    // DataGridView setup
    // -----------------------------------------------------------------------
    private void SetupDataGridView()
    {
        dgvDevices.AutoGenerateColumns = false;
        dgvDevices.DataSource = _devices;
        dgvDevices.RowHeadersVisible = false;
        dgvDevices.AllowUserToAddRows = false;
        dgvDevices.AllowUserToDeleteRows = false;
        dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDevices.MultiSelect = true;
        dgvDevices.ReadOnly = true;
        dgvDevices.BackgroundColor = Color.FromArgb(30, 30, 30);
        dgvDevices.GridColor = Color.FromArgb(60, 60, 60);
        dgvDevices.BorderStyle = BorderStyle.None;
        dgvDevices.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
        dgvDevices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvDevices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        dgvDevices.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 48);
        dgvDevices.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
        dgvDevices.DefaultCellStyle.ForeColor = Color.White;
        dgvDevices.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        dgvDevices.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvDevices.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
        dgvDevices.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 38);
        dgvDevices.ColumnHeadersHeight = 32;
        dgvDevices.RowTemplate.Height = 28;

        // Columns
        var colName = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "FriendlyName",
            HeaderText = "Device Name",
            Width = 240,
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };
        var colClass = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Class",
            HeaderText = "Class",
            Width = 120,
            ReadOnly = true
        };
        var colStatus = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Status",
            HeaderText = "Status",
            Width = 100,
            ReadOnly = true
        };
        var colAutoConnect = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "AutoConnectEnabled",
            HeaderText = "Auto-Connect",
            Width = 180,
            ReadOnly = true
        };

        dgvDevices.Columns.AddRange(colName, colClass, colStatus, colAutoConnect);

        // Color rows based on auto-connect state
        dgvDevices.CellFormatting += DgvDevices_CellFormatting;
    }

    private void DgvDevices_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _devices.Count) return;
        var dev = _devices[e.RowIndex];

        // Auto-Connect column: show friendly text
        if (dgvDevices.Columns[e.ColumnIndex].DataPropertyName == "AutoConnectEnabled")
        {
            e.Value = dev.AutoConnectEnabled ? "✔ Enabled" : "✘ Disabled";
            e.FormattingApplied = true;
            if (!dgvDevices.Rows[e.RowIndex].Selected)
                e.CellStyle!.ForeColor = dev.AutoConnectEnabled
                    ? Color.FromArgb(80, 200, 120)
                    : Color.FromArgb(220, 80, 80);
        }

        // Status column color
        if (dgvDevices.Columns[e.ColumnIndex].DataPropertyName == "Status"
            && !dgvDevices.Rows[e.RowIndex].Selected)
        {
            e.CellStyle!.ForeColor = dev.Status switch
            {
                "OK" => Color.FromArgb(80, 200, 120),
                "Disabled" => Color.FromArgb(220, 80, 80),
                "Error" => Color.FromArgb(255, 150, 50),
                _ => Color.White
            };
            e.FormattingApplied = true;
        }
    }

    // -----------------------------------------------------------------------
    // Auto-refresh every 30 s
    // -----------------------------------------------------------------------
    private void SetupRefreshTimer()
    {
        _refreshTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _refreshTimer.Tick += async (_, _) => await RefreshDevicesAsync();
        // Start timer only if the checkbox is checked (checkbox created in designer)
        try
        {
            _refreshTimer.Enabled = chkAutoRefresh?.Checked ?? false;
        }
        catch
        {
            _refreshTimer.Enabled = false;
        }
    }

    private void chkAutoRefresh_CheckedChanged(object? sender, EventArgs e)
    {
        if (_refreshTimer == null) return;

        if (chkAutoRefresh.Checked)
        {
            _refreshTimer.Start();
            lblStatus.Text = "Auto-Refresh enabled.";
            lblStatus.ForeColor = Color.FromArgb(180, 180, 180);
        }
        else
        {
            _refreshTimer.Stop();
            lblStatus.Text = "Auto-Refresh disabled";
            lblStatus.ForeColor = Color.FromArgb(180, 180, 180);
        }
    }

    // -----------------------------------------------------------------------
    // Refresh
    // -----------------------------------------------------------------------
    private async Task RefreshDevicesAsync()
    {
        btnRefresh.Enabled = false;
        lblStatus.Text = "Loading devices …";
        lblStatus.ForeColor = Color.FromArgb(180, 180, 180);

        try
        {
            var devices = await Task.Run(DeviceService.GetPairedDevices);

            _devices.Clear();
            foreach (var d in devices)
                _devices.Add(d);

            lblStatus.Text = $"{_devices.Count} device(s) found — {DateTime.Now:HH:mm:ss}";
            lblStatus.ForeColor = Color.FromArgb(80, 200, 120);
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error: {ex.Message}";
            lblStatus.ForeColor = Color.FromArgb(220, 80, 80);
        }
        finally
        {
            btnRefresh.Enabled = true;
            UpdateButtonStates();
        }
    }

    // -----------------------------------------------------------------------
    // Toggle Auto-Connect
    // -----------------------------------------------------------------------
    private async void btnToggle_Click(object sender, EventArgs e)
    {
        if (dgvDevices.CurrentRow == null) return;

        btnToggle.Enabled = false;
        btnRefresh.Enabled = false;
        lblStatus.Text = "Changing setting …";
        lblStatus.ForeColor = Color.FromArgb(255, 200, 50);

        try
        {
            foreach (var rows in dgvDevices.SelectedRows)
            {
                if ((rows as DataGridViewRow)?.DataBoundItem is not BluetoothDevice dev)
                    continue;

                bool disabling = dev.AutoConnectEnabled;

                await Task.Run(() =>
                {
                    if (disabling)
                        DeviceService.DisableDevice(dev);
                    else
                        DeviceService.EnableDevice(dev);
                });

                dev.AutoConnectEnabled = !disabling;
                dev.Status = dev.AutoConnectEnabled ? "OK" : "Disabled";
                _devices.ResetItem(_devices.IndexOf(dev));

                lblStatus.Text = $"{dev.FriendlyName}: {(disabling ? "Disabled" : "Enabled")}";
                lblStatus.ForeColor = Color.FromArgb(80, 200, 120);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error: {ex.Message}";
            lblStatus.ForeColor = Color.FromArgb(220, 80, 80);
        }
        finally
        {
            btnToggle.Enabled = true;
            btnRefresh.Enabled = true;
            UpdateButtonStates();
        }
    }

    // -----------------------------------------------------------------------
    // Refresh button
    // -----------------------------------------------------------------------
    private async void btnRefresh_Click(object sender, EventArgs e)
        => await RefreshDevicesAsync();

    // -----------------------------------------------------------------------
    // Selection change → update toggle button label
    // -----------------------------------------------------------------------
    private void dgvDevices_SelectionChanged(object sender, EventArgs e)
        => UpdateButtonStates();

    private void UpdateButtonStates()
    {
        if (dgvDevices.CurrentRow?.DataBoundItem is BluetoothDevice dev)
        {
            btnToggle.Enabled = true;
            if (dev.AutoConnectEnabled)
            {
                btnToggle.Text = "Disable Auto-Connect";
                btnToggle.BackColor = Color.FromArgb(220, 80, 80);
            }
            else
            {
                btnToggle.Text = "Enable Auto-Connect";
                btnToggle.BackColor = Color.FromArgb(80, 200, 120);
            }
        }
        else
        {
            btnToggle.Enabled = false;
            btnToggle.Text = "Toggle Auto-Connect";
        }
    }
}

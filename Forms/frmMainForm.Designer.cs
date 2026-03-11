namespace PauseMyBluetooth;

partial class frmMainForm
{
    private System.ComponentModel.IContainer components = null!;

    private DataGridView dgvDevices = null!;
    private Button btnRefresh = null!;
    private Button btnToggle = null!;
    private Label lblStatus = null!;
    private Label lblTitle = null!;
    private Panel pnlTop = null!;
    private Panel pnlBottom = null!;
    private CheckBox chkAutoRefresh = null!; // added checkbox for auto-refresh

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ── Form ──────────────────────────────────────────────────────────
        Text = "Bluetooth Auto-Connect Manager";
        Size = new Size(720, 520);
        MinimumSize = new Size(600, 400);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(25, 25, 25);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9f);

        // ── Top panel ─────────────────────────────────────────────────────
        pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.FromArgb(0, 100, 180),
            Padding = new Padding(14, 0, 14, 0)
        };

        lblTitle = new Label
        {
            Text = "🔵  Bluetooth Auto-Connect Manager",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.White
        };
        pnlTop.Controls.Add(lblTitle);

        // ── Bottom panel ──────────────────────────────────────────────────
        pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = Color.FromArgb(30, 30, 30),
            Padding = new Padding(10, 8, 10, 8)
        };

        btnRefresh = new Button
        {
            Text = "↻  Aktualisieren",
            Width = 148,
            Height = 34,
            Dock = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 100, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += btnRefresh_Click;

        btnToggle = new Button
        {
            Text = "Auto-Connect umschalten",
            Width = 250,
            Height = 34,
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 60, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnToggle.FlatAppearance.BorderSize = 0;
        btnToggle.Click += btnToggle_Click;

        lblStatus = new Label
        {
            Text = "Bereit.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(180, 180, 180),
            Font = new Font("Segoe UI", 8.5f)
        };

        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Controls.Add(btnRefresh);
        pnlBottom.Controls.Add(btnToggle);

        // ── DataGridView ──────────────────────────────────────────────────
        dgvDevices = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        dgvDevices.SelectionChanged += dgvDevices_SelectionChanged;

        // ── Add controls ──────────────────────────────────────────────────
        Controls.Add(dgvDevices);
        Controls.Add(pnlBottom);
        Controls.Add(pnlTop);
    }

}

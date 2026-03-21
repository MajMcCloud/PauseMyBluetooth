// Copyright (c) 2026 Florian Zevedei
// Repository: https://github.com/MajMcCloud/PauseMyBluetooth

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
    private Panel pnlRepoBottom = null!; // neues, oben angedocktes Panel für Repo-Link
    private Panel pnlBottom = null!;
    private CheckBox chkAutoRefresh = null!; // checkbox for auto-refresh
    private LinkLabel llRepo = null!; // link to repository
    private Label lblCopyright = null!;
    private Label lblVersion = null!;

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
        Text = "PauseMyBluetooth";
        Size = new Size(920, 520);
        MinimumSize = new Size(620, 400);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(25, 25, 25);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9f);
        ShowIcon = false;

        // ── Repo top panel (separat, oberes Panel) ─────────────────────────
        pnlRepoBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 35,
            BackColor = Color.FromArgb(30, 30, 30),
            Padding = new Padding(14, 0, 14, 10),
            BorderStyle = BorderStyle.FixedSingle
        };

        llRepo = new LinkLabel
        {
            Text = "GitHub: MajMcCloud/PauseMyBluetooth",
            Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight,
            LinkColor = Color.White,
            ActiveLinkColor = Color.LightBlue,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 8, 5),
            AutoSize = true,
            Cursor = Cursors.Hand
        };

        llRepo.LinkClicked += LlRepo_LinkClicked;

        pnlRepoBottom.Controls.Add(llRepo);


        lblCopyright = new Label()
        {
            Text = "Made by Florian Zevedei",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            AutoSize = true
        };

        pnlRepoBottom.Controls.Add(lblCopyright);

        // ── Top panel (Titel) ──────────────────────────────────────────────
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

        lblVersion = new Label()
        {
            Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleCenter,
            Width = 100
        };

        pnlTop.Controls.Add(lblVersion);

        // ── Bottom panel ──────────────────────────────────────────────────
        pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = Color.FromArgb(30, 30, 30),
            Padding = new Padding(10, 8, 10, 8)
        };

        chkAutoRefresh = new CheckBox
        {
            Text = "Auto-Refresh",
            Width = 200,
            Height = 34,
            Dock = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand,
            Checked = false,
            Padding = new Padding(10, 0, 0, 0)
        };
        chkAutoRefresh.CheckedChanged += chkAutoRefresh_CheckedChanged;

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
            Cursor = Cursors.Hand,
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

        // Add controls to bottom panel.
        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Controls.Add(btnToggle);
        pnlBottom.Controls.Add(chkAutoRefresh);
        pnlBottom.Controls.Add(btnRefresh);

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
        Controls.Add(pnlRepoBottom); // Repo-Panel direkt unter dem Titel-Panel
        Controls.Add(pnlTop);
    }

}

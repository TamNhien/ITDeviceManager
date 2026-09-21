using ITDeviceManager.Common;
using ITDeviceManager.Services;
using System.Runtime.InteropServices;

namespace ITDeviceManager.Forms;

public class MainForm : AppForm
{
    private const int WmSetRedraw = 0x000B;
    private const int WsExComposited = 0x02000000;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    // Buffers the complete child-page surface. Native WinForms controls such as
    // ComboBox/DateTimePicker are HWND-based and can briefly paint their default
    // white theme while a page is detached. Compositing the content host prevents
    // those intermediate frames from reaching the screen.
    private sealed class FlickerFreePanel : Panel
    {
        public FlickerFreePanel()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.ExStyle |= WsExComposited;
                return parameters;
            }
        }
    }

    // Sidebar navigation must not steal keyboard focus from an input before the
    // old child page has been frozen. Otherwise the native input receives a
    // WM_KILLFOCUS repaint first and may flash its Windows default white chrome.
    private sealed class NavigationButton : Button
    {
        public NavigationButton()
        {
            TabStop = false;
            SetStyle(ControlStyles.Selectable, false);
        }
    }

    private readonly FlickerFreePanel _content = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(20),
        BackColor = AppTheme.Background
    };

    private readonly Label _pageTitle = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 18F),
        ForeColor = AppTheme.TextPrimary,
        Location = new Point(76, 18)
    };

    private readonly Label _pageSubtitle = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5F),
        ForeColor = AppTheme.TextSecondary,
        Location = new Point(78, 49),
        Text = "Quản lý tài sản và thiết bị CNTT trong doanh nghiệp"
    };

    private readonly List<Button> _navButtons = [];
    private readonly ToolTip _toolTip = new();
    private Panel? _sidebar;
    private Button? _menuToggle;
    private Form? _currentChild;
    private Button? _activeNav;

    public bool LogoutRequested { get; private set; }

    public MainForm()
    {
        Text = "Quản lý thiết bị CNTT trong doanh nghiệp";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1180, 760);
        Font = new Font("Segoe UI", 10F);
        BackColor = AppTheme.Background;
        KeyPreview = true;

        var sidebar = BuildSidebar();
        var workspace = BuildWorkspace();

        Controls.Add(workspace);
        Controls.Add(sidebar);

        Shown += (_, _) =>
        {
            // Open the default page directly instead of simulating a click.
            // BeginInvoke lets the maximized MainForm finish its first layout/paint,
            // then swaps in Dashboard as one composed frame.
            BeginInvoke(OpenDefaultDashboard);
        };
        KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.M)
            {
                ToggleSidebar();
                e.SuppressKeyPress = true;
            }
        };
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 252,
            BackColor = AppTheme.Sidebar,
            Padding = new Padding(14, 14, 14, 16)
        };
        _sidebar = sidebar;

        var brand = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = AppTheme.Sidebar
        };

        var logo = new PictureBox
        {
            Width = 44,
            Height = 44,
            Location = new Point(4, 8),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        try
        {
            logo.Image = Icon?.ToBitmap();
        }
        catch
        {
            // Logo is optional; keep the brand text if the icon cannot be loaded.
        }

        brand.Controls.Add(logo);
        brand.Controls.Add(new Label
        {
            Text = "IT DEVICE",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13F),
            ForeColor = Color.White,
            Location = new Point(58, 8)
        });
        brand.Controls.Add(new Label
        {
            Text = "MANAGER",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13F),
            ForeColor = Color.FromArgb(147, 197, 253),
            Location = new Point(58, 31)
        });

        var user = AppSession.CurrentUser;
        var userCard = new ModernCard
        {
            Dock = DockStyle.Top,
            Height = 88,
            Margin = new Padding(0, 0, 0, 12),
            BackColor = AppTheme.SurfaceAlt,
            BorderColor = AppTheme.BorderStrong,
            Padding = new Padding(12)
        };

        var initials = new Label
        {
            Text = GetInitials(user?.FullName),
            Width = 44,
            Height = 44,
            Location = new Point(12, 20),
            BackColor = AppTheme.Primary,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 10.5F)
        };
        initials.Resize += (_, _) => AppTheme.ApplyRoundedRegion(initials, 22);

        userCard.Controls.Add(initials);
        userCard.Controls.Add(new Label
        {
            Text = user?.FullName ?? "Người dùng",
            AutoEllipsis = true,
            Width = 142,
            Height = 24,
            Location = new Point(68, 18),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9.5F)
        });
        userCard.Controls.Add(new Label
        {
            Text = $"Quyền: {user?.Role.Name ?? "Staff"}",
            AutoEllipsis = true,
            Width = 142,
            Height = 22,
            Location = new Point(68, 43),
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9F)
        });

        var navHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = AppTheme.Sidebar,
            Padding = new Padding(0, 12, 0, 0)
        };

        AddNav(navHost, "Tổng quan", "Tổng quan", () => new DashboardForm());
        AddNav(navHost, "Thiết bị", "Quản lý thiết bị", () => new DevicesForm());
        AddNav(navHost, "Loại thiết bị", "Loại thiết bị", () => new DeviceTypesForm());
        AddNav(navHost, "Nhân viên", "Nhân viên", () => new EmployeesForm());
        AddNav(navHost, "Phòng ban", "Phòng ban", () => new DepartmentsForm());
        AddNav(navHost, "Cấp phát / Thu hồi", "Cấp phát / Thu hồi", () => new AssignmentsForm());
        AddNav(navHost, "Bảo trì / Sửa chữa", "Bảo trì / Sửa chữa / Bảo hành", () => new MaintenancesForm());

        if (AppSession.IsAdmin)
        {
            AddNav(navHost, "Tài khoản", "Quản lý tài khoản", () => new UsersForm());
            AddNav(navHost, "Nhật ký hoạt động", "Audit Log / Nhật ký hoạt động", () => new AuditLogsForm());
            AddNav(navHost, "Sao lưu / Phục hồi", "Sao lưu / Phục hồi SQL Server", () => new BackupRestoreForm());
        }

        var logout = new Button
        {
            Text = "Đăng xuất",
            Dock = DockStyle.Bottom,
            Height = 44,
            Margin = Padding.Empty
        };
        AppTheme.SetButtonRole(logout, ButtonRole.Danger);
        logout.Click += async (_, _) =>
        {
            logout.Enabled = false;
            var user = AppSession.CurrentUser;
            if (user is not null)
            {
                await AuditService.TryWriteAsync(
                    "Đăng xuất",
                    "Phiên làm việc",
                    $"Đăng xuất tài khoản {user.Username}.",
                    user.Username,
                    user.Username,
                    user.Id);
            }

            LogoutRequested = true;
            AppSession.SignOut();
            Close();
        };

        sidebar.Controls.Add(navHost);
        sidebar.Controls.Add(logout);
        sidebar.Controls.Add(userCard);
        sidebar.Controls.Add(brand);
        return sidebar;
    }

    private Control BuildWorkspace()
    {
        var workspace = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Background
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = AppTheme.Surface
        };
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(AppTheme.Border);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };
        _menuToggle = new Button
        {
            Name = "MenuToggleButton",
            Text = "☰",
            Width = 40,
            Height = 38,
            Location = new Point(20, 20),
            TabStop = false
        };
        AppTheme.SetButtonRole(_menuToggle, ButtonRole.Secondary);
        _toolTip.SetToolTip(_menuToggle, "Ẩn / hiện menu bên trái (Ctrl+M)");
        _menuToggle.Click += (_, _) => ToggleSidebar();

        header.Controls.Add(_menuToggle);
        header.Controls.Add(_pageTitle);
        header.Controls.Add(_pageSubtitle);

        var role = AppSession.CurrentUser?.Role.Name ?? "Staff";
        var roleBadge = new Label
        {
            Text = role,
            AutoSize = false,
            Width = 86,
            Height = 30,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(header.Width - 110, 24),
            BackColor = Color.FromArgb(15, 34, 63),
            ForeColor = Color.FromArgb(147, 197, 253),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 9F)
        };
        roleBadge.Resize += (_, _) => AppTheme.ApplyRoundedRegion(roleBadge, 15);
        header.Controls.Add(roleBadge);
        header.Resize += (_, _) => roleBadge.Left = header.ClientSize.Width - roleBadge.Width - 24;

        workspace.Controls.Add(_content);
        workspace.Controls.Add(header);
        return workspace;
    }

    private void OpenDefaultDashboard()
    {
        if (IsDisposed || Disposing || _currentChild is not null || _navButtons.Count == 0)
            return;

        var overviewButton = _navButtons[0];
        SetActiveNavigation(overviewButton);
        _pageTitle.Text = "Tổng quan";
        OpenChild(new DashboardForm());

        // Force the active-navigation color into the first composed frame so the
        // user never sees the unselected state after login.
        overviewButton.Invalidate();
        overviewButton.Update();
    }

    private void ToggleSidebar()
    {
        if (_sidebar is null) return;
        _sidebar.Visible = !_sidebar.Visible;
        if (_menuToggle is not null)
        {
            _menuToggle.Text = "☰";
            _toolTip.SetToolTip(
                _menuToggle,
                _sidebar.Visible ? "Ẩn menu bên trái (Ctrl+M)" : "Hiện menu bên trái (Ctrl+M)");
        }
    }

    private void AddNav(FlowLayoutPanel sidebar, string text, string pageTitle, Func<Form> formFactory)
    {
        var btn = new NavigationButton
        {
            Text = text,
            Width = 216,
            Height = 44,
            Margin = new Padding(0, 3, 0, 3),
            TextAlign = ContentAlignment.MiddleLeft
        };
        AppTheme.SetButtonRole(btn, ButtonRole.Navigation);
        btn.Click += (_, _) =>
        {
            SetActiveNavigation(btn);
            _pageTitle.Text = pageTitle;
            OpenChild(formFactory());
        };
        _navButtons.Add(btn);
        sidebar.Controls.Add(btn);
    }

    private void SetActiveNavigation(Button button)
    {
        if (_activeNav is not null && !_activeNav.IsDisposed)
            AppTheme.SetButtonRole(_activeNav, ButtonRole.Navigation);

        _activeNav = button;
        AppTheme.SetButtonRole(button, ButtonRole.NavigationActive);
    }

    private void OpenChild(Form form)
    {
        var previous = _currentChild;

        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        form.BackColor = AppTheme.Background;
        form.Visible = false;

        // Apply the dark palette before any HWND-backed input becomes visible.
        // OnShown applies it again after handle creation, which is intentional.
        AppTheme.ApplyForm(form);

        SetRedraw(_content, enabled: false);
        if (previous is not null)
            SetRedraw(previous, enabled: false);

        _content.SuspendLayout();
        try
        {
            // Hide and detach the old page while redraw is suspended. Do not use
            // Controls.Clear(): detaching/disposal of native child HWNDs can expose
            // their default white background for one or two frames.
            if (previous is not null && !previous.IsDisposed)
            {
                previous.Hide();
                _content.Controls.Remove(previous);
            }

            _currentChild = form;
            _content.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }
        finally
        {
            _content.ResumeLayout(performLayout: true);
            SetRedraw(form, enabled: true);
            SetRedraw(_content, enabled: true);

            // Present one fully composed dark frame after the swap instead of
            // exposing intermediate native-control paints.
            _content.Invalidate(invalidateChildren: true);
            _content.Update();
        }

        // Dispose only after the old page is hidden and detached from the visible
        // tree, so its native controls can no longer flash on screen.
        if (previous is not null && !previous.IsDisposed)
        {
            previous.Close();
            previous.Dispose();
        }
    }

    private static void SetRedraw(Control control, bool enabled)
    {
        if (control.IsDisposed || !control.IsHandleCreated) return;
        _ = SendMessage(
            control.Handle,
            WmSetRedraw,
            enabled ? (IntPtr)1 : IntPtr.Zero,
            IntPtr.Zero);
    }

    private static string GetInitials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "IT";
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }
}

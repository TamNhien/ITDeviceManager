using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class MainForm : AppForm
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(8) };
    private Form? _currentChild;
    public bool LogoutRequested { get; private set; }

    public MainForm()
    {
        Text = "Quản lý thiết bị CNTT trong doanh nghiệp";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        Font = new Font("Segoe UI", 10);

        var sidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 220,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10)
        };

        var user = AppSession.CurrentUser;
        sidebar.Controls.Add(new Label
        {
            Text = "IT DEVICE MANAGER",
            Width = 190,
            Height = 55,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        });
        sidebar.Controls.Add(new Label
        {
            Text = $"{user?.FullName}\nQuyền: {user?.Role.Name}",
            Width = 190,
            Height = 54,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.DimGray
        });

        AddNav(sidebar, "Tổng quan", () => OpenChild(new DashboardForm()));
        AddNav(sidebar, "Thiết bị", () => OpenChild(new DevicesForm()));
        AddNav(sidebar, "Loại thiết bị", () => OpenChild(new DeviceTypesForm()));
        AddNav(sidebar, "Nhân viên", () => OpenChild(new EmployeesForm()));
        AddNav(sidebar, "Phòng ban", () => OpenChild(new DepartmentsForm()));
        AddNav(sidebar, "Cấp phát / Thu hồi", () => OpenChild(new AssignmentsForm()));

        if (AppSession.IsAdmin)
            AddNav(sidebar, "Tài khoản", () => OpenChild(new UsersForm()));

        var logout = new Button { Text = "Đăng xuất", Width = 190, Height = 40, Margin = new Padding(3, 24, 3, 3) };
        logout.Click += (_, _) =>
        {
            LogoutRequested = true;
            AppSession.SignOut();
            Close();
        };
        sidebar.Controls.Add(logout);

        Controls.Add(_content);
        Controls.Add(sidebar);
        Shown += (_, _) => OpenChild(new DashboardForm());
    }

    private static void AddNav(Control sidebar, string text, Action action)
    {
        var btn = new Button { Text = text, Width = 190, Height = 42, Margin = new Padding(3, 4, 3, 4) };
        btn.Click += (_, _) => action();
        sidebar.Controls.Add(btn);
    }

    private void OpenChild(Form form)
    {
        _currentChild?.Close();
        _currentChild?.Dispose();
        _currentChild = form;
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        _content.Controls.Clear();
        _content.Controls.Add(form);
        form.Show();
    }
}

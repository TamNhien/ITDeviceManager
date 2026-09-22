using ITDeviceManager.Common;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public sealed class PermissionsForm : AppForm
{
    private readonly DarkComboBox _roles = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false
    };
    private readonly Label _hint = new()
    {
        AutoSize = true,
        ForeColor = AppTheme.TextSecondary,
        Text = "Chọn vai trò rồi đánh dấu các quyền được phép. Admin luôn có toàn bộ quyền."
    };
    private readonly Button _save = new() { Text = "Lưu phân quyền", Width = 150, Height = 36 };
    private bool _loading;

    public PermissionsForm()
    {
        Text = "Phân quyền chi tiết";
        BackColor = AppTheme.Background;
        Font = new Font("Segoe UI", 10F);
        Padding = new Padding(4);

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 72,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 12, 8, 8),
            BackColor = AppTheme.Surface
        };
        top.Controls.Add(new Label
        {
            Text = "Vai trò:",
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 0),
            ForeColor = AppTheme.TextPrimary
        });
        top.Controls.Add(_roles);
        _hint.Margin = new Padding(18, 8, 0, 0);
        top.Controls.Add(_hint);

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Granted",
            HeaderText = "Cho phép",
            Width = 82,
            DataPropertyName = "Granted"
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Group",
            HeaderText = "Nhóm",
            Width = 150,
            ReadOnly = true,
            DataPropertyName = "Group"
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "Quyền",
            Width = 220,
            ReadOnly = true,
            DataPropertyName = "Name"
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Description",
            HeaderText = "Mô tả",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
            DataPropertyName = "Description"
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PermissionId",
            Visible = false,
            DataPropertyName = "PermissionId"
        });
        AppTheme.StyleGrid(_grid);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8, 10, 8, 8),
            BackColor = AppTheme.Surface
        };
        AppTheme.SetButtonRole(_save, ButtonRole.Primary);
        bottom.Controls.Add(_save);

        Controls.Add(_grid);
        Controls.Add(bottom);
        Controls.Add(top);

        Load += async (_, _) => await LoadRolesAsync();
        _roles.SelectedIndexChanged += async (_, _) =>
        {
            if (!_loading) await LoadPermissionsAsync();
        };
        _save.Click += async (_, _) => await SaveAsync();
    }

    private async Task LoadRolesAsync()
    {
        _loading = true;
        try
        {
            var roles = await PermissionService.GetRolesAsync();
            _roles.DataSource = roles.ToList();
            _roles.DisplayMember = nameof(RoleSummary.Name);
            _roles.ValueMember = nameof(RoleSummary.Id);
        }
        finally
        {
            _loading = false;
        }
        await LoadPermissionsAsync();
    }

    private async Task LoadPermissionsAsync()
    {
        if (_roles.SelectedValue is not int roleId)
            return;

        var roleName = (_roles.SelectedItem as RoleSummary)?.Name ?? string.Empty;
        var rows = await PermissionService.GetRolePermissionsAsync(roleId);
        _grid.DataSource = rows.Select(x => new PermissionGridRow
        {
            PermissionId = x.PermissionId,
            Group = x.Group,
            Name = x.Name,
            Description = x.Description,
            Granted = string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) || x.Granted
        }).ToList();

        var isAdmin = string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);
        _grid.ReadOnly = isAdmin;
        _save.Enabled = !isAdmin;
        _save.Visible = !isAdmin;
        _hint.Text = isAdmin
            ? "Admin luôn có toàn bộ quyền và không thể bị giới hạn."
            : "Đánh dấu quyền cần cấp rồi bấm Lưu phân quyền.";
    }

    private async Task SaveAsync()
    {
        if (_roles.SelectedValue is not int roleId)
            return;

        _grid.EndEdit();
        var rows = _grid.DataSource as List<PermissionGridRow> ?? [];
        var ids = rows.Where(x => x.Granted).Select(x => x.PermissionId).ToArray();

        try
        {
            await PermissionService.SaveRolePermissionsAsync(roleId, ids);
            await AuditService.TryWriteAsync(
                "Cập nhật phân quyền",
                "Vai trò",
                $"Cập nhật {ids.Length} quyền cho vai trò {(_roles.SelectedItem as RoleSummary)?.Name}.",
                roleId.ToString(),
                (_roles.SelectedItem as RoleSummary)?.Name);
            MessageBox.Show("Đã lưu phân quyền.", "Phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể lưu phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class PermissionGridRow
    {
        public int PermissionId { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Granted { get; set; }
    }
}

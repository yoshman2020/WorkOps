# メモ

SQL Server から SQLite に変更

## WorkOps.csproj
```
    <!--<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.10" />-->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.10" />
```
## Program.cs
```
    //options.UseSqlServer(connectionString));
    options.UseSqlite(connectionString));
```

## appsettings.json
```
    //"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-WorkOps-6d957cb1-99d0-42ba-afb9-bd8a84c849f0;Trusted_Connection=True;MultipleActiveResultSets=true"
    "DefaultConnection": "Data Source=.\\workops.db"
```

## PowerShell
```
dotnet ef database update 0
dotnet ef migrations remove
dotnet ef migrations add InitialCreate --output-dir Data\Migrations --namespace WorkOps.Migrations

Migrationsのファイルの日付を2025～から0000～に変更
0000～Designer.csの[Migration("00000000000000_InitialCreate")]に変更

dotnet ef database update
```

# スキャフォールディング

Pagesを右クリックし追加＞新規スキャフォールディングアイテム
＞Razor Pages using Entity Framework (CRUD)＞Modelにモデルを選択＞Data Context
作成されたページの以下を修正

```
@* @inject IDbContextFactory<WorkOps.Data.ApplicationDbContext> DbFactory *@
@inject WorkOps.Data.ApplicationDbContext DbContext

<EditForm method="post" Model="TTableName"～

@code {
    [SupplyParameterFromForm]
    //private TTableName TTableName { get; set; } = new();
    private InputModel TTableName { get; set; } = new();

    private sealed class InputModel : BaseInputModel
    {
    }
```

Edit.razorの場合は更に
```
@rendermode @(new InteractiveServerRenderMode(prerender: false))

var tTableName = await DbContext.TTableNames.FirstOrDefaultAsync(m => m.Id == Id);
if (tTableName is null)
{
    NavigationManager.NavigateTo("notfound");
}
TTableName = new InputModel
{
    Name = tTableName?.Name ?? string.Empty,
    Remarks = tTableName?.Remarks ?? string.Empty,
};
```
UpdateMCustomerも同様に修正

# 項目追加
dotnet ef migrations add AddPrevReminderEnabled --output-dir Data\Migrations --namespace WorkOps.Migrations
dotnet ef database update

# テーブル追加
ApplicationDbContextに
public DbSet<TPaidLeave> TPaidLeave { get; set; } = default!;
dotnet ef migrations add AddTPaidLeave --output-dir Data\Migrations --namespace WorkOps.Migrations
dotnet ef database update

# メールパスワード（開発用）
dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:User" "user@example.com"
dotnet user-secrets set "Smtp:Password" "your-password"

# メールパスワード（IIS）
web.config
```xml
<aspNetCore>
  <environmentVariables>
    <environmentVariable name="Smtp__Host" value="smtp.example.com" />
    <environmentVariable name="Smtp__Port" value="587" />
    <environmentVariable name="Smtp__User" value="user@example.com" />
    <environmentVariable name="Smtp__Password" value="prod-password" />
  </environmentVariables>
</aspNetCore>
```

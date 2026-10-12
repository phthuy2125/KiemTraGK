using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace KiemTraGiuaKy.EntityFrameworkCore;

public static class KiemTraGiuaKyDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<KiemTraGiuaKyDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString)
               .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public static void Configure(DbContextOptionsBuilder<KiemTraGiuaKyDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection)
               .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}

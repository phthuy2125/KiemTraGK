using KiemTraGiuaKy.Debugging;

namespace KiemTraGiuaKy;

public class KiemTraGiuaKyConsts
{
    public const string LocalizationSourceName = "KiemTraGiuaKy";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "472e3c617746466faa5cca0440cb4ac8";
}

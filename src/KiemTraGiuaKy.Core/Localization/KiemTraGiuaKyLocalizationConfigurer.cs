using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace KiemTraGiuaKy.Localization;

public static class KiemTraGiuaKyLocalizationConfigurer
{
    public static void Configure(ILocalizationConfiguration localizationConfiguration)
    {
        localizationConfiguration.Sources.Add(
            new DictionaryBasedLocalizationSource(KiemTraGiuaKyConsts.LocalizationSourceName,
                new XmlEmbeddedFileLocalizationDictionaryProvider(
                    typeof(KiemTraGiuaKyLocalizationConfigurer).GetAssembly(),
                    "KiemTraGiuaKy.Localization.SourceFiles"
                )
            )
        );
    }
}

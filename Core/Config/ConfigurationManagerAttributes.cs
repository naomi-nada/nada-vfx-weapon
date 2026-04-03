using System;
using BepInEx.Configuration;

namespace NADA.VFX.Core.Config
{
    #pragma warning disable CS0649
    
    internal sealed class ConfigurationManagerAttributes
    {
        public bool? ShowRangeAsPercent;
        public Action<ConfigEntryBase> CustomDrawer;
        public Action<ConfigEntryBase> CustomHotkeyDrawer;
        public int? Order;
        public bool? Browsable;
        public string Category;
        public object DefaultValue;
        public bool? HideDefaultButton;
        public bool? HideSettingName;
        public string Description;
        public string DispName;
        public bool? ReadOnly;
        public bool? IsAdvanced;
        public Func<object, string> ObjToStr;
        public Func<string, object> StrToObj;
    }
    
    #pragma warning restore CS0649
}
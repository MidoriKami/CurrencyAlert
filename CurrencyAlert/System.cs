using CurrencyAlert.Classes;
using CurrencyAlert.Windows;
using KamiLib.CommandManager;
using KamiLib.Window;
using KamiToolKit;

namespace CurrencyAlert;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public static class System {
    public static Configuration Config { get; set; }
    public static CommandManager CommandManager { get; set; }
    public static WindowManager WindowManager { get; set; }
    public static OverlayController OverlayController { get; set; }
    public static ConfigurationWindow ConfigurationWindow { get; set; }
    public static NativeController NativeController { get; set; }
}
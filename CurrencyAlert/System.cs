using CurrencyAlert.Models.Config;
using KamiLib.CommandManager;
using KamiLib.Window;

namespace CurrencyAlert;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class System {
    public static Configuration Config { get; set; }
    public static CommandManager CommandManager { get; set; }
    public static WindowManager WindowManager { get; set; }
}
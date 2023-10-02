using CurrencyAlert.Controllers;
using CurrencyAlert.Views.Windows.Config;
using CurrencyAlert.Views.Windows.Overlay;
using Dalamud.Plugin;
using KamiLib;
using KamiLib.Commands;

namespace CurrencyAlert;

public sealed class CurrencyAlertPlugin : IDalamudPlugin
{
    public string Name => "CurrencyAlert";

    public static CurrencyAlertSystem System = null!;

    public CurrencyAlertPlugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        KamiCommon.Initialize(pluginInterface, Name);

        System = new CurrencyAlertSystem();

        CommandController.RegisterMainCommand("/calert", "/currencyalert");
        KamiCommon.WindowManager.AddConfigurationWindow(new ConfigurationWindow(), true);
        KamiCommon.WindowManager.AddWindow(new CurrencyOverlay());
    }

    public void Dispose()
    {
        KamiCommon.Dispose();

        System.Dispose();
    }
}
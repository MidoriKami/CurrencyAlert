using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface;
using KamiLib.Configuration;

namespace CurrencyAlert.Models.Config;

public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 7;

    public List<TrackedCurrency> Currencies = [];
    
    public bool ChatWarning = false;
    public bool HideInDuties = false;
    
    public bool OverlayEnabled = false;
    public bool OverlayText = true;
    public bool OverlayLongText = true;
    public bool OverlayIcon = true;
    public bool ShowBackground = false;
    public bool SingleLine = false;
    public Vector4 OverlayTextColor = KnownColor.White.Vector();
    public Vector4 BackgroundColor = KnownColor.Black.Vector().Fade(0.75f);
    public Vector2 OverlayDrawPosition = new(1920.0f / 2.0f, 1024.0f / 2.0f);
    public Vector2 OverlaySize = new Vector2(600.0f, 200.0f);

    public static Configuration Load()
        => Service.PluginInterface.LoadConfigFile("CurrencyAlert.config.json", () => new Configuration());

    public void Save()
        => Service.PluginInterface.SaveConfigFile("CurrencyAlert.config.json", this);
}
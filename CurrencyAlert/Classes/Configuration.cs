using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface;
using KamiLib.Configuration;
using KamiToolKit.Nodes;

namespace CurrencyAlert.Classes;

public class Configuration : IPluginConfiguration {
    public Vector4 BackgroundColor = KnownColor.Black.Vector().Fade(0.75f);

    public bool ChatWarning = false;

    public List<TrackedCurrency> Currencies = [];
    public bool HideInDuties = false;
    public LayoutAnchor LayoutAnchor = LayoutAnchor.TopLeft;
    public Vector4 ListBackgroundColor = KnownColor.CornflowerBlue.Vector();
    public Vector2 OverlayDrawPosition = new(1920.0f / 2.0f, 1024.0f / 2.0f);
    public bool OverlayEnabled = false;
    public bool OverlayIcon = true;
    public Vector2 OverlaySize = new(600.0f, 200.0f);
    public bool OverlayText = true;
    public Vector4 OverlayTextColor = KnownColor.White.Vector();
    public bool ShowBackground = false;
    public bool ShowListBackground = false;
    public bool SingleLine = false;
    public int Version { get; set; } = 7;

    public static Configuration Load()
        => Service.PluginInterface.LoadConfigFile("CurrencyAlert.config.json", () => new Configuration());

    public void Save()
        => Service.PluginInterface.SaveConfigFile("CurrencyAlert.config.json", this);
}
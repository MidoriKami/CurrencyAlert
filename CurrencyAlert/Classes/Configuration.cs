using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface;
using KamiLib.Configuration;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.NodeStyles;

namespace CurrencyAlert.Classes;

public class Configuration : IPluginConfiguration {
    public bool ChatWarning = false;

    public List<TrackedCurrency> Currencies = [];
    public bool HideInDuties = false;

    public ListNodeStyle ListStyle = new() {
        LayoutAnchor = LayoutAnchor.TopLeft,
        BackgroundColor = KnownColor.CornflowerBlue.Vector() with { W = 0.33f },
        Size = new Vector2(600.0f, 200.0f),
        Position = new Vector2(1920.0f, 1024.0f) / 2.0f,
        ClipContents = true,
        BaseDisable = BaseStyleDisable.NodeFlags | BaseStyleDisable.Color | BaseStyleDisable.Margin,
        ListStyleDisable = ListStyleDisable.FitContents,
    };

    public CurrencyNodeStyle CurrencyNodeStyle = new();
    
    public int Version { get; set; } = 7;

    public static Configuration Load()
        => Service.PluginInterface.LoadConfigFile("CurrencyAlert.config.json", () => new Configuration());

    public void Save()
        => Service.PluginInterface.SaveConfigFile("CurrencyAlert.config.json", this);
}
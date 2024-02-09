using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface;
using Newtonsoft.Json;

namespace CurrencyAlert.Models.Config;

public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 7;

    public List<TrackedCurrency> Currencies = new();
    
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

    [JsonIgnore] public bool RepositionMode = false;
    [JsonIgnore] public bool WindowPosChanged = false;
    
    public void Save() => Service.PluginInterface.SavePluginConfig(this);
}
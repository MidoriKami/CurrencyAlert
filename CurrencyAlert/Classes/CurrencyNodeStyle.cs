using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.NodeStyles;

namespace CurrencyAlert.Classes;

public class CurrencyNodeStyle {
    public ImageNodeStyle BackgroundStyle = new() {
        Position = new Vector2(-5.0f, -5.0f),
        Size = new Vector2(610.0f, 42.0f),
        Color = KnownColor.Black.Vector() with { W = 0.30f },
        WrapMode = WrapMode.Unknown,
        BaseDisable = BaseStyleDisable.Position | BaseStyleDisable.Size | BaseStyleDisable.Scale | BaseStyleDisable.Margin | BaseStyleDisable.NodeFlags,
        ImageStyleDisable = ImageStyleDisable.WrapMode | ImageStyleDisable.ImageFlags,
    };

    public ImageNodeStyle CurrencyIconStyle = new() {
        NodeFlags = NodeFlags.Visible,
        Position = new Vector2(5.0f, 0.0f),
        Size = new Vector2(32.0f, 32.0f),
        WrapMode = WrapMode.Unknown,
        BaseDisable = BaseStyleDisable.Size | BaseStyleDisable.NodeFlags | BaseStyleDisable.Color | BaseStyleDisable.Margin,
        ImageStyleDisable = ImageStyleDisable.WrapMode | ImageStyleDisable.ImageFlags,
    };

    public TextNodeStyle WarningTextStyle = new() {
        FontSize = 24,
        FontType = FontType.Axis,
        Size = new Vector2(600.0f, 32.0f),
        Position = new Vector2(42.0f, 0.0f),
        TextColor = KnownColor.White.Vector(),
        TextFlags = TextFlags.AutoAdjustNodeSize | TextFlags.Edge,
        TextOutlineColor = KnownColor.Black.Vector(),
        BaseDisable = BaseStyleDisable.Size | BaseStyleDisable.NodeFlags | BaseStyleDisable.Color | BaseStyleDisable.Margin,
        TextStyleDisable = TextStyleDisable.AlignmentType | TextStyleDisable.TextFlags2 | TextStyleDisable.LineSpacing | TextStyleDisable.BackgroundColor,
    };

    public TextNodeStyle ItemCountTextStyle = new() {
        FontSize = 12,
        FontType = FontType.Axis,
        Size = new Vector2(32.0f, 16.0f),
        Position = new Vector2(5.0f, 16.0f),
        TextColor = KnownColor.White.Vector(),
        TextOutlineColor = KnownColor.Black.Vector(),
        TextFlags = TextFlags.Edge,
        BaseDisable = BaseStyleDisable.Size | BaseStyleDisable.NodeFlags | BaseStyleDisable.Color | BaseStyleDisable.Margin,
        TextStyleDisable = TextStyleDisable.AlignmentType | TextStyleDisable.TextFlags2 | TextStyleDisable.LineSpacing | TextStyleDisable.BackgroundColor,
    };

    public bool DrawSettings() {
        var configChanged = false;

        using var tabBar = ImRaii.TabBar("node_setting_tab_bar");
        if (tabBar) {
            using (var tabItem = ImRaii.TabItem("Background")) {
                if (tabItem) {
                    configChanged |= BackgroundStyle.DrawSettings();
                }
            }
                
            using (var tabItem = ImRaii.TabItem("Icon")) {
                if (tabItem) {
                    configChanged |= CurrencyIconStyle.DrawSettings();
                }
            }
                
            using (var tabItem = ImRaii.TabItem("Warning Text")) {
                if (tabItem) {
                    configChanged |= WarningTextStyle.DrawSettings();
                }
            }
                
            using (var tabItem = ImRaii.TabItem("Item Count Text")) {
                if (tabItem) {
                    configChanged |= ItemCountTextStyle.DrawSettings();
                }
            }
        }

        return configChanged;
    }
}
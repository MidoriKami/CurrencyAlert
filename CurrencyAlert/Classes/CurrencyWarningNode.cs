using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using Newtonsoft.Json;

namespace CurrencyAlert.Classes;

[JsonObject(MemberSerialization.OptIn)]
public class CurrencyWarningNode : NodeBase<AtkResNode> {
    [JsonProperty] private readonly BackgroundImageNode background;
    [JsonProperty] private readonly TextNode warningText;
    [JsonProperty] private readonly CurrencyIcon currencyIcon;

    public CurrencyWarningNode(uint baseId) : base(NodeType.Res) {
        NodeId = baseId;
        Margin = new Spacing(5.0f);

        background = new BackgroundImageNode {
            NodeId = 110_000 + baseId,
            Position = new Vector2(-5.0f, -5.0f),
            Size = new Vector2(610.0f, 42.0f),
            Color = KnownColor.Black.Vector() with { W = 0.30f },
            WrapMode = 1,
            IsVisible = false,
        };

        System.NativeController.AttachToNode(background, this, NodePosition.AsLastChild);

        currencyIcon = new CurrencyIcon {
            Position = new Vector2(5.0f, 0.0f),
            Size = new Vector2(32.0f, 32.0f),
            IsVisible = true,
            Margin = new Spacing { Right = 15.0f },
        };

        System.NativeController.AttachToNode(currencyIcon, this, NodePosition.AsLastChild);

        warningText = new TextNode {
            NodeId = 130_000 + baseId,
            FontSize = 24,
            FontType = FontType.Axis,
            Size = new Vector2(600.0f, 32.0f),
            Position = new Vector2(42.0f, 0.0f),
            TextColor = KnownColor.White.Vector(),
            TextFlags = TextFlags.AutoAdjustNodeSize | TextFlags.Edge,
            TextOutlineColor = KnownColor.Black.Vector(),
            IsVisible = true,
        };

        System.NativeController.AttachToNode(warningText, this, NodePosition.AsLastChild);
    }

    private TrackedCurrency? InternalCurrency { get; set; }

    public TrackedCurrency? Currency {
        get => InternalCurrency;
        set {
            if (value is null) {
                IsVisible = false;
            }
            else {
                currencyIcon.IconId = value.IconId;

                warningText.Text = value.ShowItemName ? $"{value.Name} {value.WarningText}" : $"{value.WarningText}";

                currencyIcon.ItemCount = value.CurrentCount;
            }
            
            InternalCurrency = value;
            RecalculateLayout();
            RecalculateSize();
        }
    }

    public bool ShowIcon {
        get => currencyIcon.IsVisible;
        set => currencyIcon.IsVisible = value;
    }

    public bool ShowText {
        get => warningText.IsVisible;
        set => warningText.IsVisible = value;
    }

    public int TextSize {
        get => (int) warningText.FontSize;
        set => warningText.FontSize = (uint) value;
    }

    public Vector4 TextColor {
        get => warningText.TextColor;
        set => warningText.TextColor = value;
    }

    public bool ShowItemCount {
        get => currencyIcon.ShowItemCount;
        set => currencyIcon.ShowItemCount = value;
    }

    public FontType LabelFont {
        get => warningText.FontType;
        set => warningText.FontType = value;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            background.Dispose();
            currencyIcon.Dispose();
            warningText.Dispose();

            base.Dispose(disposing);
        }
    }

    private void RecalculateLayout() {
        var currentPosition = 0.0f;

        if (currencyIcon.IsVisible) {
            currencyIcon.X = currentPosition;
            currentPosition += currencyIcon.LayoutSize.X;
        }

        if (warningText.IsVisible) {
            warningText.X = currentPosition;
            currentPosition += warningText.LayoutSize.X;
        }
        
        background.Width = currentPosition + 10.0f;
    }
    
    private void RecalculateSize() {
        var width = 0.0f;

        if (currencyIcon.IsVisible) {
            width += currencyIcon.LayoutSize.X;
        }

        if (warningText.IsVisible) {
            width += warningText.LayoutSize.X;
        }
    
        Size = new Vector2(width, 32.0f);
    }

    public override void DrawConfig() {
        base.DrawConfig();
        
        using (var backgroundNode = ImRaii.TreeNode("Background")) {
            if (backgroundNode) {
                background.DrawConfig();
            }
        }
    
        using (var currencyNode = ImRaii.TreeNode("Currency Icon")) {
            if (currencyNode) {
                currencyIcon.DrawConfig();
            }
        }
    
        using (var warningTextNode = ImRaii.TreeNode("Warning Text")) {
            if (warningTextNode) {
                warningText.DrawConfig();
            }
        }
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class CurrencyIcon : ResNode {
    [JsonProperty] private readonly IconImageNode currencyIcon;
    [JsonProperty] private readonly TextNode itemCountText;

    public CurrencyIcon() {
        currencyIcon = new IconImageNode {
            NodeId = 120_000,
            NodeFlags = NodeFlags.Visible,
            Size = new Vector2(32.0f, 32.0f),
            WrapMode = 1,
            IsVisible = true,
        };

        System.NativeController.AttachToNode(currencyIcon, this, NodePosition.AsLastChild);
        
        itemCountText = new TextNode {
            NodeId = 140_000,
            FontSize = 12,
            FontType = FontType.Axis,
            Size = new Vector2(32.0f, 16.0f),
            Position = new Vector2(0.0f, 16.0f),
            TextColor = KnownColor.White.Vector(),
            TextOutlineColor = KnownColor.Black.Vector(),
            TextFlags = TextFlags.Edge,
            IsVisible = true,
        };
        
        System.NativeController.AttachToNode(itemCountText, this, NodePosition.AsLastChild);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            itemCountText.Dispose();
            currencyIcon.Dispose();
            
            base.Dispose(disposing);
        }
    }

    public int ItemCount {
        set {
            if (itemCountText.IsVisible) {
                itemCountText.SetNumber(value);
            }
        }
    }

    public uint IconId {
        set => currencyIcon.IconId = value;
    }

    public bool ShowItemCount {
        get => itemCountText.IsVisible;
        set => itemCountText.IsVisible = value;
    }

    public override void DrawConfig() {
        base.DrawConfig();
    
        using (var currencyNode = ImRaii.TreeNode("Currency Icon")) {
            if (currencyNode) {
                currencyIcon.DrawConfig();
            }
        }
        
        using (var countNode = ImRaii.TreeNode("Currency Count")) {
            if (countNode) {
                itemCountText.DrawConfig();
            }
        }
    }

    public override float Width 
        => Margin.Left + Math.Max(currencyIcon.Width, itemCountText.Width) + Margin.Right;

    public override float Height 
        => Margin.Top + Math.Max(currencyIcon.Height, itemCountText.Height) + Margin.Bottom;
}
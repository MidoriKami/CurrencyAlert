using System.Drawing;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace CurrencyAlert.Classes;

public class CurrencyWarningNode : NodeBase<AtkResNode> {
    private readonly ImageNode background;
    private readonly ImageNode currencyIcon;
    private readonly TextNode warningText;
    
    public CurrencyWarningNode(uint baseId) : base(NodeType.Res) {
        NodeID = baseId;

        background = new ImageNode {
            NodeID = 110_000 + baseId,
            NodeFlags = NodeFlags.Visible,
            Size = new Vector2(600.0f, 32.0f),
            Color = KnownColor.Black.Vector() with { W = 0.30f },
        };
        
        background.AttachNode(this, NodePosition.AsFirstChild);

        currencyIcon = new ImageNode {
            NodeID = 120_000 + baseId,
            Position = new Vector2(5.0f, 0.0f),
            NodeFlags = NodeFlags.Visible,
            Size = new Vector2(32.0f, 32.0f),
            Margin = new Spacing(5.0f),
        };
        
        currencyIcon.AttachNode(this, NodePosition.AsLastChild);

        warningText = new TextNode {
            NodeID = 130_000 + baseId,
            FontSize = 24,
            FontType = FontType.Axis,
            Size = new Vector2(600.0f, 32.0f),
            Margin = new Spacing(5.0f),
            Position = new Vector2(currencyIcon.LayoutSize.X + Margin.Left, 0.0f),
            TextColor = KnownColor.White.Vector(),
            TextFlags = TextFlags.AutoAdjustNodeSize
        };
        
        warningText.AttachNode(this, NodePosition.AsLastChild);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            base.Dispose(disposing);
            
            background.Dispose();
            currencyIcon.Dispose();
            warningText.Dispose();
        }
    }

    public Vector4 BackgroundColor {
        get => background.Color;
        set => background.Color = value;
    }

    public SeString WarningText {
        get => warningText.Text;
        set => warningText.Text = value;
    }

    private uint InternalIconId { get; set; }

    public required uint IconId {
        get => InternalIconId;
        set {
            InternalIconId = value;
            currencyIcon.LoadIcon(value);
        }
    }

    public bool ShowText {
        get => warningText.IsVisible;
        set => warningText.IsVisible = value;
    }

    public bool ShowIcon {
        get => currencyIcon.IsVisible;
        set => currencyIcon.IsVisible = value;
    }

    public bool ShowBackground {
        get => background.IsVisible;
        set => background.IsVisible = value;
    }

    public Vector4 TextColor {
        get => warningText.TextColor;
        set => warningText.TextColor = value;
    }

    public void UpdateLayout() {
        if (!ShowIcon) warningText.Position = new Vector2(Margin.Left, 0.0f);
        if (ShowIcon) warningText.Position = new Vector2(currencyIcon.LayoutSize.X + Margin.Left, 0.0f);

        var width = 0.0f;

        if (ShowIcon) width += currencyIcon.LayoutSize.X;
        if (ShowText) width += warningText.LayoutSize.X;

        Width = width;
        Height = 32.0f;

        background.Width = width;
    }
}
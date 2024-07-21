using System.Drawing;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace CurrencyAlert.Classes;

public class CurrencyWarningNode : NodeBase<AtkResNode> {
    private readonly BackgroundImageNode background;
    private readonly ImageNode currencyIcon;
    private readonly TextNode warningText;
    private readonly TextNode itemCountText;

    public CurrencyWarningNode(uint baseId) : base(NodeType.Res) {
        NodeID = baseId;

        Margin = new Spacing(5.0f);

        background = new BackgroundImageNode {
            NodeID = 110_000 + baseId,
            NodeFlags = NodeFlags.Visible,
            Position = new Vector2(-5.0f, -5.0f),
            Size = new Vector2(600.0f, 32.0f) + new Vector2(10.0f, 10.0f),
            Color = KnownColor.Black.Vector() with { W = 0.30f, },
        };

        System.NativeController.AttachToNode(background, this, NodePosition.AsLastChild);

        currencyIcon = new ImageNode {
            NodeID = 120_000 + baseId,
            Position = new Vector2(5.0f, 0.0f),
            NodeFlags = NodeFlags.Visible,
            Size = new Vector2(32.0f, 32.0f),
            Margin = new Spacing(5.0f),
        };

        System.NativeController.AttachToNode(currencyIcon, this, NodePosition.AsLastChild);

        warningText = new TextNode {
            NodeID = 130_000 + baseId,
            FontSize = 24,
            FontType = FontType.Axis,
            Size = new Vector2(600.0f, 32.0f),
            Margin = new Spacing(5.0f),
            Position = new Vector2(currencyIcon.LayoutSize.X + Margin.Left + 5.0f, 0.0f),
            TextColor = KnownColor.White.Vector(),
            TextFlags = TextFlags.AutoAdjustNodeSize | TextFlags.Edge,
            TextOutlineColor = KnownColor.Black.Vector(),
        };

        System.NativeController.AttachToNode(warningText, this, NodePosition.AsLastChild);

        itemCountText = new TextNode {
            NodeID = 140_000 + baseId,
            FontSize = 12,
            FontType = FontType.Axis,
            Size = new Vector2(32.0f, 16.0f),
            Position = new Vector2(0.0f + currencyIcon.Margin.Left, 16.0f),
            TextColor = KnownColor.White.Vector(),
            TextOutlineColor = KnownColor.Black.Vector(),
            TextFlags = TextFlags.Edge,
        };
        
        System.NativeController.AttachToNode(itemCountText, this, NodePosition.AsLastChild);
    }

    public TrackedCurrency? Currency { get; set; }

    public Vector4 BackgroundColor {
        get => background.Color;
        set => background.Color = value;
    }

    public SeString WarningText {
        get => warningText.Text;
        set => warningText.Text = value;
    }

    private uint InternalIconId { get; set; }

    public uint IconId {
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

    public bool ShowItemCount {
        get => itemCountText.IsVisible;
        set => itemCountText.IsVisible = value;
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;

        System.NativeController.DetachFromNode(background);
        background.Dispose();

        System.NativeController.DetachFromNode(currencyIcon);
        currencyIcon.Dispose();

        System.NativeController.DetachFromNode(warningText);
        warningText.Dispose();

        base.Dispose(disposing);
    }

    public void Refresh() {
        if (Currency is null) return;

        if (!ShowIcon) warningText.Position = new Vector2(Margin.Left + 5.0f, 0.0f);
        if (ShowIcon) warningText.Position = new Vector2(currencyIcon.LayoutSize.X + Margin.Left + 5.0f, 0.0f);

        var width = 0.0f;

        if (ShowIcon) width += currencyIcon.LayoutSize.X;
        if (ShowText) width += warningText.LayoutSize.X;

        Width = width;
        Height = 32.0f;

        background.Width = width + 10.0f;

        IconId = Currency.IconId;
        WarningText = Currency.ShowItemName ? $"{Currency.Name} {Currency.OverlayWarningText}" : $"{Currency.OverlayWarningText}";

        if (System.Config.OverlayItemCount) {
            itemCountText.SetNumber(Currency.CurrentCount);
        }
    }
}
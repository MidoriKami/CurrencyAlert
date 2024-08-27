using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace CurrencyAlert.Classes;

public class CurrencyWarningNode : NodeBase<AtkResNode> {
    private readonly BackgroundImageNode background;
    private readonly IconImageNode currencyIcon;
    private readonly TextNode warningText;
    private readonly TextNode itemCountText;

    public CurrencyWarningNode(uint baseId) : base(NodeType.Res) {
        NodeID = baseId;
        Margin = new Spacing(5.0f);

        background = new BackgroundImageNode {
            NodeID = 110_000 + baseId,
        };

        System.NativeController.AttachToNode(background, this, NodePosition.AsLastChild);

        currencyIcon = new IconImageNode {
            NodeID = 120_000 + baseId,
        };

        System.NativeController.AttachToNode(currencyIcon, this, NodePosition.AsLastChild);

        warningText = new TextNode {
            NodeID = 130_000 + baseId,
        };

        System.NativeController.AttachToNode(warningText, this, NodePosition.AsLastChild);

        itemCountText = new TextNode {
            NodeID = 140_000 + baseId,
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
            currencyIcon.IconId = value;
        }
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
    
        background.SetStyle(System.Config.CurrencyNodeStyle.BackgroundStyle);
        currencyIcon.SetStyle(System.Config.CurrencyNodeStyle.CurrencyIconStyle);
        warningText.SetStyle(System.Config.CurrencyNodeStyle.WarningTextStyle);
        itemCountText.SetStyle(System.Config.CurrencyNodeStyle.ItemCountTextStyle);

        NodeFlags |= NodeFlags.EmitsEvents | NodeFlags.HasCollision | NodeFlags.RespondToMouse;
        
        IconId = Currency.IconId;
        WarningText = Currency.ShowItemName ? $"{Currency.Name} {Currency.OverlayWarningText}" : $"{Currency.OverlayWarningText}";

        var width = Margin.Left + Margin.Right;

        if (currencyIcon.IsVisible) {
            width += currencyIcon.LayoutSize.X;
        }

        if (warningText.IsVisible) {
            width += warningText.LayoutSize.X;
        }

        Width = width;
        Height = 32.0f;
        
        background.Width = width + 10.0f;

        if (System.Config.CurrencyNodeStyle.ItemCountTextStyle.IsVisible) {
            itemCountText.SetNumber(Currency.CurrentCount);
        }
    }
}
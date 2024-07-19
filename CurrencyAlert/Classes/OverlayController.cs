using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiLib.Extensions;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace CurrencyAlert.Classes;

public unsafe class OverlayController() : NativeUiOverlayController(Service.AddonLifecycle, Service.Framework, Service.GameGui) {
    private ListNode<CurrencyWarningNode>? overlayListNode;

    private static AddonNamePlate* AddonNamePlate => (AddonNamePlate*) Service.GameGui.GetAddonByName("NamePlate");

    protected override void AttachNodes(AddonNamePlate* addonNamePlate) {
        overlayListNode = new ListNode<CurrencyWarningNode> {
            Size = System.Config.OverlaySize,
            Position = System.Config.OverlayDrawPosition,
            LayoutAnchor = System.Config.LayoutAnchor,
            IsVisible = System.Config.OverlayEnabled,
            NodeFlags = NodeFlags.Clip,
            LayoutOrientation = System.Config.SingleLine ? LayoutOrientation.Horizontal : LayoutOrientation.Vertical,
            NodeID = 100_000,
            Color = KnownColor.White.Vector(),
            BackgroundVisible = System.Config.ShowListBackground,
            BackgroundColor = System.Config.ListBackgroundColor,
        };

        foreach (uint index in Enumerable.Range(0, 10)) {
            var newOverlayNode = new CurrencyWarningNode(100_000 + index) {
                TextColor = System.Config.OverlayTextColor,
                ShowIcon = System.Config.OverlayIcon,
                ShowBackground = System.Config.ShowBackground,
                BackgroundColor = System.Config.BackgroundColor,
                ShowText = System.Config.OverlayText,
                Height = 32.0f,
                MouseClick = () => System.ConfigurationWindow.UnCollapseOrToggle(),
                Tooltip = "Overlay from CurrencyAlert plugin",
            };

            newOverlayNode.EnableEvents(Service.AddonEventManager, (AtkUnitBase*) AddonNamePlate);
            overlayListNode.Add(newOverlayNode);
        }

        RefreshAddon();
        UpdateSettings();

        System.NativeController.AttachToAddon(overlayListNode, (AtkUnitBase*) addonNamePlate, addonNamePlate->RootNode, NodePosition.AsFirstChild);
    }

    protected override void DetachNodes(AddonNamePlate* addonNamePlate) {
        if (overlayListNode is null) return;

        System.NativeController.DetachFromAddon(overlayListNode, (AtkUnitBase*) addonNamePlate);
        overlayListNode.Dispose();
        overlayListNode = null;
    }

    protected override void LoadConfig() {
        // Nothing to load
    }

    public void Update() {
        if (overlayListNode is null) return;

        overlayListNode.IsVisible = System.Config.HideInDuties switch {
            true when Service.Condition.IsBoundByDuty() => false,
            true when !Service.Condition.IsBoundByDuty() => true,
            _ => System.Config.OverlayEnabled,
        };

        foreach (var bannerOverlayNode in overlayListNode) {
            bannerOverlayNode.IsVisible = false;
        }

        var activeWarnings = System.Config.Currencies
            .Where(currency => currency is { HasWarning: true, Enabled: true, ShowInOverlay: true })
            .ToList();

        foreach (var index in Enumerable.Range(0, 10)) {
            var overlayNode = overlayListNode[index];
            if (index > overlayListNode.Count || index >= activeWarnings.Count) continue;

            overlayNode.Currency = activeWarnings[index];
            overlayNode.IsVisible = true;
            overlayNode.Refresh();
            overlayListNode.RecalculateLayout();
        }
    }

    public void UpdateSettings() {
        if (overlayListNode is null) return;

        overlayListNode.IsVisible = System.Config.OverlayEnabled;
        overlayListNode.Position = System.Config.OverlayDrawPosition;
        overlayListNode.Size = System.Config.OverlaySize;
        overlayListNode.LayoutOrientation = System.Config.SingleLine ? LayoutOrientation.Horizontal : LayoutOrientation.Vertical;
        overlayListNode.LayoutAnchor = System.Config.LayoutAnchor;
        overlayListNode.BackgroundVisible = System.Config.ShowListBackground;
        overlayListNode.BackgroundColor = System.Config.ListBackgroundColor;
        overlayListNode.Scale = new Vector2(System.Config.OverlayScale);

        foreach (var node in overlayListNode) {
            node.ShowText = System.Config.OverlayText;
            node.ShowIcon = System.Config.OverlayIcon;
            node.ShowBackground = System.Config.ShowBackground;
            node.BackgroundColor = System.Config.BackgroundColor;
            node.TextColor = System.Config.OverlayTextColor;

            node.Refresh();
        }

        overlayListNode.RecalculateLayout();
    }
}
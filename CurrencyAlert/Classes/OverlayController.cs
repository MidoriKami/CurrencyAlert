using System.Drawing;
using System.Linq;
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
        
        System.NativeController.AttachToAddon(overlayListNode, (AtkUnitBase*)addonNamePlate, addonNamePlate->RootNode, NodePosition.AsFirstChild);
        UpdateSettings();
    }

    protected override void DetachNodes(AddonNamePlate* addonNamePlate) {
        if (overlayListNode is null) return;
        
        System.NativeController.DetachFromAddon(overlayListNode, (AtkUnitBase*)addonNamePlate);
        overlayListNode.Dispose();
    }

    protected override void LoadConfig() {
        // Nothing to load
    }

    public void Update() {
        if (overlayListNode is null) return;
        if (AddonNamePlate is null) return;
        
        overlayListNode.IsVisible = System.Config.HideInDuties switch {
            true when Service.Condition.IsBoundByDuty() => false,
            true when !Service.Condition.IsBoundByDuty() => true,
            _ => System.Config.OverlayEnabled,
        };

        foreach (var warning in System.Config.Currencies.Where(currency => currency is { HasWarning: true, Enabled: true, ShowInOverlay: true })) {
            if (overlayListNode.Any(warningNode => warningNode.IconId == warning.IconId)) continue;
            
            var newWarningNode = new CurrencyWarningNode(100_000 + warning.IconId) {
                TextColor = System.Config.OverlayTextColor,
                ShowIcon = System.Config.OverlayIcon,
                ShowBackground = System.Config.ShowBackground,
                BackgroundColor = System.Config.BackgroundColor,
                ShowText = System.Config.OverlayText,
                Height = 32.0f,
                IconId = warning.IconId,
                NodeFlags = NodeFlags.Visible,
                WarningText = warning.ShowItemName ? $"{warning.Name} {warning.OverlayWarningText}" : $"{warning.OverlayWarningText}",
                MouseClick = () => System.ConfigurationWindow.UnCollapseOrToggle(),
            };

            newWarningNode.EnableEvents(Service.AddonEventManager, (AtkUnitBase*)AddonNamePlate);
            newWarningNode.UpdateLayout();
            overlayListNode.Add(newWarningNode);
            warning.WarningNode = newWarningNode;

            RefreshAddon();
            UpdateSettings();
        }

        foreach (var warning in System.Config.Currencies.Where(currency => currency is { HasWarning: false } or { Enabled: false } or { ShowInOverlay: false })) {
            if (overlayListNode.FirstOrDefault(warningNode => warningNode.IconId == warning.IconId) is not { } node) continue;
            
            overlayListNode.Remove(node);
            warning.WarningNode = null;
            
            RefreshAddon();
            UpdateSettings();
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

        foreach (var node in overlayListNode.OfType<CurrencyWarningNode>()) {
            node.ShowText = System.Config.OverlayText;
            node.ShowIcon = System.Config.OverlayIcon;
            node.ShowBackground = System.Config.ShowBackground;
            node.BackgroundColor = System.Config.BackgroundColor;
            node.TextColor = System.Config.OverlayTextColor;
            
            node.UpdateLayout();
        }
        
        overlayListNode.RecalculateLayout();
    }
}
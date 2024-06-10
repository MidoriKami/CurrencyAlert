using System;
using System.Drawing;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiLib.Extensions;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace CurrencyAlert.Classes;

public unsafe class OverlayController : IDisposable {
    private readonly ListNode<CurrencyWarningNode> overlayListNode;
    
    public OverlayController() {
        overlayListNode = new ListNode<CurrencyWarningNode> {
            Size = System.Config.OverlaySize,
            Position = System.Config.OverlayDrawPosition,
            LayoutAnchor = System.Config.LayoutAnchor,
            IsVisible = System.Config.OverlayEnabled,
            NodeFlags = NodeFlags.Clip,
            LayoutOrientation = System.Config.SingleLine ? LayoutOrientation.Horizontal : LayoutOrientation.Vertical,
            NodeID = 100_000,
            Tooltip = "Overlay from CurrencyAlert Plugin",
            OnClick = () => System.ConfigurationWindow.UnCollapseOrToggle(),
            Color = KnownColor.White.Vector(),
            BackgroundVisible = System.Config.ShowListBackground,
            BackgroundColor = System.Config.ListBackgroundColor,
        };
        
        // If the NamePlate addon doesn't exist yet, wait for it.
        var addon = (AtkUnitBase*) Service.GameGui.GetAddonByName("NamePlate");
        if (addon is null) {
            Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "NamePlate", AttachListNode);
        }
        
        // else it does exist, 
        else {
            AttachToAddon(addon);
        }

        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose() {
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.AddonLifecycle.UnregisterListener(AttachListNode);

        overlayListNode.Dispose();
    }
    
    private void OnFrameworkUpdate(IFramework framework) {
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
            };
                
            newWarningNode.UpdateLayout();
            overlayListNode.Add(newWarningNode);

            warning.WarningNode = newWarningNode;
        }

        foreach (var warning in System.Config.Currencies.Where(currency => currency is { HasWarning: false } or { Enabled: false } or { ShowInOverlay: false })) {
            if (overlayListNode.FirstOrDefault(warningNode => warningNode.IconId == warning.IconId) is not { } node) continue;
            
            overlayListNode.Remove(node);
            warning.WarningNode = null;
        }
    }
    
    private void AttachListNode(AddonEvent type, AddonArgs args) {
        var addon = (AtkUnitBase*) args.Addon; // Note, args.Addon is guaranteed to never be null.
        if (addon->RootNode is null) return; // addon->RootNode is not guaranteed to never be null.
        
        AttachToAddon(addon);
    }

    private void AttachToAddon(AtkUnitBase* addon) {
        overlayListNode.AttachNode(addon, addon->RootNode, NodePosition.AsFirstChild);
        overlayListNode.EnableTooltip(Service.AddonEventManager, addon);
        overlayListNode.EnableOnClick(Service.AddonEventManager, addon);
        
        UpdateSettings();
    }

    public void UpdateSettings() {
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
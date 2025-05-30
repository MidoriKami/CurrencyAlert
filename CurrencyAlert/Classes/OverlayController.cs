using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Events;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiLib.Extensions;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.System;

namespace CurrencyAlert.Classes;

public unsafe class OverlayController : NameplateAddonController {
    public ListNode<CurrencyWarningNode>? OverlayListNode { get; private set; }

    internal static string ListNodeConfigPath 
        => Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "ListNode.style.json");

    internal static string CurrencyNodeConfigPath 
        => Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "CurrencyNode.style.json");

    private int lastFrameWarningCount;
    
    public OverlayController() : base(Service.PluginInterface) {
        OnAttach += AttachNodes;
        OnDetach += DetachNodes;
    }

    public override void Dispose() {
        OnAttach -= AttachNodes;
        OnDetach -= DetachNodes;
        
        base.Dispose();
    }

    private void AttachNodes(AddonNamePlate* addonNamePlate) {
        OverlayListNode = new ListNode<CurrencyWarningNode> {
            NodeId = 100_000,
            LayoutAnchor = LayoutAnchor.TopLeft,
            BackgroundColor = KnownColor.CornflowerBlue.Vector() with { W = 0.33f },
            Size = new Vector2(600.0f, 200.0f),
            Position = new Vector2(1920.0f, 1024.0f) / 2.0f,
            ClipListContents = true,
            IsVisible = true,
            ItemMargin = new Spacing(10.0f),
        };
        
        OverlayListNode.Load(ListNodeConfigPath);

        foreach (uint index in Enumerable.Range(0, 10)) {
            var newOverlayNode = new CurrencyWarningNode(100_000 + index) {
                Height = 32.0f,
                Tooltip = "Overlay from CurrencyAlert plugin",
                EnableEventFlags = true,
            };
            
            newOverlayNode.Load(CurrencyNodeConfigPath);
            
            newOverlayNode.AddEvent(AddonEventType.MouseClick, System.ConfigurationWindow.UnCollapseOrToggle);
            newOverlayNode.EnableEvents(Service.AddonEventManager, (AtkUnitBase*) addonNamePlate);
            OverlayListNode.Add(newOverlayNode);
        }

        System.NativeController.AttachToAddon(OverlayListNode, (AtkUnitBase*) addonNamePlate, addonNamePlate->RootNode, NodePosition.AsFirstChild);

        OverlayListNode.RecalculateLayout();
    }

    private void DetachNodes(AddonNamePlate* addonNamePlate) {
        if (OverlayListNode is null) return;

        System.NativeController.DetachFromAddon(OverlayListNode, (AtkUnitBase*) addonNamePlate);
        OverlayListNode.Dispose();
        OverlayListNode = null;
    }

    public void Update() {
        if (OverlayListNode is null) return;

        OverlayListNode.IsVisible = System.Config.HideInDuties switch {
            true when Service.Condition.IsBoundByDuty() => false,
            true when !Service.Condition.IsBoundByDuty() => true,
            _ => OverlayListNode.IsVisible,
        };

        foreach (var bannerOverlayNode in OverlayListNode) {
            bannerOverlayNode.IsVisible = false;
        }

        var activeWarnings = System.Config.Currencies
            .Where(currency => currency is { HasWarning: true, Enabled: true, ShowInOverlay: true })
            .ToList();

        var currentWarningCount = activeWarnings.Count;
        
        foreach (var index in Enumerable.Range(0, 10)) {
            var overlayNode = OverlayListNode[index];
            if (index > OverlayListNode.Count || index >= activeWarnings.Count) continue;

            overlayNode.Currency = activeWarnings[index];
            overlayNode.IsVisible = true;
        }

        if (lastFrameWarningCount != currentWarningCount) {
            OverlayListNode.RecalculateLayout();
            lastFrameWarningCount = currentWarningCount;
        }
    }
}
using System.Linq;
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
            NodeID = 100_000,
        };

        foreach (uint index in Enumerable.Range(0, 10)) {
            var newOverlayNode = new CurrencyWarningNode(100_000 + index) {
                Height = 32.0f,
                MouseClick = () => System.ConfigurationWindow.UnCollapseOrToggle(),
                Tooltip = "Overlay from CurrencyAlert plugin",
            };

            newOverlayNode.EnableEvents(Service.AddonEventManager, (AtkUnitBase*) AddonNamePlate);
            overlayListNode.Add(newOverlayNode);
        }

        Refresh();

        System.NativeController.AttachToAddon(overlayListNode, (AtkUnitBase*) addonNamePlate, addonNamePlate->RootNode, NodePosition.AsFirstChild);
    }

    protected override void DetachNodes(AddonNamePlate* addonNamePlate) {
        if (overlayListNode is null) return;

        System.NativeController.DetachFromAddon(overlayListNode, (AtkUnitBase*) addonNamePlate);
        overlayListNode.Dispose();
        overlayListNode = null;
    }

    protected override void PreAttach() {
        // Nothing to load
    }

    public void Update() {
        if (overlayListNode is null) return;

        if (System.Config.HideInDuties && Service.Condition.IsBoundByDuty()) {
            overlayListNode.IsVisible = false;
        }

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

    public void Refresh() {
        if (overlayListNode is null) return;

        overlayListNode.SetStyle(System.Config.ListStyle);

        foreach (var node in overlayListNode) {
            node.Refresh();
        }

        overlayListNode.RecalculateLayout();
    }
}
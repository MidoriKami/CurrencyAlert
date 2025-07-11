using System.IO;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Extensions;
using KamiToolKit;
using KamiToolKit.Classes;

namespace CurrencyAlert.Classes;

public unsafe class OverlayController : NameplateAddonController {

    public OverlayListNode? OverlayListNode;

    private static string ListNodeConfigPath => Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "ListNode.style.json");
    
    public OverlayController() : base(Service.PluginInterface) {
        OnAttach += AttachNodes;
        OnDetach += DetachNodes;

        Enable();
    }

    public override void Dispose() {
        OnAttach -= AttachNodes;
        OnDetach -= DetachNodes;
        
        base.Dispose();
    }

    private void AttachNodes(AddonNamePlate* addonNamePlate) {
        OverlayListNode = new OverlayListNode {
            NodeId = 100000,
            Size = new Vector2(600.0f, 200.0f),
            Position = new Vector2(1920.0f, 1024.0f) / 2.0f,
            IsVisible = true,
        };
        
        OverlayListNode.Load(ListNodeConfigPath);
        System.NativeController.AttachNode(OverlayListNode, addonNamePlate->RootNode, NodePosition.AsFirstChild);
    }

    private void DetachNodes(AddonNamePlate* addonNamePlate) {
        if (OverlayListNode is null) return;

        System.NativeController.DetachNode(OverlayListNode, () => {
            OverlayListNode.Dispose();
            OverlayListNode = null;
        });
    }

    public void Update() {
        if (OverlayListNode is null) return;

        OverlayListNode.IsVisible = System.Config.HideInDuties switch {
            true when Service.Condition.IsBoundByDuty() => false,
            true when !Service.Condition.IsBoundByDuty() => true,
            _ => OverlayListNode.IsVisible,
        };

        var activeWarnings = System.Config.Currencies
            .Where(currency => currency is { HasWarning: true, Enabled: true, ShowInOverlay: true })
            .ToList();
        
        OverlayListNode.UpdateWarnings(activeWarnings);
    }

    public void Save() {
        OverlayListNode?.Save(ListNodeConfigPath);
        OverlayListNode?.SaveCurrencyWarningNode();
    }
}
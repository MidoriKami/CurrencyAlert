using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Events;
using Dalamud.Interface;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using Newtonsoft.Json;

namespace CurrencyAlert.Classes;

[JsonObject(MemberSerialization.OptIn)]
public class OverlayListNode : SimpleComponentNode {

	[JsonProperty] private readonly BackgroundImageNode backgroundImageNode;
	[JsonProperty] private readonly NineGridNode borderNode;
	[JsonProperty] private LayoutListNode<CurrencyWarningNode> listNode;

	public readonly List<CurrencyWarningNode> NodeList = [];

	internal static string CurrencyNodeConfigPath => Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "CurrencyNode.style.json");

	public OverlayListNode() {
		backgroundImageNode = new BackgroundImageNode {
			NodeId = 2,
			Color = KnownColor.CornflowerBlue.Vector() with { W = 0.33f },
			IsVisible = true,
		};
		System.NativeController.AttachNode(backgroundImageNode, this);

		borderNode = new BorderNineGridNode {
			NodeId = 3,
			IsVisible = true,
		};
		System.NativeController.AttachNode(borderNode, this);

		listNode = new VerticalListNode<CurrencyWarningNode>();
		
		RebuildListNode();
	}

	public override float Width {
		get => base.Width;
		set {
			base.Width = value;
			backgroundImageNode.Width = value;
			borderNode.Width = value + 30.0f;
			borderNode.X = -15.0f;
			listNode.Width = value;
		}
	}

	public override float Height {
		get => base.Height;
		set {
			base.Height = value;
			backgroundImageNode.Height = value;
			borderNode.Height = value + 30.0f;
			borderNode.Y = -15.0f;
			listNode.Height = value;
		}
	}

	public Vector4 BackgroundColor {
		get => backgroundImageNode.Color;
		set => backgroundImageNode.Color = value;
	}

	public LayoutMode LayoutOrientation {
		get;
		set {
			field = value;
			RebuildListNode();
		}
	}

	public float ItemSpacing {
		get => listNode.ItemSpacing;
		set => listNode.ItemSpacing = value;
	}

	public bool ShowBackground {
		get => backgroundImageNode.IsVisible;
		set => backgroundImageNode.IsVisible = value;
	}

	public bool ShowBorder {
		get => borderNode.IsVisible;
		set => borderNode.IsVisible = value;
	}

	public void UpdateWarnings(List<TrackedCurrency> activeWarnings) {
		// Get a list of warnings that we need to remove
		var oldWarnings = NodeList.Where(node => activeWarnings.All(warning => warning != node.Currency)).ToList();

		foreach (var oldWarning in oldWarnings) {
			listNode.RemoveNode(oldWarning);
			NodeList.Remove(oldWarning);
			oldWarning.Dispose();
		}
		
		// Get a list of warnings that we need to add
		var newWarnings = activeWarnings.Where(warning => NodeList.All(node => node.Currency != warning)).ToList();

		foreach (var newWarning in newWarnings) {
			var newWarningNode = new CurrencyWarningNode {
				Height = 32.0f,
				Currency = newWarning,
				IsVisible = true,
				EnableEventFlags = true,
				Tooltip = "Overlay from CurrencyAlert plugin",
			};
			
			newWarningNode.Load(CurrencyNodeConfigPath);
			newWarningNode.AddEvent(AddonEventType.MouseClick, OpenConfigurationWindow);
			
			listNode.AddNode(newWarningNode);
			NodeList.Add(newWarningNode);
		}
		
		foreach (var node in NodeList) {
			node.UpdateFromCurrency();
			node.RecalculateLayout();
		}

		listNode.EventFlagsSet = NodeList.Count is 0;
		listNode.RecalculateLayout();
	}

	private void RebuildListNode() {
		System.NativeController.DetachNode(listNode, () => {
			listNode.Dispose();
		});
		NodeList.Clear();
		
		if (LayoutOrientation is LayoutMode.Vertical) {
			listNode = new VerticalListNode<CurrencyWarningNode> {
				NodeId = 4,
				Alignment = VerticalListAnchor.Top,
			};
		}
		else {
			listNode = new HorizontalListNode<CurrencyWarningNode> {
				NodeId = 4,
				Alignment = HorizontalListAnchor.Left,
			};
		}

		listNode.IsVisible = true;
		listNode.FirstItemSpacing = 10.0f;
		listNode.ItemSpacing = 10.0f;
		listNode.ClipListContents = true;
		listNode.Tooltip = "Overlay from CurrencyAlert plugin";
		listNode.Size = Size;
		
		listNode.AddEvent(AddonEventType.MouseClick, OpenConfigurationWindow);
		System.NativeController.AttachNode(listNode, this);
	}

	private void OpenConfigurationWindow(AddonEventData _)
		=> System.ConfigurationWindow.UnCollapseOrToggle();

	public void SaveCurrencyWarningNode()
		=> NodeList.FirstOrDefault()?.Save(CurrencyNodeConfigPath);
}
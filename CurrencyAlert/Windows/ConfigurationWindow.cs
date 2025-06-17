using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using CurrencyAlert.Classes;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.CommandManager;
using KamiLib.Window;
using KamiLib.Window.SelectionWindows;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using Lumina.Excel.Sheets;

namespace CurrencyAlert.Windows;

public class ConfigurationWindow : TabbedSelectionWindow<TrackedCurrency> {

    public ConfigurationWindow() : base("CurrencyAlert Configuration Window", new Vector2(450.0f, 400.0f)) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => Toggle(), ActivationPath = "/",
        });
    }

    protected override List<TrackedCurrency> Options => System.Config.Currencies;
    
    protected override float SelectionListWidth { get; set; } = 150.0f;
    
    protected override float SelectionItemHeight => 20.0f;
    
    protected override bool AllowChildScroll => false;
    
    protected override string SelectionListTabName => "Tracked Currencies";
    
    protected override bool ShowListButton => true;

    protected override List<ITabItem> Tabs { get; } = [
        new GeneralSettingsTab(),
        new ListNodeSettingsTab(),
        new CurrencyNodeSettingsTab(),
    ];

    protected override void DrawListOption(TrackedCurrency option) {
        // If ID is zero, and type is LimitedTomestone, then the limited tomestone doesn't exist.
        // This only happens between expansion release and the release of savage, so this won't be relevant again for 2-3 years.
        if (option is { ItemId: 0, Type: CurrencyType.LimitedTomestone }) {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3.0f * ImGuiHelpers.GlobalScale);
            ImGui.Image(Service.TextureProvider.GetFromGameIcon(60071).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(24.0f));

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Limited Tomestone (Currently Unavailable)");

            return;
        }

        if (option is { Name: var name, Icon: { } icon }) {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3.0f * ImGuiHelpers.GlobalScale);
            ImGui.Image(icon.ImGuiHandle, ImGuiHelpers.ScaledVector2(24.0f));

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(name);
        }
        else {
            ImGui.TextColored(KnownColor.OrangeRed.Vector(), $"Error, unable to display currency. ItemId: {option.ItemId}");
        }
    }

    protected override void DrawSelectedOption(TrackedCurrency selectedOption) {
        DrawHeaderAndWatermark(selectedOption);
        DrawCurrentStatus(selectedOption);
        DrawSettings(selectedOption);
    }

    private void DrawHeaderAndWatermark(TrackedCurrency currency) {
        var region = ImGui.GetContentRegionAvail();
        var minDimension = Math.Min(region.X, region.Y);

        // If ID is zero, and type is LimitedTomestone, then the limited tomestone doesn't exist.
        // This only happens between expansion release and the release of savage, so this won't be relevant again for 2-3 years.
        if (currency is { ItemId: 0, Type: CurrencyType.LimitedTomestone }) {
            const string text = "Limited Tomestone (Currently Unavailable)";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.Text(text);
            ImGui.Separator();

            var areaStart = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(region.X / 2.0f - minDimension / 2.0f);
            ImGui.Image(Service.TextureProvider.GetFromGameIcon(60071).GetWrapOrEmpty().ImGuiHandle, new Vector2(minDimension), Vector2.Zero, Vector2.One, Vector4.One with {
                W = 0.10f,
            });
            ImGui.SetCursorPos(areaStart);

            return;
        }

        if (currency is { Name: var name }) {
            var textSize = ImGui.CalcTextSize(name);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.Text(name);
            ImGui.Separator();

            var areaStart = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(region.X / 2.0f - minDimension / 2.0f);
            ImGui.Image(currency.Icon.ImGuiHandle, new Vector2(minDimension), Vector2.Zero, Vector2.One, Vector4.One with {
                W = 0.10f,
            });
            ImGui.SetCursorPos(areaStart);
        }
    }

    private void DrawCurrentStatus(TrackedCurrency currency) {
        if (currency is not { CurrentCount: var currentCount, Threshold: var threshold }) return;

        var color = ((float) currentCount / threshold) switch {
            < 0.75f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.White.Vector(),
            < 0.85f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.Orange.Vector(),
            < 0.95f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.OrangeRed.Vector(),
            > 0.95f and < 1.00f => KnownColor.Red.Vector(),
            >= 1.00f and < 1.05f => currency.Invert ? KnownColor.OrangeRed.Vector() : KnownColor.Red.Vector(),
            >= 1.05f and < 1.15f => currency.Invert ? KnownColor.Orange.Vector() : KnownColor.Red.Vector(),
            >= 1.15f => currency.Invert ? KnownColor.White.Vector() : KnownColor.Red.Vector(),
            _ => KnownColor.White.Vector(),
        };

        DrawCurrentStatusTable(currentCount, color, threshold);

        ImGuiHelpers.ScaledDummy(5.0f);
    }

    private static void DrawCurrentStatusTable(int currentCount, Vector4 color, int threshold) {
        using var table = ImRaii.Table("CurrentStatusTable", 3);
        if (!table) return;
        
        ImGui.TableSetupColumn("##CurrentAmount", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##Slash", ImGuiTableColumnFlags.WidthFixed, 5.0f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("##ThresholdAmount", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        var text = $"{currentCount}";
        var currentCountSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - currentCountSize.X);
        ImGui.TextColored(color, text);

        ImGui.TableNextColumn();
        ImGui.Text("/");

        ImGui.TableNextColumn();
        ImGui.Text(threshold.ToString());
    }

    private void DrawSettings(TrackedCurrency currency) {
        using var id = ImRaii.PushId(currency.ItemId.ToString());
        
        var configChanged = false;

        configChanged |= ImGui.Checkbox($"Enable", ref currency.Enabled);

        ImGuiHelpers.ScaledDummy(5.0f);

        configChanged |= ImGuiTweaks.Checkbox("Chat Warning", ref currency.ChatWarning, "When amount is above threshold, print a message to chat when changing zones");
        configChanged |= ImGuiTweaks.Checkbox("Invert", ref currency.Invert, "Warn when below the threshold instead of above");
        configChanged |= ImGuiTweaks.Checkbox("Overlay", ref currency.ShowInOverlay, "Allows this currency to show in the overlay");
        configChanged |= ImGuiTweaks.Checkbox("Overlay Show Name", ref currency.ShowItemName, "Show item name in the overlay");

        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.InputTextWithHint("##WarningText","Warning Text", ref currency.WarningText, 1024)) {
            configChanged = true;
        }

        ImGuiHelpers.ScaledDummy(5.0f);

        ImGui.PushItemWidth(50.0f * ImGuiHelpers.GlobalScale);
        configChanged |= ImGui.InputInt($"Threshold", ref currency.Threshold, 0, 0);

        ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 23.0f * ImGuiHelpers.GlobalScale);
        using (ImRaii.Disabled(!(ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl && currency.CanRemove))) {
            if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Trash, "Delete", new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale))) {
                System.Config.Currencies.Remove(currency);
                DeselectItem();
                System.Config.Save();
            }
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
            ImGui.SetTooltip(currency.CanRemove ? "Hold Shift + Control while clicking to delete this currency" : "Special currencies cannot be removed");
        }

        if (configChanged) {
            System.Config.Save();
        }
    }

    protected override void DrawExtraButton() {
        using var spacingTable = ImRaii.Table("buttonTable", 3, ImGuiTableFlags.SizingStretchSame);
        if (!spacingTable) return;

        ImGui.TableNextColumn();
        if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Plus, "addNewCurrency", ImGui.GetContentRegionAvail(), "Add New Normal Item")) {
            TryCloseAndRemoveItemListWindow();

            System.WindowManager.AddWindow(new ItemSelectionWindow(Service.PluginInterface) {
                MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Item),
            }, WindowFlags.OpenImmediately);
        }

        ImGui.TableNextColumn();
        if (ImGui.Button($"{SeIconChar.HighQuality.ToIconString()}##hqSearch", ImGui.GetContentRegionAvail())) {
            TryCloseAndRemoveItemListWindow();

            System.WindowManager.AddWindow(new HighQualityItemSelectionWindow(Service.PluginInterface) {
                MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.HighQualityItem),
            }, WindowFlags.OpenImmediately);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Add New High Quality Item");
        }

        ImGui.TableNextColumn();
        if (ImGui.Button($"{SeIconChar.Collectible.ToIconString()}##collectableSearch", ImGui.GetContentRegionAvail())) {
            TryCloseAndRemoveItemListWindow();

            System.WindowManager.AddWindow(new CollectableItemSelectionWindow(Service.PluginInterface) {
                MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Collectable),
            }, WindowFlags.OpenImmediately);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Add New Collectable Item");
        }
    }

    private static void AddSelectedItems(List<Item> selected, CurrencyType type) {
        foreach (var item in selected.Where(item => !System.Config.Currencies.Any(currency => currency.ItemId == item.RowId))) {
            System.Config.Currencies.Add(new TrackedCurrency {
                Enabled = true, Threshold = 1000, Type = type, ItemId = item.RowId,
            });
        }
    }

    private void TryCloseAndRemoveItemListWindow() {
        if (System.WindowManager.GetWindow<SelectionWindowBase<Item>>() is { } existingWindow) {
            existingWindow.Close();
            System.WindowManager.RemoveWindow(existingWindow);
        }
    }

    public override void OnClose() {
        System.OverlayController.OverlayListNode?.Save(OverlayController.ListNodeConfigPath);
        System.OverlayController.OverlayListNode?.FirstOrDefault()?.Save(OverlayController.CurrencyNodeConfigPath);
    }
}

public class GeneralSettingsTab : ITabItem {
    public string Name => "Settings";
    
    public bool Disabled => false;

    public void Draw() {
        var configChanged = false;

        ImGuiTweaks.Header("General Settings");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Enable Chat Warnings", ref System.Config.ChatWarning);
        }

        ImGuiTweaks.Header("Overlay Settings");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Hide in Duties", ref System.Config.HideInDuties);
        }
        
        if (configChanged) {
            System.Config.Save();
        }
    }
}

public class ListNodeSettingsTab : ITabItem {
    public string Name => "Warning List Style";
    
    public bool Disabled => false;
    
    public void Draw() {
        var listNode = System.OverlayController.OverlayListNode;
        if (listNode is null) return;
        
        ImGuiTweaks.Header("Warning List Overlay Style");
        DrawListConfig(listNode);
        
        listNode.RecalculateLayout();
    }

    private static void DrawListConfig(ListBoxNode<CurrencyWarningNode> listNode) {
        using var tabBar = ImRaii.TabBar("mode_select_tab_bar");
        if (!tabBar) return;

        DrawSimpleModeConfig(listNode);
        DrawAdvancedConfig(listNode);
    }

    private static void DrawSimpleModeConfig(ListBoxNode<CurrencyWarningNode> listNode) {
        using var tabItem = ImRaii.TabItem("Simple Mode");
        if (!tabItem) return;
        
        using var tabChild = ImRaii.Child("tabChild", ImGui.GetContentRegionAvail());
        if (!tabChild) return;
        
        using var table = ImRaii.Table("simple_mode_table", 2);
        if (!table) return;
        
        ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthStretch, 1.0f);
        ImGui.TableSetupColumn("##config", ImGuiTableColumnFlags.WidthStretch, 2.0f);
                
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Text("Position");
                        
        ImGui.TableNextColumn();
        var position = listNode.Position;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.DragFloat2("##position", ref position, 0.75f, 0.0f, 5000.0f)) {
            listNode.Position = position;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Size");
                
        ImGui.TableNextColumn();
        var size = listNode.Size;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.DragFloat2("##size", ref size, 0.75f, 0.0f, 5000.0f)) {
            listNode.Size = size;
        }
             
        ImGui.TableNextColumn();
        ImGui.Text("Scale");
        
        ImGui.TableNextColumn();
        ImGuiTweaks.SetFullWidth();
        var scale = listNode.Scale.X;
        if (ImGui.DragFloat("##Scale", ref scale, 0.005f, 0.10f, 3.0f)) {
            listNode.ScaleX = scale;
            listNode.ScaleY = scale;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Background Color");
                
        ImGui.TableNextColumn();
        var backgroundColor = listNode.BackgroundColor;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.ColorEdit4("##BackgroundColor", ref backgroundColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            listNode.BackgroundColor = backgroundColor;
        }
                
        ImGui.TableNextColumn();
        ImGui.Text("List Orientation");
                
        ImGui.TableNextColumn();
        var orientation = listNode.LayoutOrientation;
        ImGuiTweaks.SetFullWidth();
        if (ComboHelper.EnumCombo("##Orientation", ref orientation)) {
            listNode.LayoutOrientation = orientation;
        }

        ImGui.TableNextColumn();
        ImGui.Text("Anchor Corner");
                
        ImGui.TableNextColumn();
        var anchor = listNode.LayoutAnchor;
        ImGuiTweaks.SetFullWidth();
        if (ComboHelper.EnumCombo("##Anchor", ref anchor)) {
            listNode.LayoutAnchor = anchor;
        }

        ImGui.TableNextColumn();
        ImGui.Text("Category Vertical Spacing");
        
        ImGui.TableNextColumn();
        var categorySpacing = listNode.Margin.Top;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.DragFloat("##VerticalSpacing", ref categorySpacing, 0.10f, -5.0f, 5000.0f)) {
            listNode.Margin.Top = categorySpacing;

            listNode.ItemMargin = new Spacing(0.0f);
            
            foreach (var node in listNode) {
                node.Margin.Top = categorySpacing;
                node.Margin.Bottom = 0.0f;
            }
        }

        ImGui.TableNextColumn();
        ImGui.Text("Category Horizontal Spacing");
                
        ImGui.TableNextColumn();
        var horizontalSpacing = listNode.Margin.Left;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.DragFloat("##HorizontalSpacing", ref horizontalSpacing, 0.10f, -10.0f, 5000.0f)) {
            listNode.Margin.Left = horizontalSpacing;

            listNode.ItemMargin = new Spacing(0.0f);
            
            foreach (var node in listNode) {
                node.Margin.Left = horizontalSpacing;
                node.Margin.Right = 0.0f;
            }
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Show Background");

        ImGui.TableNextColumn();
        var background = listNode.BackgroundVisible;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.Checkbox("##BackgroundVisible", ref background)) {
            listNode.BackgroundVisible = background;
        }
                
        ImGui.TableNextColumn();
        ImGui.Text("Show Border");

        ImGui.TableNextColumn();
        var border = listNode.BorderVisible;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.Checkbox("##BorderVisible", ref border)) {
            listNode.BorderVisible = border;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Enable");

        ImGui.TableNextColumn();
        var enable = listNode.IsVisible;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.Checkbox("##Enable", ref enable)) {
            listNode.IsVisible = enable;
        }
    }
    
    private static void DrawAdvancedConfig(ListBoxNode<CurrencyWarningNode> listNode) {
        using var tabItem = ImRaii.TabItem("Advanced Mode");
        if (!tabItem) return;
        
        using var tabChild = ImRaii.Child("tabChild", ImGui.GetContentRegionAvail());
        if (!tabChild) return;
        
        listNode.DrawConfig();
    }
}

public class CurrencyNodeSettingsTab : ITabItem {
    public string Name => "Currency Node Style";
    
    public bool Disabled => false;
    
    public void Draw() {
        var listNode = System.OverlayController.OverlayListNode;
        if (listNode is null) return;

        var firstNode = listNode.FirstOrDefault();
        if (firstNode is null) return;
                
        ImGuiTweaks.Header("Currency Node Overlay Style");

        using var tabBar = ImRaii.TabBar("mode_select_tab_bar");
        if (!tabBar) return;
        
        DrawSimpleModeConfig(firstNode, listNode);
        
        DrawAdvancedModeConfig(firstNode);
        
        listNode.RecalculateLayout();
    }

    private void DrawSimpleModeConfig(CurrencyWarningNode firstNode, ListBoxNode<CurrencyWarningNode> listNode) {
        using var tabItem = ImRaii.TabItem("Simple Mode");
        if (!tabItem) return;
        
        using var tabChild = ImRaii.Child("tabChild");
        if (!tabChild) return;
        
        using var table = ImRaii.Table("simple_mode_table", 2);
        if (!table) return;
        
        ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthStretch, 1.0f);
        ImGui.TableSetupColumn("##config", ImGuiTableColumnFlags.WidthStretch, 2.0f);
                
        ImGui.TableNextRow();
        
        ImGui.TableNextColumn();
        ImGui.Text("Text Color");
        
        ImGui.TableNextColumn();
        var textColor = firstNode.TextColor;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.ColorEdit4("##TextColor", ref textColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            firstNode.TextColor = textColor;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Text Font");
        
        ImGui.TableNextColumn();
        var textFont = firstNode.LabelFont;
        ImGuiTweaks.SetFullWidth();
        if (ComboHelper.EnumCombo("##TextFont", ref textFont)) {
            firstNode.LabelFont = textFont;
        }

        ImGui.TableNextColumn();
        ImGui.Text("Text Size");
        
        ImGui.TableNextColumn();
        var textSize = firstNode.TextSize;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.InputInt("##TextSize", ref textSize)) {
            firstNode.TextSize = textSize;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Show Icon");
        
        ImGui.TableNextColumn();
        var showIcon = firstNode.ShowIcon;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.Checkbox("##ShowIcon", ref showIcon)) {
            firstNode.ShowIcon = showIcon;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Show Text");
        
        ImGui.TableNextColumn();
        var showText = firstNode.ShowText;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.Checkbox("##ShowText", ref showText)) {
            firstNode.ShowText = showText;
        }
        
        ImGui.TableNextColumn();
        ImGui.Text("Show Item Count");

        ImGui.TableNextColumn();
        var showItemCount = firstNode.ShowItemCount;
        ImGuiTweaks.SetFullWidth();
        if (ImGui.Checkbox("##ShowItemCount", ref showItemCount)) {
            firstNode.ShowItemCount = showItemCount;
        }
        
        ApplyAll(firstNode, listNode);
    }

    private void ApplyAll(CurrencyWarningNode referenceNode, ListBoxNode<CurrencyWarningNode> listNode) {
        foreach (var node in listNode) {
            node.Load(referenceNode);
        }
    }

    private void DrawAdvancedModeConfig(CurrencyWarningNode firstNode) {
        using var tabItem = ImRaii.TabItem("Advanced Mode");
        if (!tabItem) return;
        
        using var tabChild = ImRaii.Child("tabChild");
        if (!tabChild) return;

        ImGui.Spacing();
        ImGui.TextColored(KnownColor.GreenYellow.Vector(), "Modifications will only appear to effect the first warning, but once saved will apply to all warnings");
        ImGui.Spacing();
        ImGui.Spacing();
        
        firstNode.DrawConfig();
    }
}
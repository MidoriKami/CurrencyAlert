using System;
using System.Drawing;
using System.Numerics;
using CurrencyAlert.Controllers;
using CurrencyAlert.Models;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace CurrencyAlert.Views.Views;

public class CurrencyConfigView {
    private readonly TrackedCurrency currency;

    public CurrencyConfigView(TrackedCurrency currency) {
        this.currency = currency;
    }

    public void Draw() {
        DrawHeaderAndWatermark();
        DrawCurrentStatus();
        DrawSettings();
    }

    private void DrawCurrentStatus() {
        if (currency is not { CurrentCount: var currentCount, Threshold: var threshold }) return;
        
        var color = ((float)currentCount / threshold) switch {
            < 0.75f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.White.Vector(),
            < 0.85f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.Orange.Vector(),
            < 0.95f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.OrangeRed.Vector(),
            > 0.95f and < 1.00f => KnownColor.Red.Vector(),
            >= 1.00f and < 1.05f => currency.Invert ? KnownColor.OrangeRed.Vector() : KnownColor.Red.Vector(),
            >= 1.05f and < 1.15f => currency.Invert ? KnownColor.Orange.Vector() : KnownColor.Red.Vector(),
            >= 1.15f => currency.Invert ? KnownColor.White.Vector() : KnownColor.Red.Vector(),
            _ => KnownColor.White.Vector(),
        };
        
        if (ImGui.BeginTable("CurrentStatusTable", 3)) {
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

            ImGui.EndTable();
        }
        ImGuiHelpers.ScaledDummy(10.0f);
    }

    private void DrawHeaderAndWatermark() {
        if (currency is not { Name: var name, Icon: {} icon}) return;
        
        var region = ImGui.GetContentRegionAvail();
        var minDimension = Math.Min(region.X, region.Y);

        var textSize = ImGui.CalcTextSize(name);
        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
        ImGui.Text(name);
        ImGui.Separator();

        var areaStart = ImGui.GetCursorPos();
        ImGui.SetCursorPosX(region.X / 2.0f - minDimension / 2.0f);
        ImGui.Image(icon.ImGuiHandle, new Vector2(minDimension), Vector2.Zero, Vector2.One, Vector4.One with { W = 0.10f });
        ImGui.SetCursorPos(areaStart);
    }
    
    private void DrawSettings() {
        if (currency is not { ItemId: var itemId, Enabled: var enabled, ChatWarning: var chatWarning, ShowInOverlay: var overlay, Invert: var invert, Threshold: var threshold }) return;
        
        if (ImGui.Checkbox($"Enable##{itemId}", ref enabled)) {
            currency.Enabled = enabled;
            CurrencyAlertSystem.Config.Save();
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        
        if (ImGui.Checkbox($"Chat Warning##{itemId}", ref chatWarning)) {
            currency.ChatWarning = chatWarning;
            CurrencyAlertSystem.Config.Save();
        }
        ImGuiComponents.HelpMarker("When amount is above threshold, print a message to chat when changing zones");

        if (ImGui.Checkbox($"Overlay##{itemId}", ref overlay)) {
            currency.ShowInOverlay = overlay;
            CurrencyAlertSystem.Config.Save();
        }
        ImGuiComponents.HelpMarker("Allows this currency to show in the overlay");
        
        if (ImGui.Checkbox($"Invert##{itemId}", ref invert)) {
            currency.Invert = invert;
            CurrencyAlertSystem.Config.Save();
        }
        ImGuiComponents.HelpMarker("Warn when below the threshold instead of above");
        
        ImGuiHelpers.ScaledDummy(5.0f);

        ImGui.PushItemWidth(50.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt($"Threshold##{itemId}", ref threshold, 0, 0)) {
            currency.Threshold = threshold;
            CurrencyAlertSystem.Config.Save();
        }
        
        ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 23.0f * ImGuiHelpers.GlobalScale);
        var hotkeyHeld = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl && currency.CanRemove;

        if (!hotkeyHeld) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale)) && hotkeyHeld && currency.CanRemove) {
            CurrencyAlertSystem.Config.Currencies.Remove(currency);
            CurrencyAlertSystem.Config.Save();
        }
        ImGui.PopFont();
        
        if (!hotkeyHeld) ImGui.PopStyleVar();
        
        if (ImGui.IsItemHovered() && !hotkeyHeld) {
            ImGui.SetTooltip(currency.CanRemove ? "Hold Shift + Control while clicking to delete this currency" : "Special currencies cannot be removed");
        }
    }
}
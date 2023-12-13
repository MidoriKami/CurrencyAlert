using CurrencyAlert.Controllers;
using Dalamud.Interface.Utility;
using ImGuiNET;
using KamiLib.Interfaces;

namespace CurrencyAlert.Views.Windows.WindowTabs;

public class GeneralSettingsTab : ITabItem {
    public string TabName => "Settings";
    public bool Enabled => true;

    public void Draw() {
        var settingsChange = false;
        
        ImGui.Text("General Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Enable Chat Warnings", ref CurrencyAlertSystem.Config.ChatWarning);
        ImGuiHelpers.ScaledDummy(5.0f);
        
        ImGui.Text("Overlay Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Enabled", ref CurrencyAlertSystem.Config.OverlayEnabled);
        settingsChange |= ImGui.Checkbox("Reposition Mode", ref CurrencyAlertSystem.Config.RepositionMode);
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Show Icon", ref CurrencyAlertSystem.Config.OverlayIcon);
        settingsChange |= ImGui.Checkbox("Show Text", ref CurrencyAlertSystem.Config.OverlayText);
        settingsChange |= ImGui.Checkbox("Show Long Text", ref CurrencyAlertSystem.Config.OverlayLongText);
        settingsChange |= ImGui.Checkbox("Show Background", ref CurrencyAlertSystem.Config.ShowBackground);
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Single Line Mode", ref CurrencyAlertSystem.Config.SingleLine);
        ImGuiHelpers.ScaledDummy(5.0f);
        
        settingsChange |= ImGui.ColorEdit4("Text Color", ref CurrencyAlertSystem.Config.OverlayTextColor, ImGuiColorEditFlags.AlphaPreviewHalf);
        settingsChange |= ImGui.ColorEdit4("Background Color", ref CurrencyAlertSystem.Config.BackgroundColor, ImGuiColorEditFlags.AlphaPreviewHalf);

        if (ImGui.DragFloat2("Overlay Position", ref CurrencyAlertSystem.Config.OverlayDrawPosition, 5.0f)) {
            CurrencyAlertSystem.Config.WindowPosChanged = true;
            settingsChange = true;
        }
        
        if (settingsChange) CurrencyAlertSystem.Config.Save();
    }
}
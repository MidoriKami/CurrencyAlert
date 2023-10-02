using CurrencyAlert.Controllers;
using Dalamud.Interface.Utility;
using ImGuiNET;
using KamiLib.Interfaces;

namespace CurrencyAlert.Views.Windows.WindowTabs;

public class GeneralSettingsTab : ITabItem
{
    public string TabName => "Settings";
    public bool Enabled => true;

    public void Draw()
    {
        ImGui.Text("General Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Enable Chat Warnings", ref CurrencyAlertSystem.Config.ChatWarning))
        {
            CurrencyAlertSystem.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);
        
        ImGui.Text("Overlay Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Enabled", ref CurrencyAlertSystem.Config.OverlayEnabled))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.Checkbox("Reposition Mode", ref CurrencyAlertSystem.Config.RepositionMode))
        {
            CurrencyAlertSystem.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Right Align", ref CurrencyAlertSystem.Config.RightAlign))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.Checkbox("Grow Upwards", ref CurrencyAlertSystem.Config.GrowUp))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.Checkbox("Show Icon", ref CurrencyAlertSystem.Config.OverlayIcon))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.Checkbox("Show Text", ref CurrencyAlertSystem.Config.OverlayText))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.Checkbox("Show Long Text", ref CurrencyAlertSystem.Config.OverlayLongText))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.Checkbox("Show Background", ref CurrencyAlertSystem.Config.ShowBackground))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.ColorEdit4("Text Color", ref CurrencyAlertSystem.Config.OverlayTextColor, ImGuiColorEditFlags.AlphaPreviewHalf))
        {
            CurrencyAlertSystem.Config.Save();
        }
        
        if (ImGui.ColorEdit4("Background Color", ref CurrencyAlertSystem.Config.BackgroundColor, ImGuiColorEditFlags.AlphaPreviewHalf))
        {
            CurrencyAlertSystem.Config.Save();
        }

        if (ImGui.DragFloat2("Overlay Position", ref CurrencyAlertSystem.Config.OverlayDrawPosition, 5.0f))
        {
            CurrencyAlertSystem.Config.WindowPosChanged = true;
            CurrencyAlertSystem.Config.Save();
        }
    }
}
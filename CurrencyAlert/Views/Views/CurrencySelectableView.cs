using System.Drawing;
using CurrencyAlert.Models;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace CurrencyAlert.Views.Views;

public class CurrencySelectableView {
    private readonly TrackedCurrency currency;

    public CurrencySelectableView(TrackedCurrency currency) {
        this.currency = currency;
    }

    public void Draw() {
        if (currency is { Name: var name, Icon: { } icon}) {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3.0f * ImGuiHelpers.GlobalScale);
            ImGui.Image(icon.ImGuiHandle, ImGuiHelpers.ScaledVector2(24.0f));
            
            ImGui.SameLine();
            ImGui.Text(name);
        }
        else {
            ImGui.TextColored(KnownColor.OrangeRed.Vector(), $"Error, unable to display currency. ItemId: {currency.ItemId}");
        }
    }
}
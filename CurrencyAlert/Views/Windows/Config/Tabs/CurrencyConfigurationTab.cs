using System.Collections.Generic;
using System.Linq;
using CurrencyAlert.Controllers;
using CurrencyAlert.Models;
using CurrencyAlert.Views.Views;
using KamiLib.Interfaces;

namespace CurrencyAlert.Views.Windows.WindowTabs;

public class CurrencyConfigurationTab : ISelectionWindowTab
{
    public string TabName => "Currencies";
    public ISelectable? LastSelection { get; set; }

    public IEnumerable<ISelectable> GetTabSelectables() => CurrencyAlertSystem.Config.Currencies
        .Select(currency => new TrackedCurrencySelectable(currency));
}

public class TrackedCurrencySelectable : ISelectable, IDrawable
{
    public IDrawable Contents => this;

    public string ID => currency.ItemId.ToString();

    private readonly TrackedCurrency currency;
    private readonly CurrencySelectableView label;
    private readonly CurrencyConfigView config;
    
    public TrackedCurrencySelectable(TrackedCurrency currency)
    {
        this.currency = currency;
        label = new CurrencySelectableView(this.currency);
        config = new CurrencyConfigView(this.currency);
    }

    public void DrawLabel() => label.Draw();

    public void Draw() => config.Draw();
}
using System;
using Terraria.Localization;

namespace MSEnchant.Models;

public class StarForceStatAttribute
{

    public StarForceAttributeType Type { get; init; }
    
    public double Value { get; set; }
    
    public StarForceStatAttribute(StarForceAttributeType type, double value)
    {
        Type = type;
        Value = value;
    }

    public string GetEquipTooltip(int baseValue)
    {
        baseValue = Math.Max(baseValue, 0);
        return Language.GetTextValue("Mods.MSEnchant.ItemTooltip.BonusAttribute_EquipToolTip", Name,
            (baseValue + Value).ToString("0"), baseValue, Value.ToString("0"));
    }

    public string EnchantTooltip => Language.GetTextValue("Mods.MSEnchant.ItemTooltip.BonusAttribute_EnchantToolTip", Name, Value.ToString("0"));

    public string Name => Language.GetTextValue($"Mods.MSEnchant.BonusAttribute.{Type}");

}
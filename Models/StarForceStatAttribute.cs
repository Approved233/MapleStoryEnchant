using System;

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
        return $"{Name} : {baseValue + Value:0} ({baseValue} +{Value:0})";
    }

    public string EnchantTooltip => $"{Name} ： +{Value:0}";

    public string Name => Type switch
    {
        StarForceAttributeType.Damage => "攻击力",
        StarForceAttributeType.Defense => "防御力"
    };

}
using System;

namespace MSEnchant.Models;

public class StarForceRareLevelSetting
{
    
    public int MaxStar;

    public int[] Costs;

    public int[] BonusDamageWeapon;
    public int[] BonusDamageArmor;

    public StarForceRareLevelSetting(int maxStar, int[] costs, int[] bonusDamageWeapon = null, int[] bonusDamageArmor = null)
    {
        MaxStar = maxStar;
        Costs = costs;
        
        BonusDamageWeapon = bonusDamageWeapon ?? Array.Empty<int>();
        BonusDamageArmor = bonusDamageArmor ?? Array.Empty<int>();
    }
    
}
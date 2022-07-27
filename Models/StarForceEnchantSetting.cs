using Terraria;

namespace MSEnchant.Models;

public class StarForceEnchantSetting
{
    public Item Item;

    public int BaseCosts;

    public bool Protect;

    public StarForceChanceSetting Chance;

    public bool IsMiniGameSuccess;

    public double SuccessRate
    {
        get
        {
            var successRate = Chance.SuccessRate;
            if (IsMiniGameSuccess)
                successRate *= 1.05;

            return successRate;
        }
    }

    public int Costs
    {
        get
        {
            var costs = BaseCosts;
            if (Protect)
                costs *= 2;

            return costs;
        }
    }
}
using Terraria.GameContent.ItemDropRules;

namespace MSEnchant.DropRules;

public class EnemyCommonDrop : CommonDrop
{
    public EnemyCommonDrop(int itemId, int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1) : base(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator)
    {
    }

    public override bool CanDrop(DropAttemptInfo info)
    {
        return !info.npc.friendly && info.npc.damage > 0 && info.npc.value > 0;
    }
    
    
}
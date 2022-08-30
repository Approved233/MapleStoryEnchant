using MSEnchant.Helper;
using MSEnchant.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MSEnchant.Globals;

public class MSEnchantGlobalItem : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.IsBossBag() || item.IsFishingCrate())
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarItem>(), 1, 50, 300));
        }
    }
}
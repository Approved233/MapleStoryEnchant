using MSEnchant.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MSEnchant.Globals;

public class MSEnchantGlobalItem : GlobalItem
{
    
     public override void OpenVanillaBag(string context, Player player, int type)
     {
         if (context is "bossBag" or "crate")
         {
             var starItem = ModContent.GetInstance<StarItem>().Item;
             player.QuickSpawnItem(player.GetSource_OpenItem(type, context), starItem, Main.rand.Next(50, 300));
         }
     }
     
}
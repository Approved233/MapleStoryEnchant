using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MSEnchant.DropRules;
using MSEnchant.Helper;
using MSEnchant.Items;
using MSEnchant.Models;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MSEnchant.Globals;

public class MSEnchantGlobalNPC : GlobalNPC
{
    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
        var starItem = ModContent.GetInstance<StarItem>();
        globalLoot.Add(new EnemyCommonDrop(starItem.Type, 100, 1, 20, 15));
    }

    public override void OnKill(NPC npc)
    {
        if (!npc.boss)
            return;

        var setting = Global.StarScrollLootSettings.FirstOrDefault(s => s.Type == npc.type);
        if (setting.Type == 0)
            return;

        var chance = PlayerHelper.RollNearPlayersLuck(npc.position, 100 * 1000);
        if (chance > 10 * 1000)
            return;

        const int baseMinStars = 5;
        const int baseMaxStars = 22;
        var value = setting.Value;

        var bonusMaxStars = (int)(value / 125000f);
        var bonusMinStars = (int)(value / 625000f);

        var minStars = Math.Clamp(baseMinStars + bonusMinStars, baseMinStars, 15);
        var maxStars = Math.Min(minStars + bonusMaxStars, baseMaxStars);

        var scrollItem = ModContent.GetInstance<StarForceScrollItem>();

        var index = Item.NewItem(npc.GetSource_Loot(),
            new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), scrollItem.Type);
        var item = Main.item.ElementAtOrDefault(index);
        if (item == null)
            return;

        scrollItem = item.ModItem as StarForceScrollItem;
        if (scrollItem == null)
            return;

        scrollItem.ScrollStarForce = Main.rand.Next(minStars, maxStars);
        scrollItem.SuccessRate = Math.Max(0.01, Main.rand.NextDouble());
#if DEBUG
        Global.Logger.Info($"Dropped Star Force Scroll from {npc.FullName} StarForce: {scrollItem.ScrollStarForce} SuccessRate: {scrollItem.SuccessRate * 100:0}");
#endif
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (!npc.boss)
            return;

        Global.UpdateScheduleQueue.Enqueue(() =>
        {
            var loots = npcLoot.Get(false);

            var totalRules = new List<IItemDropRule>();

            void GetRules(IEnumerable<IItemDropRule> rules)
            {
                foreach (var rule in rules)
                {
                    var ruleFields = rule.GetType().GetFields().Where(f => f.FieldType == typeof(IItemDropRule))
                        .Select(f => f.GetValue(rule)).Cast<IItemDropRule>().ToArray();
                    if (rule.ChainedRules.Count > 0)
                        GetRules(rule.ChainedRules.Select(r => r.RuleToChain));

                    if (ruleFields.Length > 0)
                        GetRules(ruleFields);

                    totalRules.Add(rule);
                }
            }

            GetRules(loots);

            foreach (var rule in totalRules)
            {
                if (rule is not CommonDrop commonDropLoot)
                    continue;
                
                if (!ItemID.Sets.BossBag[commonDropLoot.itemId] &&
                    (ItemLoader.GetItem(commonDropLoot.itemId)?.IsBossBag() ?? false))
                    continue;
#if DEBUG
                Global.Logger.Info($"Added StarForce scroll loot npc: {npc.FullName} value: {npc.value}");
#endif
                Global.StarScrollLootSettings.Add(new StarForceScrollLootSetting
                {
                    Type = npc.type,
                    Value = npc.value
                });
                break;
            }
        });
    }
}
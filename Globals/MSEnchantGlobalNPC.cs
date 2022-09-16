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
        globalLoot.Add(new EnemyCommonDrop(ModContent.ItemType<StarItem>(), 100, 1, 20, 15));
    }

    public override void OnKill(NPC npc)
    {
        if (!npc.boss)
            return;

        var setting = Global.StarScrollLootSettings.FirstOrDefault(s => s.Type == npc.type);
        if (setting.Type == 0)
            return;

        const int baseMinStars = 5;
        const int baseMaxStars = 22;
        var value = setting.Value;

        var min = Global.StarScrollLootSettings.First(s => s.Type == NPCID.KingSlime).Value;
        var middle = Global.StarScrollLootSettings.First(s => s.Type == NPCID.WallofFlesh).Value;
        
        var bonusMaxStars = (int)(value / min);
        var bonusMinStars = (int)(value / middle);

        var minStars = Math.Clamp(baseMinStars + bonusMinStars, baseMinStars, 15);
        var maxStars = Math.Min(minStars + bonusMaxStars, baseMaxStars);

        RollStarScrollDropPerInteraction(npc, minStars, maxStars);

#if DEBUG
        Global.Logger.Info($"Dropped Star Force Scroll from {npc.FullName} StarForce: {scrollItem.ScrollStarForce} SuccessRate: {scrollItem.SuccessRate * 100:0} Range: {minStars}-{maxStars}");
#endif
    }

    protected void RollStarScrollDropPerInteraction(NPC npc, int min, int max)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            for (var remoteClient = 0; remoteClient < byte.MaxValue; ++remoteClient)
            {
                if (Main.player[remoteClient].active && npc.playerInteraction[remoteClient])
                    RollStarScrollDrop(Main.player[remoteClient], npc, min, max);
            }
        }
        else
        {
            RollStarScrollDrop(Main.LocalPlayer, npc, min, max);
        }
    }

    protected void RollStarScrollDrop(Player player, NPC npc, int min, int max)
    {
        var chance = player.RollLuck(100 * 1000);
        if (chance > 10 * 1000)
            return;

        player.DropItemLocal(npc.GetSource_Loot(),
            new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height),
            ModContent.ItemType<StarForceScrollItem>(), onItemSpawn:
            index =>
            {
                var item = Main.item.ElementAtOrDefault(index);
                if (item?.ModItem is not StarForceScrollItem scrollItem)
                    return;
                
                scrollItem.ScrollStarForce = Main.rand.Next(min, max);
                scrollItem.SuccessRate = Math.Clamp(Main.rand.NextDouble(), 0.01, 1);
            });
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (!npc.boss || npc.value == 0)
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
                
                if (!ItemID.Sets.BossBag[commonDropLoot.itemId] && !ItemLoader.GetItem(commonDropLoot.itemId).IsBossBag())
                    continue;

                Global.Logger.Info($"Added StarForce scroll loot npc: {npc.FullName} value: {npc.value}");

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
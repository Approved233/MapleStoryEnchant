using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MSEnchant.Effects;
using MSEnchant.Globals;
using MSEnchant.Models;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net;

namespace MSEnchant.Helper;

public static class PlayerHelper
{
    public static void SendMessage(this Player player, string msg, Color? color = null)
    {
        var c = color.GetValueOrDefault(Color.White);
        ChatHelper.DisplayMessageOnClient(NetworkText.FromLiteral(msg), c, player.whoAmI);
    }

    public static bool TryFindEmptySlot(this Player player, out int slot)
    {
        for (var i = Main.InventoryItemSlotsStart;
             i < Main.InventoryItemSlotsStart + Main.InventoryItemSlotsCount;
             i++)
        {
            if (!player.inventory[i].IsAir)
                continue;

            slot = i;
            return true;
        }

        slot = -1;
        return false;
    }

    public static int FindEmptySlot(this Player player)
    {
        TryFindEmptySlot(player, out var i);
        return i;
    }

    public static float GetEquipmentBonus(this Player player, StarForceAttributeType type)
    {
        var bonus = 0f;
        for (var i = 0; i < 10; i++)
        {
            var item = player.armor.ElementAtOrDefault(i);
            var msItem = item?.GetEnchantItem();
            if (msItem == null)
                continue;

            if (msItem.Destroyed)
            {
                if (type == StarForceAttributeType.Defense && item.defense > 0)
                    bonus -= item.defense;

                continue;
            }

            bonus += (float)(msItem.BonusAttributes.FirstOrDefault(a => a.Type == type)?.Value ?? 0);
        }

        return bonus;
    }

    public static bool FindItemInInventory(this Player player, Item item)
    {
        for (var i = Main.InventoryItemSlotsStart;
             i < Main.InventoryItemSlotsStart + Main.InventoryItemSlotsCount;
             i++)
        {
            if (player.inventory[i] == item)
                return true;
        }

        return false;
    }

    public static bool HasItem<T>(this Player player, int num) where T : ModItem
    {
        return HasItem(player, ModContent.GetInstance<T>().Type, num);
    }

    public static bool HasItem(this Player player, int type, int num)
    {
        var count = 0;
        for (var i = Main.InventoryItemSlotsStart; i < Main.InventoryItemSlotsStart + Main.InventoryItemSlotsCount; i++)
        {
            if (count >= num)
                break;

            if (type != player.inventory[i].type)
                continue;

            var stack = player.inventory[i].stack;
            if (stack > num)
            {
                count += num;
            }
            else if (stack <= num)
            {
                count += stack;
            }
        }

        return count >= num;
    }

    public static bool CostItem<T>(this Player player, int num) where T : ModItem
    {
        return CostItem(player, ModContent.GetInstance<T>().Type, num);
    }

    public static bool CostItem(this Player player, int type, int num)
    {
        var count = 0;
        for (var i = Main.InventoryItemSlotsStart; i < Main.InventoryItemSlotsStart + Main.InventoryItemSlotsCount; i++)
        {
            if (count >= num)
                break;

            if (type == player.inventory[i].type)
            {
                var stack = player.inventory[i].stack;
                if (stack > num)
                {
                    player.inventory[i].stack -= num;
                    count += num;
                }
                else if (stack <= num)
                {
                    player.inventory[i].TurnToAir();
                    count += stack;
                }
            }
        }

        return count >= num;
    }

    public static bool PlayEffect(this Player player, EffectType type)
    {
        if (!Global.WorldEffectAnimations.TryGetValue(type, out var animation))
            return false;
        
        return PlayEffect(player, animation.Clone());
    }

    public static bool PlayEffect(this Player player, WorldEffectAnimation animation)
    {
        if (!player.TryGetModPlayer<MSEnchantPlayer>(out var msPlayer))
            return false;
        
        msPlayer.PlayEffect(animation);
        return true;
    }

    public static int RollNearPlayersLuck(Vector2 pos, int range, float max = 1f)
    {
        var luck = Math.Min(max, Player.GetClosestPlayersLuck(pos));
        if (luck > 0.0 && Main.rand.NextFloat() < (double) luck)
            return Main.rand.Next(Main.rand.Next(range / 2, range));
        return luck < 0.0 && Main.rand.NextFloat() < 0.0 - luck ? Main.rand.Next(Main.rand.Next(range, range * 2)) : Main.rand.Next(range);
    }
}
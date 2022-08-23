using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MSEnchant.Effects;
using MSEnchant.Globals;
using MSEnchant.Helper;
using MSEnchant.Items;
using MSEnchant.Models;
using MSEnchant.Network;
using MSEnchant.Network.Packets;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using MSEnchant.UI.Window;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant;

public class MSEnchantSystem : ModSystem
{
    protected MSEnchantUI State;

    protected MSInventoryUI InventoryState;

    public override void OnModLoad()
    {
        if (Main.dedServ) return;

        State = new MSEnchantUI();
        InventoryState = new MSInventoryUI();
    }

    public override void OnModUnload()
    {
        State = null;
        InventoryState = null;
    }

    public override void PreSaveAndQuit()
    {
        if (Main.dedServ) return;

        State.CleanElements();
    }

    public override void PostSetupContent()
    {
        Global.UpdateScheduleQueue.Enqueue(LoadAttributeBonus);
    }

    private KeyValuePair<Item, int>? lastMouseItem = null;
    private List<Item>? lastInventory = null;
    private TimeSpan lastUpdateInventoryTime;

    public override void UpdateUI(GameTime gameTime)
    {
        if (Main.dedServ)
            return;

        UpdateInventory(gameTime);

        while (Global.RemoveElementQueue.TryDequeue(out var element))
        {
            element.Remove();
        }

        while (Global.AppendElementQueue.TryDequeue(out var element))
        {
            State.Append(element);
        }

        State.UserInterface.Update(gameTime);
        InventoryState.UserInterface.Update(gameTime);

        UpdateStarScroll();
    }

    protected void UpdateInventory(GameTime gameTime)
    {
        if (lastInventory != null)
        {
            if (!Main.mouseItem.IsNullOrAir())
            {
                for (var i = Main.InventoryItemSlotsStart;
                     i < Main.InventoryItemSlotsStart + Main.InventoryItemSlotsCount;
                     i++)
                {
                    if (lastInventory[i] == Main.LocalPlayer.inventory[i])
                        continue;

                    if (lastInventory[i] == Main.mouseItem)
                    {
                        lastMouseItem = new KeyValuePair<Item, int>(Main.mouseItem, i);
                        break;
                    }
                }
            }
            else if (lastMouseItem != null)
            {
                lastMouseItem = null;
            }
        }

        if (gameTime.TotalGameTime != lastUpdateInventoryTime)
        {
            lastUpdateInventoryTime = gameTime.TotalGameTime;
            lastInventory = Main.LocalPlayer.inventory.ToList();
        }
    }

    private KeyValuePair<Item, int>? lastScrollItem;

    protected void UpdateStarScroll()
    {
        if (!Main.playerInventory || State.Children.Any(e => e is MSWindow) || lastMouseItem == null)
        {
            lastScrollItem = null;
            return;
        }

        if (lastMouseItem.Value.Key.ModItem is StarForceScrollItem && lastScrollItem == null)
        {
            lastScrollItem = new KeyValuePair<Item, int>(lastMouseItem.Value.Key, lastMouseItem.Value.Value);
        }

        if (lastScrollItem != null && lastScrollItem.Value.Key != lastMouseItem.Value.Key)
        {
            var (scrollItemVanilla, scrollItemSlot) = lastScrollItem.Value;
            lastScrollItem = null;

            if (scrollItemVanilla.ModItem is not StarForceScrollItem scrollItem)
                return;

            var (mouseItem, mouseItemSlot) = lastMouseItem.Value;

            var msItem = mouseItem.GetEnchantItem();
            if (msItem == null)
                return;

            if (!msItem.HasStarForce)
                return;

            var targetItemSlot = Main.LocalPlayer.inventory[scrollItemSlot];
            var validSlot = scrollItemSlot;

            if (targetItemSlot.IsNullOrAir() || Main.LocalPlayer.TryFindEmptySlot(out validSlot))
                Main.LocalPlayer.inventory[validSlot] = scrollItemVanilla;
            else
                return;

            var currentScrollItemSlot = Main.LocalPlayer.inventory[mouseItemSlot];
            validSlot = mouseItemSlot;
            if (currentScrollItemSlot == scrollItemVanilla || Main.LocalPlayer.TryFindEmptySlot(out validSlot))
            {
                Main.LocalPlayer.inventory[validSlot] = mouseItem;
                Main.mouseItem = new Item();
            }
            else
                return;

            if (!scrollItem.CanApplyTo(mouseItem))
            {
                State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.InvalidItem"));
                return;
            }

            State.ShowNoticeYesNoCenter(Language.GetTextValue("Mods.MSEnchant.UIText.StarScrollConsumeQuestion", mouseItem.Name, scrollItemVanilla.Name), () =>
            {
                if (!Main.LocalPlayer.FindItemInInventory(scrollItemVanilla) ||
                    !Main.LocalPlayer.FindItemInInventory(mouseItem))
                {
                    State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.ProcessingActions"));
                    return;
                }

                var result = scrollItem.ApplyTo(mouseItem);
                if (result == StarForceScrollResult.NoResult)
                {
                    State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.InvalidItem"));
                    return;
                }

                scrollItemVanilla.TurnToAir();

                var sound = result switch
                {
                    StarForceScrollResult.Success => "MSEnchant/Assets/ScrollSuccess",
                    StarForceScrollResult.Failed => "MSEnchant/Assets/ScrollFailure",
                    _ => null
                };

                EffectType? effect = result switch
                {
                    StarForceScrollResult.Success => EffectType.ScrollSuccess,
                    StarForceScrollResult.Failed => EffectType.ScrollFailure,
                    _ => null
                };

                var hint = Language.GetTextValue($"Mods.MSEnchant.UIText.ScrollResult_{result}", scrollItemVanilla.Name, mouseItem.Name);

                if (sound != null)
                    SoundEngine.PlaySound(new SoundStyle(sound), Main.LocalPlayer.position);

                if (effect != null)
                {
                    Main.LocalPlayer.PlayEffect(effect.Value);
                    Global.Mod.SendPacket<RequestPlayEffectPacket>(PacketType.RequestPlayEffect, packet =>
                    {
                        packet.Effect = effect.Value;
                        packet.Sound = sound ?? string.Empty;
                    });
                }

                if (hint != null)
                    Main.LocalPlayer.SendMessage($"[c/FFAAAA:{hint}]");
            });
        }
    }

    protected void LoadAttributeBonus()
    {
        var vanillaWeaponDamages = new Dictionary<int, int>();
        var vanillaArmorDefenses = new Dictionary<int, int>();

        for (var i = 1; i < ItemID.Count; i++)
        {
            var item = new Item(i);
            var type = item.GetItemType();

            if (type == ItemType.Weapon)
                vanillaWeaponDamages[i] = item.OriginalDamage;
            else if (type == ItemType.Armor)
                vanillaArmorDefenses[i] = item.OriginalDefense;
        }

        var modItems = typeof(ItemLoader).GetField("items", BindingFlags.Static | BindingFlags.NonPublic)
            ?.GetValue(null) as List<ModItem>;

        var modWeaponDamages = new Dictionary<Mod, List<int>>();
        var modArmorDefenses = new Dictionary<Mod, List<int>>();

        foreach (var item in modItems)
        {
            if (item.Mod.Name == "ModLoader")
                continue;

            var vanillaItem = item.Item;
            if (!modWeaponDamages.ContainsKey(item.Mod))
                modWeaponDamages[item.Mod] = new List<int>();
            if (!modArmorDefenses.ContainsKey(item.Mod))
                modArmorDefenses[item.Mod] = new List<int>();

            var type = vanillaItem.GetItemType();

            if (type == ItemType.Weapon)
            {
                // Logger.Info(
                //     $"Mod: {item.Mod.Name} Weapon: {vanillaItem.Name} Rarity: {vanillaItem.OriginalRarity} Damage: {vanillaItem.OriginalDamage}");
                modWeaponDamages[item.Mod].Add(vanillaItem.OriginalDamage);
            }
            else if (type == ItemType.Armor)
            {
                // Logger.Info(
                //     $"Mod: {item.Mod.Name} Armor: {vanillaItem.Name} Rarity: {vanillaItem.OriginalRarity} Defense: {vanillaItem.OriginalDefense}");
                modArmorDefenses[item.Mod].Add(vanillaItem.OriginalDefense);
            }
        }

        foreach (var (_, value) in modWeaponDamages)
        {
            Global.AttributeStatBonus[StarForceAttributeType.Damage] +=
                CalculateBonus(vanillaWeaponDamages.Values, value, 0.085f);
        }

        foreach (var (_, value) in modArmorDefenses)
        {
            Global.AttributeStatBonus[StarForceAttributeType.Defense] +=
                CalculateBonus(vanillaArmorDefenses.Values, value, 0.2f);
        }

        foreach (var player in Main.player)
        {
            player.bank.item.UpdateStarForceAttributes();
            player.bank2.item.UpdateStarForceAttributes();
            player.bank3.item.UpdateStarForceAttributes();
            player.bank4.item.UpdateStarForceAttributes();
            player.inventory.UpdateStarForceAttributes();
            player.armor.UpdateStarForceAttributes();
        }
    }

    private float CalculateBonus(IEnumerable<int> vanilla, IEnumerable<int> mod, float step)
    {
        var vanillaValues = vanilla.ToArray();
        var modValues = FilterLargeNumber(mod.ToList().OrderByDescending(v => v).ToArray());

        var vanillaAverage = vanillaValues.Average();

        var max = 0d;
        var avg = 0d;
        if (modValues.Length > 0)
        {
            max = modValues.Max();
            avg = modValues.AsQueryable().Average();
        }

        if (max < vanillaValues.Max())
            return 0f;

        return (float)(avg / vanillaAverage * step);
    }

    private int[] FilterLargeNumber(IEnumerable<int> input)
    {
        var a = input.ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            var current = a[i];

            if (i + 1 >= a.Length)
                break;

            var next = a[i + 1];

            if ((double)current / next >= 1.5)
                continue;

            return a[i..];
        }

        return a;
    }

    public override void PostUpdateEverything()
    {
        while (Global.UpdateScheduleQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }

        if (Main.dedServ)
            return;

        if (Global.EnableEnchantUIKey.JustReleased)
        {
            MSEnchantUI.Instance.ToggleWindow<MainWindow>();
        }

        if (Main.oldKeyState.IsKeyDown(Keys.Escape) && Main.keyState.IsKeyUp(Keys.Escape))
        {
            MSEnchantUI.Instance.CloseFrontWindow();
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "MSEnchant: UI",
                delegate
                {
                    if (State.Children.Any())
                        Main.hidePlayerCraftingMenu = true;

                    State.Draw(Main.spriteBatch);
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }

        var cursorIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor"));
        if (cursorIndex != -1)
        {
            layers.Insert(cursorIndex, new LegacyGameInterfaceLayer(
                "MSEnchant: Hacked Item Tooltip",
                delegate
                {
                    while (Global.DrawTooltipQueue.TryDequeue(out var item))
                    {
                        item.DrawTooltipHacked();
                    }

                    return true;
                }, InterfaceScaleType.UI));
        }

        var inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (inventoryIndex != -1)
        {
            layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                "MSEnchant: Inventory",
                delegate
                {
                    if (!Main.playerInventory)
                        return true;

                    InventoryState.Draw(Main.spriteBatch);
                    return true;
                }, InterfaceScaleType.UI));
        }
    }
}
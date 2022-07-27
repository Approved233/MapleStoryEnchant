using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.Models;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace MSEnchant.Globals;

public class MSEnchantItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    private int _starForce;

    public int StarForce
    {
        get => _starForce;
        set => _starForce = Math.Clamp(value, 0, MaxStarForceLevel);
    }

    public bool Destroyed;

    public Item Item { get; private set; }

    public StarForceStatAttribute[] BonusAttributes;

    public const int MaxStarRow = 15;

    public ItemType ItemType => Item.GetItemType();

    public bool HasStarForce
    {
        get
        {
            if (Item.IsNullOrAir() || Item.questItem)
                return false;

            return ItemType != ItemType.Unknown;
        }
    }

    public int MaxStarForceLevel
    {
        get
        {
            if (!HasStarForce)
                return 0;

            if (Destroyed)
                return StarForce;

            var setting = FindNearRareLevelSetting();
            return setting.MaxStar;
        }
    }

    public bool IsReachedMaxStarForce => StarForce >= MaxStarForceLevel;

    public StarForceChanceSetting? FindNextLevelChanceSetting()
    {
        if (Global.StarForceChanceSettings.ElementAtOrDefault(StarForce /* Next Level */)?.Clone() is not
            StarForceChanceSetting setting)
        {
            return null;
        }

        return setting;
    }

    public StarForceRareLevelSetting FindNearRareLevelSetting()
    {
        var rarity = Item.OriginalRarity;
        if (Item.master || Item.expert)
            rarity = (int)Math.Round(Item.GetStoreValue() / 75000.0);

        var rare = Math.Clamp(rarity, Global.StarForceRareLevelSettings.Keys.Min(),
            Global.StarForceRareLevelSettings.Keys.Max());

        if (Global.StarForceRareLevelSettings.TryGetValue(rare, out var setting))
            return setting;

        return Global.StarForceRareLevelSettings.FirstOrDefault(p => p.Key > rare).Value;
    }

    public int StarRows
    {
        get
        {
            var star = MaxStarForceLevel;
            if (star <= MaxStarRow)
                return 1;

            return star / MaxStarRow + 1;
        }
    }

    public StarForceStatAttribute[] CalculateBonusAttributes(int min = 0, int? max = null)
    {
        max ??= StarForce;
        if (max < 1)
            return Array.Empty<StarForceStatAttribute>();

        var setting = FindNearRareLevelSetting();
        var attributes = new List<StarForceStatAttribute>();
        var type = ItemType;
        if (type == ItemType.Weapon || type == ItemType.Armor)
        {
            var starDamage = 0;
            for (var i = min; i < max; i++)
            {
                double bonus = 0;
                if (i < 16)
                {
                    if (type == ItemType.Weapon)
                    {
                        bonus = (int)Math.Ceiling((Item.OriginalDamage + starDamage) / 50.0);
                        if (Item.OriginalDamage > 75)
                            bonus *= Global.AttributeStatBonus[StarForceAttributeType.Damage];
                    }
                    else
                    {
                        bonus = i < 6 ? 2 : 3;
                        bonus *= Global.AttributeStatBonus[StarForceAttributeType.Damage];
                    }
                }
                else
                {
                    var bonusIndex = i - 15;
                    bonus = type == ItemType.Armor
                        ? setting.BonusDamageArmor[bonusIndex]
                        : setting.BonusDamageWeapon[bonusIndex];
                    
                    bonus *= type == ItemType.Armor 
                        ? Global.AttributeStatBonus[StarForceAttributeType.Defense]
                        : Global.AttributeStatBonus[StarForceAttributeType.Damage];
                }

                starDamage += Math.Max((int)bonus, 1);
            }

            attributes.Add(new StarForceStatAttribute(StarForceAttributeType.Damage, starDamage));
        }

        if (type == ItemType.Armor || type == ItemType.Accessory)
        {
            var starDefence = 0;
            for (var i = min; i < max; i++)
            {
                starDefence += Math.Max(1, (i + 1) / 4);
            }

            attributes.Add(new StarForceStatAttribute(StarForceAttributeType.Defense, starDefence));
        }

        return attributes.ToArray();
    }

    public StarForceStatAttribute[] GetStarForceBonusAttributes(int? level = null)
    {
        return CalculateBonusAttributes(0, StarForce);
    }

    public override GlobalItem Clone(Item from, Item to)
    {
        var clone = (MSEnchantItem)base.Clone(@from, to);
        clone.StarForce = StarForce;
        clone.BonusAttributes = BonusAttributes;
        clone.Destroyed = Destroyed;
        return clone;
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        _starForce = tag.GetInt("StarForce");
        Destroyed = tag.GetBool("Destroyed");
        TryInitData(item);
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        tag["StarForce"] = StarForce;
        tag["Destroyed"] = Destroyed;
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        TryInitData(item);

        writer.Write(_starForce);

        writer.Write(BonusAttributes.Length);
        foreach (var attribute in BonusAttributes)
        {
            writer.Write((int)attribute.Type);
            writer.Write(attribute.Value);
        }

        writer.Write(Destroyed);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        _starForce = reader.ReadInt32();

        var attributeCount = reader.ReadInt32();
        var bonusAttributes = new StarForceStatAttribute[attributeCount];
        for (var i = 0; i < attributeCount; i++)
        {
            var type = (StarForceAttributeType)reader.ReadInt32();
            var value = reader.ReadDouble();
            bonusAttributes[i] = new StarForceStatAttribute(type, value);
        }

        BonusAttributes = bonusAttributes;

        Destroyed = reader.ReadBoolean();
    }

    public void TryInitData(Item item)
    {
        if (!Item.IsNullOrAir())
            return;

        Item = item;
        StarForce = _starForce;
        BonusAttributes = GetStarForceBonusAttributes();
    }

    public int DowngradeMinLevel => StarForce switch
    {
        >= 20 => 20,
        >= 15 => 15,
        >= 10 => 10,
        _ => 0
    };

    public StarForceTransmissionResult TryTransmission(Item targetItem, out Item beforeItem)
    {
        beforeItem = Item?.Clone();

        if (Item.IsNullOrAir() || targetItem.type != Item!.type || !targetItem.IsItemValidInUIAction() ||
            !Item.IsItemValidInUIAction())
            return StarForceTransmissionResult.NoResult;

        targetItem.TurnToAir();
        
        if (RollChance(60))
        {
            Destroyed = false;
            StarForce = 6; // CMS setting
            UpdateData();
            
            return StarForceTransmissionResult.Success;
        }

        return StarForceTransmissionResult.Failed;
    }

    public StarForceEnchantResult TryEnchant(StarForceEnchantSetting setting, out Item beforeItem)
    {
        beforeItem = setting.Item.Clone();
        var item = setting.Item;
        var msItem = item.GetEnchantItem();
        if (msItem == null)
            return StarForceEnchantResult.NoResult;

        if (setting.Protect && !setting.Chance.AllowProtect)
            setting.Protect = false;

        if (setting.Protect)
            setting.Chance.FailDestroy = 0;

        if (RollChance(setting.SuccessRate))
        {
            msItem.StarForce += 1;
            return StarForceEnchantResult.Success;
        }

        if (RollChance(setting.Chance.FailDestroy))
        {
            msItem.Destroyed = true;
            return StarForceEnchantResult.Destroy;
        }

        var doDowngrade = false;

        if (setting.Chance.FailDowngrade > 0 && setting.Chance.FailKeep > 0)
        {
            if (RollChance(setting.Chance.FailDowngrade))
                doDowngrade = true;
        }
        else
        {
            if (setting.Chance.FailDowngrade > 0)
                doDowngrade = true;
        }

        if (doDowngrade)
        {
            msItem.StarForce = Math.Max(DowngradeMinLevel, msItem.StarForce - 1);
            return StarForceEnchantResult.Downgrade;
        }

        return StarForceEnchantResult.Failed;
    }

    private bool RollChance(double number)
    {
        if (number <= 0)
            return false;

        var chance = Main.rand.NextDouble() * 100.0;
        return chance < number;
    }

    public void UpdateData()
    {
        BonusAttributes = GetStarForceBonusAttributes();
    }

    private static Vector2? placeHolderSize;

    private const int TooltipMinWidth = 261;

    public Color DrawColor => Destroyed ? Global.TraceItemDrawColor : Color.White;

    public override bool CanResearch(Item item)
    {
        TryInitData(item);
        if (Destroyed)
            return false;

        return base.CanResearch(item);
    }

    public override bool PreReforge(Item item)
    {
        TryInitData(item);
        if (Destroyed)
            return false;

        return base.PreReforge(item);
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        TryInitData(item);
        if (Destroyed)
            return false;

        return base.CanEquipAccessory(item, player, slot, modded);
    }

    public override bool CanRightClick(Item item)
    {
        TryInitData(item);
        if (Destroyed)
            return false;

        return base.CanRightClick(item);
    }

    public override void HoldItem(Item item, Player player)
    {
        TryInitData(item);
        UpdateData();
    }

    public override bool CanUseItem(Item item, Player player)
    {
        TryInitData(item);
        if (Destroyed)
            return false;

        return base.CanUseItem(item, player);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor,
        Color itemColor, Vector2 origin, float scale)
    {
        TryInitData(item);
        if (!Destroyed)
            return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

        var texture2D = TextureAssets.Item[item.type].Value;

        spriteBatch.UseNonPremultiplied(() =>
        {
            spriteBatch.Draw(texture2D, position, frame, DrawColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
        });

        return false;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        TryInitData(item);

        var maxStarForce = MaxStarForceLevel;
        if (maxStarForce == 0)
            return;

        var rows = Math.Max(2, StarRows);

        const string placeHolderText = "　";
        placeHolderSize ??= ChatManager.GetStringSize(FontAssets.MouseText.Value, placeHolderText, Vector2.One);

        var count = (int)Math.Ceiling(TooltipMinWidth / placeHolderSize.Value.X);

        var textLines = new List<string>();

        for (var i = 0; i < rows; i++)
        {
            textLines.Add(string.Join("", Enumerable.Repeat(placeHolderText, count)));
        }

        var tooltip = new TooltipLine(Mod, string.Empty, string.Join("\n", textLines));
        tooltips.Insert(0, tooltip);

        var tooltipSize = tooltips
            .Select(l => ChatManager.GetStringSize(FontAssets.MouseText.Value, l.Text, Vector2.One)).ToArray();
        var maxWidth = tooltipSize.Max(a => a.X);

        var setting = new TooltipSetting
        {
            Type = TooltipType.StarForce,
            MaxWidth = maxWidth,
            MaxStarForce = maxStarForce,
            StarForceRows = rows
        };

        var text = "~" + Convert.ToHexString(MemoryHelper.StructureToByteArray(setting));

        typeof(TooltipLine).GetField("Name", BindingFlags.Instance | BindingFlags.Public)!.SetValue(tooltip, text);

        if (StarForce > 0)
        {
            var damageOrDefenseRowIndex =
                tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name is "Defense" or "Damage");
            var type = ItemType;
            var bonusTypes = type switch
            {
                ItemType.Armor => new[] { StarForceAttributeType.Damage, StarForceAttributeType.Defense },
                ItemType.Accessory => new[] { StarForceAttributeType.Defense },
                ItemType.Weapon => new[] { StarForceAttributeType.Damage },
                _ => throw new ArgumentOutOfRangeException()
            };

            foreach (var bonusType in bonusTypes)
            {
                var attribute = BonusAttributes.FirstOrDefault(a => a.Type == bonusType);
                if (attribute == null)
                    continue;

                var baseValue = bonusType switch
                {
                    StarForceAttributeType.Damage => item.OriginalDamage,
                    StarForceAttributeType.Defense => item.OriginalDefense,
                    _ => 0
                };

                var t = new TooltipLine(Mod, "bonus", attribute.GetEquipTooltip(baseValue))
                {
                    IsModifier = false,
                    OverrideColor = new Color(102, 255, 255)
                };

                if (damageOrDefenseRowIndex == -1)
                    tooltips.Add(t);
                else
                    tooltips.Insert(damageOrDefenseRowIndex + 1, t);
            }
        }

        if (Destroyed)
        {
            var name = tooltips.FirstOrDefault(t => t.Mod == "Terraria" && t.Name is "ItemName");
            if (name != null)
            {
                name.Text += "的痕迹";
                name.OverrideColor = Color.Gray;
            }

            tooltips.Add(new TooltipLine(Mod, "DestroyText", $"\n可以将能力继承到{item.Name}上。"));
        }
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        TryInitData(item);

        if (line.Mod != Mod.Name || !line.Name.StartsWith("~"))
            return base.PreDrawTooltipLine(item, line, ref yOffset);

        TooltipSetting setting;
        try
        {
            setting = MemoryHelper.ByteArrayToStructure<TooltipSetting>(Convert.FromHexString(line.Name[1..]));
        }
        catch
        {
            setting = default;
        }

        if ((int)setting.Type == 0)
            return base.PreDrawTooltipLine(item, line, ref yOffset);

        if (setting.MaxStarForce == 0)
            return base.PreDrawTooltipLine(item, line, ref yOffset);

        var starWidth = Global.StarTexture.Width();
        var starHeight = Global.StarTexture.Height();

        var pos = new Vector2(line.X - 5, line.Y + 13);
        // pos.Y += (setting.StarForceRows - 1) * starHeight;
        var size = ChatManager.GetStringSize(FontAssets.MouseText.Value, line.Text, Vector2.One);
        size.X = setting.MaxWidth + 10;

        var center = new Vector2(pos.X + size.X / 2, pos.Y);

        Main.spriteBatch.UseNonPremultiplied(() =>
        {
            const int emptyWidth = 8;
            for (var l = 0; l < setting.StarForceRows; l++)
            {
                var offset = Vector2.Zero;
                offset.Y += l * (starHeight + 8f);

                var starsInline = Math.Min(setting.MaxStarForce - l * MaxStarRow, MaxStarRow);

                var halfStar = starsInline / 2f;
                var halfStarDigit = MathF.Ceiling(Math.Abs((int)halfStar - halfStar));
                var emptySlots = (starsInline / 5f) - 1;

                if (halfStarDigit > 0f)
                    offset.X += halfStarDigit;

                offset.X -= halfStar * starWidth;
                offset.X -= emptySlots * emptyWidth;

                if (emptySlots == 0)
                    offset.X -= emptyWidth;

                for (var i = 0; i < starsInline; i++)
                {
                    if (i % 5 == 0)
                        offset.X += emptyWidth;

                    var star = l * MaxStarRow + i;

                    Texture2D starTexture;
                    if (StarForce > star)
                        starTexture = Global.StarTexture.Value;
                    else
                        starTexture = Global.GrayStarTexture.Value;

                    Main.spriteBatch.Draw(starTexture, new Vector2(center.X, center.Y) + offset, Color.White);
                    offset.X += starWidth;
                }
            }
        });

        return false;
    }
}

public enum TooltipType
{
    StarForce = 1,
    Bonus = 2
}

public struct TooltipSetting
{
    public TooltipType Type;
    public float MaxWidth;
    public int StarForceRows;
    public int MaxStarForce;
}

public enum ItemType
{
    Unknown,
    Weapon,
    Armor,
    Accessory
}
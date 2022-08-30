using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Effects;
using MSEnchant.Models;
using MSEnchant.UI.Control;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant;

public class Global
{
    public delegate void VoidDelegate();

    public delegate void PopupButtonClickDelegate(MSButton button, string name);

    public static ModKeybind EnableEnchantUIKey;
    
    public static Color TraceItemDrawColor => new(76, 135, 230, 125);
    
    public static MSAnimationImage SuccessEffect;
    public static MSAnimationImage DestroyEffect;
    public static MSAnimationImage FailEffect;

    public static readonly ConcurrentQueue<UIElement> RemoveElementQueue = new();

    public static readonly ConcurrentQueue<UIElement> AppendElementQueue = new();

    public static readonly ConcurrentQueue<Item> DrawTooltipQueue = new();

    public static readonly ConcurrentQueue<Action> UpdateScheduleQueue = new();

    public const int MaxStars = 25;

    public static ILog Logger;
    
    public static MSEnchant Mod;

    public static readonly Dictionary<string, UISetting> CultureUISettings =
        new Dictionary<string, UISetting>();
    
    public static readonly Dictionary<int, ContentSamples.CreativeHelper.ItemGroup> ItemGroupCache = new();

    public static UISetting CurrentCultureUISetting
    {
        get
        {
            var culture = LanguageManager.Instance.ActiveCulture.Name;
            if (CultureUISettings.TryGetValue(culture, out var settings))
                return settings;

            return FallbackCultureUISetting;
        }
    }

    public static UISetting FallbackCultureUISetting => CultureUISettings["zh-Hans"];
    
    public static Asset<DynamicSpriteFont> TextBold => CurrentCultureUISetting.Font.BoldAsset;
    public static Asset<DynamicSpriteFont> TextRegular => CurrentCultureUISetting.Font.RegularAsset;

    public static Asset<Texture2D> StarTexture;
    public static Asset<Texture2D> GrayStarTexture;
    public static Asset<Texture2D> SuperiorStarTexture;

    public static readonly List<StarForceScrollLootSetting> StarScrollLootSettings = new List<StarForceScrollLootSetting>();

    public static Texture2D PixelTexture;

    public static SoundStyle DragEndSound;

    public static readonly Dictionary<string, Asset<Texture2D>> TooltipTextures = new();

    public static readonly Dictionary<EffectType, WorldEffectAnimation> WorldEffectAnimations = new();

    public static readonly Dictionary<StarForceAttributeType, float> AttributeStatBonus = new()
    {
        {StarForceAttributeType.Damage, 0.9f},
        {StarForceAttributeType.Defense, 0.9f},
    };

    // CMS Chance
    public static readonly StarForceChanceSetting[] StarForceChanceSettings =
    {
        new(15), // 1
        new(20),
        new(25),
        new(25),
        new(30), // 5
        new(35),
        new(40),
        new(45),
        new(50),
        new(55), // 10
        new(60), // 11
        new(failDowngrade: 68.6, failDestroy: 1.4), // 12
        new(failDowngrade: 72.7, failDestroy: 2.3, allowProtect: true),
        new(failDowngrade: 72.0, failDestroy: 3.0, allowProtect: true),
        new(failDowngrade: 76.0, failDestroy: 4.0, allowProtect: true), // 15
        new(failDowngrade: 75.2, failDestroy: 4.8, allowProtect: true), // 16
        new(failDowngrade: 74.4, failDestroy: 5.6), // 17
        new(failDowngrade: 73.6, failDestroy: 6.4), // 18
        new(failDowngrade: 77.3, failDestroy: 7.7), // 19
        new(failDowngrade: 76.5, failDestroy: 8.5), // 20
        new(failDowngrade: 74.8, failDestroy: 10.2), // 21
        new(failDowngrade: 76.5, failDestroy: 13.5), // 22
        new(failDowngrade: 77.6, failDestroy: 19.4), // 23
        new(failDowngrade: 68.6, failDestroy: 29.4), // 24
        new(failDowngrade: 59.4, failDestroy: 39.6), // 25
    };

    public static readonly Dictionary<int, StarForceRareLevelSetting> StarForceRareLevelSettings = new()
    {
        { ItemRarityID.Blue, new StarForceRareLevelSetting(5, new[] { 5, 5, 5, 6, 6 }) },
        { ItemRarityID.Green, new StarForceRareLevelSetting(5, new[] { 15, 15, 15, 16, 16 }) },
        { ItemRarityID.Orange, new StarForceRareLevelSetting(8, new[] { 36, 36, 37, 38, 51, 58, 60, 60 }) },
        { ItemRarityID.LightRed, new StarForceRareLevelSetting(10, new[] { 46, 46, 47, 48, 65, 72, 74, 74, 89, 90 }) },
        {
            ItemRarityID.Pink,
            new StarForceRareLevelSetting(15,
                new[] { 57, 57, 58, 59, 76, 83, 85, 85, 102, 103, 144, 145, 162, 163, 165 })
        },
        {
            ItemRarityID.LightPurple,
            new StarForceRareLevelSetting(20,
                new[] { 60, 60, 61, 62, 79, 86, 88, 88, 105, 106, 147, 148, 167, 168, 170, 179, 197, 198, 201, 202 },
                new[]
                {
                    6, 7, 7, 8, 9
                }, new[]
                {
                    7, 8, 9, 10, 11
                })
        },
        {
            ItemRarityID.Lime, new StarForceRareLevelSetting(25, new[]
            {
                70, 70, 71, 72, 91, 98, 100, 100, 119, 120, 161, 162, 183, 184, 186, 195, 215, 216, 219, 220, 272, 272,
                332, 365, 400
            }, new[]
            {
                7, 8, 8, 9, 10, 11, 12, 30, 31, 32
            }, new[]
            {
                8, 9, 10, 11, 12, 13, 15, 17, 19, 21
            })
        },
        {
            ItemRarityID.Yellow, new StarForceRareLevelSetting(25, new[]
            {
                93, 93, 94, 95, 126, 154, 156, 156, 187, 188, 250, 251, 284, 285, 287, 317, 349, 350, 353, 354, 439,
                441, 532, 584, 640
            }, new[]
            {
                8, 9, 9, 10, 11, 12, 13, 31, 32, 33
            }, new[]
            {
                9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 22
            })
        },
        {
            ItemRarityID.Cyan, new StarForceRareLevelSetting(25, new[]
            {
                112, 112, 113, 114, 185, 187, 187, 187, 258, 259, 300, 301, 341, 342, 344, 380, 419, 420, 424, 425, 439,
                439, 532, 654, 715
            }, new[]
            {
                9, 9, 10, 11, 12, 13, 14, 32, 33, 34
            }, new []
            {
                10, 11, 12, 13, 14, 15, 17, 19, 21, 23
            })
        },
        {
            ItemRarityID.Red, new StarForceRareLevelSetting(25, new[]
            {
                212, 212, 213, 214, 352, 355, 355, 355, 490, 492, 570, 571, 648, 649, 651, 724, 798, 800, 807, 808,
                1003, 1008, 1224, 1504, 1645
            }, new[]
            {
                13, 13, 14, 14, 15, 16, 17, 34, 35, 36
            }, new []
            {
                12, 13, 14, 15, 16, 17, 19, 21, 23, 25
            })
        },
    };
}
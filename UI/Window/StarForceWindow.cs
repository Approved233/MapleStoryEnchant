using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MSEnchant.Helper;
using MSEnchant.Items;
using MSEnchant.Models;
using MSEnchant.UI.Component;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.UI;
using Vector2 = System.Numerics.Vector2;

namespace MSEnchant.UI.Window;

public class StarForceWindow : MSWindow
{
    public override string BaseTexturePath => "MSEnchant/Assets/enchantUI.tab_hyper";

    public override Type[] LinkWindowTypes => new[]
    {
        typeof(MainWindow),
        typeof(MiniGameWindow),
        typeof(TransmissionWindow)
    };

    protected MSImage LayerText;

    protected MSText CurrentStarText;

    protected MSText NextStarText;

    protected MSText DetailText;

    protected MSCheckBox DisableMiniGameCheckBox;

    protected MSCheckBox UseProtectScrollCheckBox;

    protected MSButton StartEnchantButton;

    protected MSButton CancelButton;

    protected MSItem EnchantItemComponent;

    protected MSImage StarFlag;

    protected MSAnimationImage HyperEffect;

    public MSText CostText;

    public Item EnchantItem
    {
        get => EnchantSetting.Item;
        set
        {
            EnchantSetting.Item = value;
            UpdateEnchantItem();
        }
    }

    public Item DisplayItem
    {
        get => EnchantItemComponent.DisplayItem;
        set => EnchantItemComponent.DisplayItem = value;
    }

    protected string ConfirmPopupContent;

    public readonly StarForceEnchantSetting EnchantSetting = new StarForceEnchantSetting();

    protected bool CanEnchant => EnchantItem.IsItemValidInUIAction() && CanPayCosts;

    protected bool CanPayCosts = false;

    public bool Processing { get; protected set; } = false;

    protected override void DoInit()
    {
        AddBackGroundTexture("backgrnd2", 11, 22);

        AddCloseButton(322, tooltip: "结束强化。");

        var tabScrollButton = new MSButton("MSEnchant/Assets/enchantUI.buttontab_scroll", 17, 29)
        {
            Disabled = true
        };
        Append(tabScrollButton);

        var tabHyperButton = new MSButton("MSEnchant/Assets/enchantUI.tab_hyper.buttontab_hyper", 122, 29)
        {
            Tooltip = "使用星星对已消耗所有可升级次数的装备进行强化。"
        };
        tabHyperButton.SetImage("MSEnchant/Assets/enchantUI.tab_hyper.buttontab_hyper.normal");
        Append(tabHyperButton);

        var tabTransmissionButton = new MSButton("MSEnchant/Assets/enchantUI.buttontab_transmission", 226, 29)
        {
            Tooltip = "将装备痕迹具有的潜能继承到装备上。"
        };
        tabTransmissionButton.OnClick += (evt, element) => { SwitchTransmissionTab(); };
        Append(tabTransmissionButton);

        Append(LayerText = new MSImage(string.Empty, 117, 58));

        Append(EnchantItemComponent = new MSItem(68, 68, 38, 101));

        Append(StarFlag = new MSImage(string.Empty, 18, 78));

        Append(HyperEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.hyperEffect", new[]
        {
            new MSFrameData(90, 14, 78),
            new MSFrameData(90, -44, 20),
            new MSFrameData(90, -9, 55),
            new MSFrameData(90, 6, 70),
            new MSFrameData(90, 12, 76),
            new MSFrameData(90, 11, 76),
            new MSFrameData(90, 11, 76),
            new MSFrameData(90, 48, 122)
        })
        {
            Visible = false
        });

        var layerArrowTexture = ModContent.Request<Texture2D>("MSEnchant/Assets/enchantUI.tab_hyper.layerarrow",
            AssetRequestMode.ImmediateLoad);

        var list = new MSList();
        list.Width.Set(180f, 0f);
        list.Height.Set(95f, 0f);
        list.ListPadding = 5f;
        list.Left.Set(168 - layerArrowTexture.Width() - 14, 0f);
        list.Top.Set(92, 0f);

        var firstLine = new UIElement();
        firstLine.SetPadding(0);
        firstLine.Width.Set(0, 1f);
        firstLine.Height.Set(layerArrowTexture.Height() - 8f, 0f);

        const float fontScale = 0.5f;

        firstLine.Append(CurrentStarText = new MSText(string.Empty, fontScale - 0.03f)
        {
            Font = Global.TextBold
        });

        var arrow = new MSImage(layerArrowTexture, 168 - list.Left.Pixels,
            89 - list.Top.Pixels)
        {
            DrawAlpha = false
        };
        firstLine.Append(arrow);

        firstLine.Append(NextStarText =
            new MSText(string.Empty, fontScale - 0.03f, left: arrow.Left.Pixels + arrow.Width.Pixels + 6,
                top: CurrentStarText.Top.Pixels)
            {
                Font = Global.TextBold
            });
        list.Add(firstLine);

        list.Add(DetailText = new MSText(string.Empty, fontScale)
        {
            Font = Global.TextBold
        });

        Append(list);

        var scrollbar = new MSScrollbar("MSEnchant/Assets/enchantUI.tab_hyper.scroll", 315, 85)
        {
            ScrollPower =
            {
                X = 0,
                Y = 96 - 85
            }
        };
        list.SetScrollbar(scrollbar);

        Append(scrollbar);

        Append(DisableMiniGameCheckBox = new MSCheckBox("MSEnchant/Assets/enchantUI.tab_hyper.checkBox1", 151, 200)
        {
            Tooltip = "接触时，可以更快地进行星之力强化，\n但无法获得通过‘抓星星’获取的成功率提高效果。"
        });

        Append(UseProtectScrollCheckBox = new MSCheckBox("MSEnchant/Assets/enchantUI.tab_hyper.checkBox1", 308, 200)
        {
            Disabled = true,
            Tooltip = "用星之力12星至16星之间的装备来尝试进行星之力强化时，\n可额外消耗星星，令破坏率变为0%。\n极真装备无法使用。"
        });
        UseProtectScrollCheckBox.OnClick += (evt, element) => { UpdateEnchantItem(); };

        Append(CostText = new MSText(string.Empty, fontScale, left: 204, top: 224)
        {
            Font = Global.TextBold
        });

        Append(StartEnchantButton = new MSButton("MSEnchant/Assets/enchantUI.tab_hyper.buttonenchantStart", 83, 252)
        {
            Disabled = true,
            Tooltip = "使用星星，对升级次数全部耗尽的装备进行强化。"
        });
        StartEnchantButton.OnClick += (evt, element) =>
        {
            if (Processing)
                return;

            Processing = true;

            var popup = new MSEnchantPopup(ConfirmPopupContent);
            popup.OnButtonClick += (button, name) =>
            {
                if (name != "buttonconfirm")
                {
                    Processing = false;
                    return;
                }

                void DoStartEnchant(bool success)
                {
                    if (!StartEnchant(success))
                        Processing = false;
                }

                if (!DisableMiniGameCheckBox.Checked)
                {
                    var miniGameWindow = State.ShowWindowCenter<MiniGameWindow>();
                    miniGameWindow.DisplayItem = EnchantItem;
                    miniGameWindow.OnMiniGameEnd += () => { DoStartEnchant(miniGameWindow.IsSuccess); };
                }
                else
                    DoStartEnchant(false);
            };
            State.ShowPopupCenter(popup);
        };

        Append(CancelButton = new MSButton("MSEnchant/Assets/enchantUI.buttoncancel", 174, 252)
        {
            Disabled = true,
            Tooltip = "回到初始画面。"
        });
        CancelButton.OnClick += (e, element) => { EnchantItem = null; };

        OnMouseUp += (e, element) =>
        {
            var item = Main.mouseItem;
            if (ItemHelper.HandleItemPick(item) && EnchantItem != item)
                EnchantItem = item;
        };

        OnUpdate += element =>
        {
            IsLocked = Processing;
            StartEnchantButton.Disabled = !CanEnchant || State.HasVisibleAlwaysTop;
            if (!EnchantItem.IsItemValidInUIAction())
                EnchantItem = null;

            if (EnchantItem != null)
            {
                var costBefore = CanPayCosts;
                CanPayCosts = Main.LocalPlayer.HasItem<StarItem>(EnchantSetting.Costs);

                if (costBefore != CanPayCosts)
                    UpdateEnchantItem();
            }
        };
    }

    protected void SwitchTransmissionTab()
    {
        if (!EnchantItem.IsItemValidInUIAction())
            return;

        var msItem = EnchantItem.GetEnchantItem();
        if (msItem == null)
            return;

        MSEnchantUI.Instance.ReplaceWindow<TransmissionWindow>(this, window => { window.SetItem(EnchantItem); });
    }

    protected bool StartEnchant(bool miniGameSuccess)
    {
        if (!CanEnchant)
        {
            State.ShowNoticeCenter("正在处理别的请求。\n请稍后再试。");
            return false;
        }

        var msItem = EnchantItem.GetEnchantItem();
        if (msItem == null)
        {
            State.ShowNoticeCenter("发生未知错误。");
            return false;
        }

        EnchantSetting.IsMiniGameSuccess = miniGameSuccess;

        var result = msItem.TryEnchant(EnchantSetting, out var beforeItem);
        if (result == StarForceEnchantResult.NoResult)
        {
            State.ShowNoticeCenter("发生未知错误。");
            return false;
        }

        if (!Main.LocalPlayer.CostItem<StarItem>(EnchantSetting.Costs))
        {
            State.ShowNoticeCenter("发生未知错误。");
            return false;
        }

        msItem.UpdateData();

        HyperEffect.RegisterAnimationEndOnce(element =>
        {
            Processing = false;
            var afterItem = EnchantSetting.Item;
            UpdateEnchantItem();

            var content = result switch
            {
                StarForceEnchantResult.Success => "强化成功。",
                StarForceEnchantResult.Downgrade => "强化失败，强化阶段下降。",
                StarForceEnchantResult.Destroy => "强化失败，装备损坏。",
                StarForceEnchantResult.Failed => "强化失败。",
                _ => ""
            };

            var animation = result switch
            {
                StarForceEnchantResult.Destroy => Global.DestroyEffect,
                StarForceEnchantResult.Failed or StarForceEnchantResult.Downgrade => Global.FailEffect,
                StarForceEnchantResult.Success => Global.SuccessEffect,
                _ => null
            };

            var sound = result switch
            {
                StarForceEnchantResult.Destroy => "MSEnchant/Assets/EnchantDestroyed",
                StarForceEnchantResult.Failed or StarForceEnchantResult.Downgrade => "MSEnchant/Assets/EnchantFail",
                StarForceEnchantResult.Success => "MSEnchant/Assets/EnchantSuccess",
                _ => null
            };

            var popup = new MSEnchantResultPopup(content)
            {
                DisplayItemBefore = beforeItem,
                DisplayItemAfter = afterItem
            };
            if (result == StarForceEnchantResult.Destroy)
                popup.ItemAfterDrawColor = Global.TraceItemDrawColor;

            popup.OnButtonClick += (button, name) =>
            {
                if (name != "buttonconfirm")
                    return;

                animation?.Stop();

                if (msItem.Destroyed || msItem.IsReachedMaxStarForce)
                {
                    EnchantItem = null;
                    if (!msItem.Destroyed)
                        AlertPopup("强化成功了。\n装备已达到强化上限，\n无法继续强化。");
                }
            };
            State.ShowPopupCenter(popup);

            if (sound != null)
                SoundEngine.PlaySound(new SoundStyle(sound));

            if (animation != null)
                State.PlayAnimationCenter(animation);
        });

        HyperEffect.Play();
        SoundEngine.PlaySound(new SoundStyle("MSEnchant/Assets/Enchant"));
        return true;
    }

    protected void AlertPopup(string content)
    {
        var popup = new MSEnchantAlertPopup(content);
        State.ShowPopupCenter(popup);
    }

    protected void UpdateEnchantItem()
    {
        var msItem = EnchantItem?.GetEnchantItem();
        if (msItem == null)
        {
            MSEnchantUI.Instance.ReplaceWindow<MainWindow>(this);
            return;
        }

        if (msItem.Destroyed || msItem.IsReachedMaxStarForce)
        {
            SwitchTransmissionTab();
            return;
        }

        DisplayItem = EnchantItem;

        var starForce = msItem.StarForce;

        var flagTexture = starForce switch
        {
            >= 20 => "MSEnchant/Assets/enchantUI.tab_hyper.layerstar20",
            >= 15 => "MSEnchant/Assets/enchantUI.tab_hyper.layerstar15",
            >= 10 => "MSEnchant/Assets/enchantUI.tab_hyper.layerstar10",
            _ => null
        };
        var minDowngradeStarForce = starForce switch
        {
            >= 20 => 20,
            >= 15 => 15,
            >= 10 => 10,
            _ => 0
        };
        StarFlag.Tooltip = $"达到了{minDowngradeStarForce}星！即使星之力强化失败，强化阶段下\n降，也不会降到{minDowngradeStarForce}星以下。";
        StarFlag.SetTexture(flagTexture);

        CurrentStarText.Text = $"{msItem.StarForce}星";
        NextStarText.Text = $"{msItem.StarForce + 1}星";

        EnchantSetting.Chance = msItem.FindNextLevelChanceSetting();
        if (EnchantSetting.Chance == null)
        {
            State.ShowNoticeCenter("无法获取星之力配置，请稍后再试。");
            return;
        }

        var rareSetting = msItem.FindNearRareLevelSetting();

        if (msItem.StarForce is 10 or 15 or 20 && EnchantSetting.Chance.FailDowngrade > 0)
        {
            EnchantSetting.Chance.FailKeep = EnchantSetting.Chance.FailDowngrade;
            EnchantSetting.Chance.FailDowngrade = 0;
        }

        UseProtectScrollCheckBox.Disabled = !EnchantSetting.Chance.AllowProtect;
        if (UseProtectScrollCheckBox.Checked && !EnchantSetting.Chance.AllowProtect)
        {
            UseProtectScrollCheckBox.Checked = false;
            State.ShowNoticeCenter("无法使用防损坏，已解除。");
        }

        EnchantSetting.Protect = UseProtectScrollCheckBox.Checked && EnchantSetting.Chance.AllowProtect;

        string layerTextTexture;
        Vector2 flagOffset;

        EnchantSetting.BaseCosts = rareSetting.Costs[msItem.StarForce];

        var confirmPopupLines = new List<string> { $"{EnchantSetting.Costs} 星星\n" };

        if (!CanPayCosts)
        {
            layerTextTexture = "MSEnchant/Assets/enchantUI.tab_hyper.layerlack_meso";
            flagOffset = new Vector2(72, 58);
        }
        else if (!EnchantSetting.Protect && EnchantSetting.Chance.FailDestroy > 0 &&
                 EnchantSetting.Chance.FailDowngrade > 0)
        {
            layerTextTexture = "MSEnchant/Assets/enchantUI.tab_hyper.layerbothways";
            flagOffset = new Vector2(53, 59);
            confirmPopupLines.Add("强化失败时，装备可能会损坏或\n强化等级下降。");
        }
        else if (EnchantSetting.Chance.FailKeep > 0 && EnchantSetting.Chance.FailDestroy > 0 && !EnchantSetting.Protect)
        {
            layerTextTexture = "MSEnchant/Assets/enchantUI.tab_hyper.layerdestroyable2";
            flagOffset = new Vector2(74, 59);
            confirmPopupLines.Add("强化失败时，装备有可能破坏。");
        }
        else if (EnchantSetting.Chance.FailDowngrade > 0)
        {
            layerTextTexture = "MSEnchant/Assets/enchantUI.tab_hyper.layerdowngradable";
            flagOffset = new Vector2(78, 59);
            confirmPopupLines.Add("强化失败时，强化等级会下降。");
        }
        else
        {
            layerTextTexture = "MSEnchant/Assets/enchantUI.tab_hyper.layerzero";
            flagOffset = new Vector2(117, 58);
        }

        LayerText.SetTexture(layerTextTexture);
        LayerText.Left.Set(flagOffset.X, 0f);
        LayerText.Top.Set(flagOffset.Y, 0f);

        confirmPopupLines.Add("是否进行强化？");
        ConfirmPopupContent = string.Join("\n", confirmPopupLines);

        var detailTextLines = new List<string>();
        detailTextLines.AddRange(EnchantSetting.Chance.DetailText);
        detailTextLines.Add(string.Empty);

        var nextAttribute = msItem.CalculateBonusAttributes(msItem.StarForce, msItem.StarForce + 1);
        detailTextLines.AddRange(nextAttribute.Select(a => a.EnchantTooltip));
        DetailText.Text = string.Join("\n", detailTextLines);
        CostText.Text = EnchantSetting.Costs.ToString();

        CancelButton.Disabled = false;

        Recalculate();
    }
}
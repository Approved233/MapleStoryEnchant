using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using MSEnchant.Helper;
using MSEnchant.Items;
using MSEnchant.Models;
using MSEnchant.UI.Component;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.UI;

namespace MSEnchant.UI.Window;

public class StarForceWindow : MSWindow
{
    public override string BaseTexturePath => "enchantUI.tab_hyper";

    public override Type[] LinkWindowTypes => new[]
    {
        typeof(MainWindow),
        typeof(MiniGameWindow),
        typeof(TransmissionWindow)
    };

    protected MSImage LayerText;

    protected MSText CurrentStarText;

    protected MSText NextStarText;

    protected MSMultiLineText DetailText;

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
            if (EnchantSetting.Item == value)
                return;
            
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

        AddCloseButton(322, tooltip: Language.GetTextValue("Mods.MSEnchant.UIText.EndEnchant"));

        var tabScrollButton = new MSButton("enchantUI.buttontab_scroll", 17, 29)
        {
            Disabled = true
        };
        Append(tabScrollButton);

        var tabHyperButton = new MSButton("enchantUI.tab_hyper.buttontab_hyper", 122, 29)
        {
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.HyperButton_Tooltip")
        };
        tabHyperButton.SetImage("enchantUI.tab_hyper.buttontab_hyper.normal");
        Append(tabHyperButton);

        var tabTransmissionButton = new MSButton("enchantUI.buttontab_transmission", 226, 29)
        {
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.TransmissionButton_Tooltip")
        };
        tabTransmissionButton.OnClick += (evt, element) => { SwitchTransmissionTab(); };
        Append(tabTransmissionButton);

        Append(LayerText = new MSImage(string.Empty));

        Append(EnchantItemComponent = new MSItem(68, 68, 38, 101));

        Append(StarFlag = new MSImage(string.Empty, 18, 78));

        Append(HyperEffect = new MSAnimationImage("enchantUI.hyperEffect", new[]
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

        var layerArrowTexture = "enchantUI.tab_hyper.layerarrow".LoadLocaleTexture(AssetRequestMode.ImmediateLoad);

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

        firstLine.Append(CurrentStarText = new MSText(Language.GetTextValue("Mods.MSEnchant.UIText.StarForceLevel", 25), fontScale - 0.03f)
        {
            Font = Global.TextBold
        });

        var arrow = new MSImage(layerArrowTexture, Math.Max(168 - list.Left.Pixels, CurrentStarText.TextSize.X),
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

        Append(list);

        var scrollbar = new MSScrollbar("enchantUI.tab_hyper.scroll", 315, 85)
        {
            ScrollPower =
            {
                X = 0,
                Y = 96 - 85
            }
        };
        list.SetScrollbar(scrollbar);
        
        list.Add(DetailText = new MSMultiLineText(string.Empty)
        {
            AlignCenter = false,
            NewLineWhenReachMaxWidth = true,
            Font = Global.TextBold,
            FontScale = fontScale,
            MaxTextWidth = scrollbar.Left.Pixels - list.Left.Pixels
        });

        Append(scrollbar);

        Append(DisableMiniGameCheckBox = new MSCheckBox("enchantUI.tab_hyper.checkBox1", 151, 200)
        {
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.DisableMiniGameCheckBox_Tooltip")
        });

        Append(UseProtectScrollCheckBox = new MSCheckBox("enchantUI.tab_hyper.checkBox1", 308, 200)
        {
            Disabled = true,
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.UseProtectScrollCheckBox_Tooltip")
        });
        UseProtectScrollCheckBox.OnClick += (evt, element) => { UpdateEnchantItem(); };

        Append(CostText = new MSText(string.Empty, fontScale, left: 204, top: 224)
        {
            Font = Global.TextBold
        });

        Append(StartEnchantButton = new MSButton("enchantUI.tab_hyper.buttonenchantStart", 83, 252)
        {
            Disabled = true,
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.StartEnchantButton_Tooltip")
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

        Append(CancelButton = new MSButton("enchantUI.buttoncancel", 174, 252)
        {
            Disabled = true,
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.CancelButton_Tooltip")
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

        State.ReplaceWindow<TransmissionWindow>(this, window => { window.SetItem(EnchantItem); });
    }

    protected bool StartEnchant(bool miniGameSuccess)
    {
        if (!CanEnchant)
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.ProcessingActions"));
            return false;
        }

        var msItem = EnchantItem.GetEnchantItem();
        if (msItem == null)
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.UnknownError"));
            return false;
        }

        EnchantSetting.IsMiniGameSuccess = miniGameSuccess;

        var result = msItem.TryEnchant(EnchantSetting, out var beforeItem);
        if (result == StarForceEnchantResult.NoResult)
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.UnknownError"));
            return false;
        }

        if (!Main.LocalPlayer.CostItem<StarItem>(EnchantSetting.Costs))
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.UnknownError"));
            return false;
        }

        msItem.UpdateData();

        HyperEffect.RegisterAnimationEndOnce(element =>
        {
            Processing = false;
            var afterItem = EnchantSetting.Item;
            UpdateEnchantItem();

            var content = Language.GetTextValue($"Mods.MSEnchant.UIText.EnchantResult_{result}");

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
                        AlertPopup(Language.GetTextValue("Mods.MSEnchant.UIText.EnchantPopup_MaxStars"));
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
            if (State.IsWindowEnabled<MiniGameWindow>(out var window) && window != this)
                window.Close(true);

            State.ReplaceWindow<MainWindow>(this);
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
            >= 20 => "enchantUI.tab_hyper.layerstar20",
            >= 15 => "enchantUI.tab_hyper.layerstar15",
            >= 10 => "enchantUI.tab_hyper.layerstar10",
            _ => null
        };
        var minDowngradeStarForce = starForce switch
        {
            >= 20 => 20,
            >= 15 => 15,
            >= 10 => 10,
            _ => 0
        };
        StarFlag.Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.StarFlag_Tooltip", minDowngradeStarForce);
        StarFlag.SetTexture(flagTexture);

        CurrentStarText.Text = Language.GetTextValue("Mods.MSEnchant.UIText.StarForceLevel", msItem.StarForce);
        NextStarText.Text = Language.GetTextValue("Mods.MSEnchant.UIText.StarForceLevel", msItem.StarForce + 1);

        EnchantSetting.Chance = msItem.FindNextLevelChanceSetting();
        if (EnchantSetting.Chance == null)
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.UnableFindStarForceSetting"));
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
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.UncheckProtectPopup"));
        }

        EnchantSetting.Protect = UseProtectScrollCheckBox.Checked && EnchantSetting.Chance.AllowProtect;

        string layerTextTexture;
        Vector2 flagOffset;

        EnchantSetting.BaseCosts = rareSetting.Costs[msItem.StarForce];

        var riskLanguageKey = string.Empty;
        if (!CanPayCosts)
        {
            layerTextTexture = "enchantUI.tab_hyper.layerlack_meso";
        }
        else if (!EnchantSetting.Protect && EnchantSetting.Chance.FailDestroy > 0 &&
                 EnchantSetting.Chance.FailDowngrade > 0)
        {
            layerTextTexture = "enchantUI.tab_hyper.layerbothways";
            riskLanguageKey = "Mods.MSEnchant.UIText.ConfirmPopup_RiskBoth";
        }
        else if (EnchantSetting.Chance.FailKeep > 0 && EnchantSetting.Chance.FailDestroy > 0 && !EnchantSetting.Protect)
        {
            layerTextTexture = "enchantUI.tab_hyper.layerdestroyable2";
            riskLanguageKey = "Mods.MSEnchant.UIText.ConfirmPopup_Destroy";
        }
        else if (EnchantSetting.Chance.FailDowngrade > 0)
        {
            layerTextTexture = "enchantUI.tab_hyper.layerdowngradable";
            riskLanguageKey = "Mods.MSEnchant.UIText.ConfirmPopup_Downgrade";
        }
        else
        {
            layerTextTexture = "enchantUI.tab_hyper.layerzero";
        }

        flagOffset = layerTextTexture.GetTextureOrigin();

        LayerText.SetTexture(layerTextTexture);
        LayerText.Left.Set(flagOffset.X, 0f);
        LayerText.Top.Set(flagOffset.Y, 0f);
        
        ConfirmPopupContent = Language.GetTextValue("Mods.MSEnchant.UIText.ConfirmPopup_Content", EnchantSetting.Costs, 
            !string.IsNullOrEmpty(riskLanguageKey) ? "\n" + Language.GetText(riskLanguageKey) : "\n");

        var detailTextLines = new List<string>();
        detailTextLines.AddRange(EnchantSetting.Chance.DetailText);
        detailTextLines.Add(string.Empty);

        var nextAttribute = msItem.CalculateBonusAttributes(msItem.StarForce, msItem.StarForce + 1);
        detailTextLines.AddRange(nextAttribute.Select(a => a.EnchantTooltip));
        DetailText.Content = string.Join("\n", detailTextLines);
        CostText.Text = EnchantSetting.Costs.ToString();

        CancelButton.Disabled = false;

        Recalculate();
    }
}
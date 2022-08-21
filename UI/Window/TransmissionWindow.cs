using System;
using Microsoft.Xna.Framework;
using MSEnchant.Helper;
using MSEnchant.Models;
using MSEnchant.UI.Component;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using Terraria;
using Terraria.Audio;
using Terraria.Localization;

namespace MSEnchant.UI.Window;

public class TransmissionWindow : MSWindow
{
    public override string BaseTexturePath => "enchantUI.tab_transmission";

    public override Type[] LinkWindowTypes => new[]
    {
        typeof(MainWindow),
        typeof(MiniGameWindow),
        typeof(StarForceWindow)
    };
    
    private Item _traceItem;

    public Item TraceItem
    {
        get => _traceItem;
        protected set
        {
            _traceItem = value;
            UpdateItem();
        }
    }

    private Item _targetItem;

    public Item TargetItem
    {
        get => _targetItem;
        protected set
        {
            _targetItem = value;
            UpdateItem();
        }
    }

    protected MSButton TransmissionButton;

    protected MSButton CancelButton;

    protected MSImage LeftText;

    protected MSImage RightText;

    protected MSImage Tips;

    protected MSItem TraceItemComponent;

    protected MSItem TargetItemComponent;

    protected MSAnimationImage TraceEffect;

    protected MSAnimationImage TransmissionEffect;

    protected int TipIndex = -1;

    protected TipOption[] TipOptions =
    {
        new("enchantUI.tab_transmission.layertip1", 4000),
        new("enchantUI.tab_transmission.layertip2", 4000),
        new("enchantUI.tab_transmission.layertip3", 4000),
        new("enchantUI.tab_transmission.layertip4", 4000)
    };

    protected DateTime NextTipTime;

    protected MSButton TabHyperButton;

    public bool Processing => TransmissionEffect.Visible;

    protected override void DoInit()
    {
        AddBackGroundTexture("backgrnd2", 11, 22);
        AddCloseButton(322, tooltip: Language.GetTextValue("Mods.MSEnchant.UIText.EndTransmission"));

        var tabScrollButton = new MSButton("enchantUI.buttontab_scroll", 17, 29)
        {
            Disabled = true
        };
        Append(tabScrollButton);

        TabHyperButton = new MSButton("enchantUI.buttontab_hyper", 122, 29)
        {
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.HyperButton_Tooltip")
        };
        TabHyperButton.OnClick += (evt, element) => { SwitchHyperTab(); };
        Append(TabHyperButton);

        var tabTransmissionButton = new MSButton("enchantUI.buttontab_transmission", 226, 29)
        {
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.TransmissionButton_Tooltip")
        };
        tabTransmissionButton.SetImage("enchantUI.tab_transmission.buttontab_transmission.normal");
        Append(tabTransmissionButton);

        Append(Tips = new MSImage(string.Empty));

        Append(LeftText = new MSImage("enchantUI.tab_transmission.layerleftText"));
        Append(RightText = new MSImage("enchantUI.tab_transmission.layerrightText"));

        Append(TransmissionButton =
            new MSButton("enchantUI.tab_transmission.buttontransmissionStart", 83, 252)
            {
                Disabled = true,
                Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.TransmissionStartButton_Tooltip")
            });
        TransmissionButton.OnClick += (e, element) =>
        {
            var popup = new MSEnchantPopup(Language.GetTextValue("Mods.MSEnchant.UIText.TransmissionPopup_Content"));
            popup.OnButtonClick += (button, name) =>
            {
                if (name != "buttonconfirm")
                    return;

                StartTransmission();
            };
            State.ShowPopupCenter(popup);
        };

        Append(CancelButton = new MSButton("enchantUI.buttoncancel", 174, 252)
        {
            Tooltip = Language.GetTextValue("Mods.MSEnchant.UIText.CancelButton_Tooltip")
        });
        CancelButton.OnClick += (e, element) =>
        {
            if (Processing)
                return;

            Clean();
        };

        Append(TraceItemComponent = new MSItem(68, 68, 55, 140)
        {
            DrawColor = Global.TraceItemDrawColor
        });

        Append(TraceEffect = new MSAnimationImage("enchantUI.tab_transmission.traceEffect", new[]
        {
            new MSFrameData(120, 62, 132),
            new MSFrameData(120, 53, 126),
            new MSFrameData(120, 54, 119),
            new MSFrameData(120, 54, 119),
            new MSFrameData(120, 54, 119),
            new MSFrameData(120, 54, 120),
            new MSFrameData(120, 54, 124),
            new MSFrameData(120, 54, 116),
            new MSFrameData(120, 54, 112),
            new MSFrameData(120, 55, 113),
            new MSFrameData(120, 56, 113),
            new MSFrameData(120, 60, 118),
            new MSFrameData(120, 62, 142),
            new MSFrameData(120, 62, 138),
        })
        {
            Visible = false,
            Loop = true
        });

        Append(TargetItemComponent = new MSItem(68, 68, 221, 140));

        Append(TransmissionEffect = new MSAnimationImage("enchantUI.transmissionEffect", new[]
        {
            new MSFrameData(120, 38, 127),
            new MSFrameData(120, 40, 127),
            new MSFrameData(120, 47, 113),
            new MSFrameData(120, 57, 113),
            new MSFrameData(120, 47, 113),
            new MSFrameData(120, 49, 113),
            new MSFrameData(120, 56, 113),
            new MSFrameData(120, 49, 113),
            new MSFrameData(120, 58, 113),
            new MSFrameData(120, 69, 113),
            new MSFrameData(120, 78, 114),
            new MSFrameData(120, 101, 118),
            new MSFrameData(120, 137, 47),
            new MSFrameData(120, 162, 65),
            new MSFrameData(120, 155, 54),
            new MSFrameData(120, 157, 56),
            new MSFrameData(120, 162, 66),
            new MSFrameData(120, 163, 67),
            new MSFrameData(120, 210, 111),
            new MSFrameData(120, 209, 114),
            new MSFrameData(120, 213, 116),
            new MSFrameData(120, 217, 119),
            new MSFrameData(120, 215, 115)
        })
        {
            Visible = false
        });

        OnMouseUp += (evt, element) =>
        {
            var item = Main.mouseItem;
            if (!ItemHelper.HandleItemPick(item))
                return;

            SetItem(item);
        };

        OnUpdate += element =>
        {
            if (DateTime.Now >= NextTipTime)
            {
                TipIndex = TipIndex + 1 >= TipOptions.Length ? 0 : TipIndex + 1;
                var currentTip = TipOptions[TipIndex];
                Tips.SetTexture(currentTip.Texture);
                var offset = currentTip.Texture.GetTextureOrigin();
                Tips.Left.Set(offset.X, 0);
                Tips.Top.Set(offset.Y, 0);
                NextTipTime = DateTime.Now.AddMilliseconds(currentTip.Delay);

                Recalculate();
            }

            TransmissionButton.Disabled = TargetItem.IsNullOrAir() || TraceItem.IsNullOrAir() || Processing;
            CancelButton.Disabled = Processing;

            if (!TraceItem.IsItemValidInUIAction())
                TraceItem = null;

            if (!TargetItem.IsItemValidInUIAction())
                TargetItem = null;

            if (!Processing && IsLocked)
                IsLocked = false;
        };
    }

    protected void StartTransmission()
    {
        if (Processing)
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.ProcessingActions"));
            return;
        }

        var traceMsItem = TraceItem?.GetEnchantItem();
        if (traceMsItem == null)
        {
            State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.UnknownError"));
            return;
        }

        IsLocked = true;

        TransmissionEffect.OnAnimationEnded += element =>
        {
            IsLocked = false;

            traceMsItem = TraceItem?.GetEnchantItem();
            if (traceMsItem == null || TargetItem.IsNullOrAir())
            {
                State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.ProcessingActions"));
                return;
            }

            var result = traceMsItem.TryTransmission(TargetItem, out var beforeItem);
            if (result == StarForceTransmissionResult.NoResult)
            {
                State.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.ProcessingActions"));
                return;
            }

            var afterItem = TraceItem;

            var content = Language.GetTextValue($"Mods.MSEnchant.UIText.TransmissionResult_{result}");

            var animation = result switch
            {
                StarForceTransmissionResult.Failed => Global.FailEffect,
                StarForceTransmissionResult.Success => Global.SuccessEffect,
                _ => null
            };

            var sound = result switch
            {
                StarForceTransmissionResult.Failed => "MSEnchant/Assets/EnchantFail",
                StarForceTransmissionResult.Success => "MSEnchant/Assets/EnchantSuccess",
                _ => null
            };

            Clean();

            var popup = new MSEnchantResultPopup(content)
            {
                DisplayItemBefore = beforeItem,
                DisplayItemAfter = afterItem,
                ItemBeforeDrawColor = Global.TraceItemDrawColor
            };
            if (result == StarForceTransmissionResult.Failed)
                popup.ItemAfterDrawColor = Global.TraceItemDrawColor;
            
            popup.OnButtonClick += (button, name) =>
            {
                if (name != "buttonconfirm")
                    return;

                animation?.Stop();
            };
            State.ShowPopupCenter(popup);

            if (sound != null)
                SoundEngine.PlaySound(new SoundStyle(sound));

            if (animation != null)
                State.PlayAnimationCenter(animation);
        };
        SoundEngine.PlaySound(new SoundStyle("MSEnchant/Assets/transmission"));
        TransmissionEffect.Play();
        TraceEffect.Visible = false;
    }

    public void Clean()
    {
        TraceItem = null;
        TargetItem = null;
    }

    protected void SwitchHyperTab()
    {
        if (TargetItem.IsNullOrAir())
            return;

        var msItem = TargetItem.GetEnchantItem();
        if (msItem == null)
            return;

        State.ReplaceWindow<StarForceWindow>(this, window => { window.EnchantItem = TargetItem; });
    }

    public void SetItem(Item item)
    {
        var msItem = item.GetEnchantItem();
        if (msItem == null)
            return;

        if (TargetItem.IsNullOrAir() && !msItem.Destroyed)
        {
            if (!TraceItem.IsNullOrAir() && item.type != TraceItem.type)
            {
                goto FAIL;
            }

            TargetItem = item;
            return;
        }

        if (TraceItem.IsNullOrAir() && msItem.Destroyed)
        {
            if (!TargetItem.IsNullOrAir() && item.type != TargetItem.type)
            {
                goto FAIL;
            }

            TraceItem = item;
            return;
        }

        FAIL:
        State.ShowNoticeCenter( Language.GetTextValue("Mods.MSEnchant.UIText.Transmission_EquipNotSame"));
    }

    protected void UpdateItem()
    {
        var msTargetItem = TargetItem.GetEnchantItem();
        TabHyperButton.Disabled = msTargetItem == null || msTargetItem.IsReachedMaxStarForce;
        LeftText.Visible = TraceItem.IsNullOrAir();
        RightText.Visible = TargetItem.IsNullOrAir();

        var traceEffectVisible = TraceEffect.Visible;
        TraceEffect.Visible = !TraceItem.IsNullOrAir();
        if (!traceEffectVisible && TraceEffect.Visible)
            TraceEffect.Play();

        TraceItemComponent.DisplayItem = TraceItem;
        TargetItemComponent.DisplayItem = TargetItem;

        if (TargetItem.IsNullOrAir() && TraceItem.IsNullOrAir())
            State.ReplaceWindow<MainWindow>(this);
    }

    public struct TipOption
    {
        public string Texture;
        public int Delay;

        public TipOption(string texture, int delay)
        {
            Texture = texture;
            Delay = delay;
        }
    }
}
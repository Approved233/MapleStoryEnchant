using System;
using Microsoft.Xna.Framework;
using MSEnchant.Helper;
using MSEnchant.Models;
using MSEnchant.UI.Component;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using Terraria;
using Terraria.Audio;

namespace MSEnchant.UI.Window;

public class TransmissionWindow : MSWindow
{
    public override string BaseTexturePath => "MSEnchant/Assets/enchantUI.tab_transmission";

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
        new TipOption("MSEnchant/Assets/enchantUI.tab_transmission.layertip1", 4000, new Vector2(46, 63)),
        new TipOption("MSEnchant/Assets/enchantUI.tab_transmission.layertip2", 4000, new Vector2(70, 62)),
        new TipOption("MSEnchant/Assets/enchantUI.tab_transmission.layertip3", 4000, new Vector2(89, 67)),
        new TipOption("MSEnchant/Assets/enchantUI.tab_transmission.layertip4", 4000, new Vector2(38, 62))
    };

    protected DateTime NextTipTime;

    protected MSButton TabHyperButton;

    public bool Processing => TransmissionEffect.Visible;

    protected override void DoInit()
    {
        AddBackGroundTexture("backgrnd2", 11, 22);
        AddCloseButton(322, tooltip: "结束继承。");

        var tabScrollButton = new MSButton("MSEnchant/Assets/enchantUI.buttontab_scroll", 17, 29)
        {
            Disabled = true
        };
        Append(tabScrollButton);

        TabHyperButton = new MSButton("MSEnchant/Assets/enchantUI.buttontab_hyper", 122, 29)
        {
            Tooltip = "使用星星对已消耗所有可升级次数的装备进行强化。"
        };
        TabHyperButton.OnClick += (evt, element) => { SwitchHyperTab(); };
        Append(TabHyperButton);

        var tabTransmissionButton = new MSButton("MSEnchant/Assets/enchantUI.buttontab_transmission", 226, 29)
        {
            Tooltip = "将装备痕迹具有的潜能继承到装备上。"
        };
        tabTransmissionButton.SetImage("MSEnchant/Assets/enchantUI.tab_transmission.buttontab_transmission.normal");
        Append(tabTransmissionButton);

        Append(Tips = new MSImage(string.Empty));

        Append(LeftText = new MSImage("MSEnchant/Assets/enchantUI.tab_transmission.layerleftText", 46, 153));
        Append(RightText = new MSImage("MSEnchant/Assets/enchantUI.tab_transmission.layerrightText", 214, 135));

        Append(TransmissionButton =
            new MSButton("MSEnchant/Assets/enchantUI.tab_transmission.buttontransmissionStart", 83, 252)
            {
                Disabled = true,
                Tooltip = "将装备痕迹具有的潜能继承到装备上。装备上原有的潜能将会消失。"
            });
        TransmissionButton.OnClick += (e, element) =>
        {
            var popup = new MSEnchantPopup("是否使用装备痕迹，\n提高装备的能力？");
            popup.OnButtonClick += (button, name) =>
            {
                if (name != "buttonconfirm")
                    return;

                StartTransmission();
            };
            State.ShowPopupCenter(popup);
        };

        Append(CancelButton = new MSButton("MSEnchant/Assets/enchantUI.buttoncancel", 174, 252)
        {
            Tooltip = "返回初始画面。"
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

        Append(TraceEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.tab_transmission.traceEffect", new[]
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

        Append(TransmissionEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.transmissionEffect", new[]
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
                Tips.Left.Set(currentTip.Offset.X, 0);
                Tips.Top.Set(currentTip.Offset.Y, 0);
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
            State.ShowNoticeCenter("正在处理别的请求。\n请稍后再试。");
            return;
        }

        var traceMsItem = TraceItem?.GetEnchantItem();
        if (traceMsItem == null)
        {
            State.ShowNoticeCenter("发生未知错误。");
            return;
        }

        IsLocked = true;

        TransmissionEffect.OnAnimationEnded += element =>
        {
            IsLocked = false;

            var result = traceMsItem.TryTransmission(TargetItem, out var beforeItem);
            if (result == StarForceTransmissionResult.NoResult)
            {
                State.ShowNoticeCenter("正在处理别的请求。\n请稍后再试。");
                return;
            }

            var afterItem = TraceItem;

            var content = result switch
            {
                StarForceTransmissionResult.Success => "继承成功。",
                StarForceTransmissionResult.Failed => "继承失败。",
                _ => string.Empty
            };

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

        MSEnchantUI.Instance.ReplaceWindow<StarForceWindow>(this, window => { window.EnchantItem = TargetItem; });
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
        State.ShowNoticeCenter("显示装备痕迹的装备和\n继承能力的装备\n必须一致，才能继承能力。");
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
            MSEnchantUI.Instance.ReplaceWindow<MainWindow>(this);
    }

    public struct TipOption
    {
        public string Texture;
        public int Delay;
        public Vector2 Offset;

        public TipOption(string texture, int delay, Vector2 offset)
        {
            Texture = texture;
            Delay = delay;
            Offset = offset;
        }
    }
}
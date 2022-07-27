using System;
using System.Linq;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Component;
using MSEnchant.UI.Control;
using Terraria;
using Terraria.Audio;
using Terraria.UI;

namespace MSEnchant.UI.State;

public class MSEnchantUI : UIState
{
    public static MSEnchantUI Instance { get; private set; }

    public UserInterface UserInterface { get; private set; }

    public MSEnchantUI()
    {
        Instance = this;
        UserInterface = new UserInterface();
        UserInterface.SetState(this);
    }

    public Vector2 CenterOfScreen
    {
        get
        {
            var dimension = GetDimensions();
            return new Vector2(dimension.Width / 2, dimension.Height / 2) / new Vector2(Main.UIScale);
        }
    }

    public bool HasVisibleAlwaysTop
    {
        get
        {
            lock (Elements)
            {
                return Elements.Any(c => c is MSElement msElement && msElement.AlwaysTop);
            }
        }
    }

    public MSNotice ShowNoticeCenter(string content, Action? confirm = null)
    {
        var pos = CenterOfScreen;
        var notice = ShowNotice(content, pos, confirm);
        notice.Left.Set(pos.X - notice.Width.Pixels / 2f, 0f);
        notice.Top.Set(pos.Y - notice.Height.Pixels / 2f, 0f);
        notice.Recalculate();
        return notice;
    }

    private MSNotice InitNotice(string content, Vector2 pos, Action<MSNotice> afterInit)
    {
        var notice = new MSNotice("MSEnchant/Assets/Notice6", content, pos.X, pos.Y);
        notice.OnButtonClick += (evt, element) => { notice.RemoveNextFrame(); };
        afterInit.Invoke(notice);
        SoundEngine.PlaySound(new SoundStyle("MSEnchant/Assets/DlgNotice"));
        notice.AppendNextFrame();
        return notice;
    }

    public MSNotice ShowNotice(string content, Vector2 pos, Action? confirm = null)
    {
        return InitNotice(content, pos,
            notice => { notice.OnConfirmButtonClick += (evt, element) => { confirm?.Invoke(); }; });
    }

    public MSNotice ShowNoticeYesNoCenter(string content, Action? confirm = null, Action? cancel = null)
    {
        var pos = CenterOfScreen;
        var notice = ShowNoticeYesNo(content, pos, confirm);
        notice.Left.Set(pos.X - notice.Width.Pixels / 2f, 0f);
        notice.Top.Set(pos.Y - notice.Height.Pixels / 2f, 0f);
        notice.Recalculate();
        return notice;
    }

    public MSNotice ShowNoticeYesNo(string content, Vector2 pos, Action? confirm = null, Action? cancel = null)
    {
        return InitNotice(content, pos, notice =>
        {
            notice.ShowCancelButton = true;
            notice.OnConfirmButtonClick += (evt, element) => { confirm?.Invoke(); };
            notice.OnCancelButtonClick += (evt, element) => { cancel?.Invoke(); };
        });
    }

    public T ShowPopupCenter<T>(T popup) where T : MSPopup
    {
        var pos = CenterOfScreen;
        popup.Left.Set(pos.X - popup.Width.Pixels / 2f, 0f);
        popup.Top.Set(pos.Y - popup.Height.Pixels / 2f, 0f);
        popup.AppendNextFrame();
        return popup;
    }

    public T ShowPopup<T>(T popup, Vector2 pos) where T : MSPopup
    {
        popup.Left.Set(pos.X, 0f);
        popup.Top.Set(pos.Y, 0f);
        popup.AppendNextFrame();
        return popup;
    }

    public void PlayAnimationCenter(MSAnimationImage animation)
    {
        animation.AnimationOffset = CenterOfScreen;
        animation.RegisterAnimationEndOnce(element => { element.RemoveNextFrame(); });
        animation.Play();
        animation.AppendNextFrame();
    }

    public T ShowWindowCenter<T>() where T : MSWindow
    {
        var pos = CenterOfScreen;
        var window = ShowWindow<T>(pos);
        window.Left.Set(pos.X - window.Width.Pixels / 2, 0f);
        window.Top.Set(pos.Y - window.Height.Pixels / 2, 0f);
        window.Recalculate();
        return window;
    }

    public T ShowWindow<T>(Vector2 pos) where T : MSWindow
    {
        CloseWindow<T>();
        var window = Activator.CreateInstance<T>();
        window.InitWindow(pos);
        window.AppendNextFrame();
        return window;
    }

    public void CloseWindow<T>() where T : MSWindow
    {
        foreach (var window in Children.Where(c => c is T).Cast<T>())
        {
            window.Close();
        }
    }

    public T ReplaceWindow<T>(MSWindow replace, Action<T>? doBeforeClose = null) where T : MSWindow
    {
        if (replace.IsClosing)
            return null;

        var r = ShowWindow<T>(replace.Position());
        r.DragOffset = replace.DragOffset;
        doBeforeClose?.Invoke(r);
        r.Recalculate();
        replace.Close();
        return r;
    }

    public void BringAlwaysTopToFront()
    {
        lock (Elements)
        {
            if (HasVisibleAlwaysTop && !IsLastAlwaysTop)
            {
                BringToFront(Elements.First(IsElementAlwaysTop));
            }
        }
    }

    protected bool IsElementAlwaysTop(UIElement element)
    {
        return element is MSElement { AlwaysTop: true };
    }

    public bool IsLastAlwaysTop
    {
        get
        {
            lock (Elements)
            {
                return IsElementAlwaysTop(Elements.Last());
            }
        }
    }

    public void BringToFront(UIElement element)
    {
        lock (Elements)
        {
            if (HasVisibleAlwaysTop && IsLastAlwaysTop && !IsElementAlwaysTop(element.FindTopElement()))
                return;

            var i = Elements.IndexOf(element);
            if (i == -1 || i == Elements.Count - 1)
                return;

            Elements.RemoveAt(i);
            Elements.Add(element);
        }
    }

    public override void MouseDown(UIMouseEvent evt)
    {
        BringAlwaysTopToFront();

        base.MouseDown(evt);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        var pointElement = GetElementAt(new Vector2(Main.mouseX, Main.mouseY));
        if (pointElement is MSElement e)
            e.DrawTooltip(spriteBatch);
    }

    public bool HasWindow => Children.Count(c => c is MSWindow) > 0;
    
    public bool HasMultipleWindow => Children.Count(c => c is MSWindow) > 1;

    public bool IsWindowEnabled<T>(out MSWindow exists) where T : MSWindow
    {
        exists = Children.FirstOrDefault(c => c is T) as MSWindow;
        
        if (exists == null && HasWindow)
        {
            var window = Activator.CreateInstance<T>();
            exists = Children.FirstOrDefault(c => window.LinkWindowTypes.Contains(c.GetType())) as MSWindow;
        }

        return exists != null;
    }
    
    public void CloseFrontWindow()
    {
        var window = Children.LastOrDefault(c => c is MSWindow) as MSWindow;
        if (window == null)
            return;

        if (!window.Close())
            return;
        
        if (!HasMultipleWindow && Main.playerInventory)
            Main.LocalPlayer.ToggleInv();
    }

    public void EnableWindow<T>() where T : MSWindow
    {
        if (IsWindowEnabled<T>(out _))
            return;
        
        if (!Main.playerInventory)
            Main.LocalPlayer.ToggleInv();

        ShowWindowCenter<T>();
    }
    
    public void ToggleWindow<T>() where T : MSWindow
    {
        if (!IsWindowEnabled<T>(out var exists))
        {
            Main.CloseNPCChatOrSign();
            
            if (!Main.playerInventory)
                Main.LocalPlayer.ToggleInv();

            ShowWindowCenter<T>();
            SoundEngine.PlaySound(new SoundStyle("MSEnchant/Assets/MenuUp"));
        }
        else if (exists.Close())
        {
            if (!HasMultipleWindow && Main.playerInventory)
                Main.LocalPlayer.ToggleInv();
            SoundEngine.PlaySound(new SoundStyle("MSEnchant/Assets/MenuDown"));
        }
    }
}
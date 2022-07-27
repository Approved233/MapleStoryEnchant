using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MSEnchant.Helper;
using Terraria;
using Terraria.GameInput;

namespace MSEnchant.UI.Control;

public abstract class MSWindow : MSElement
{
    protected MSImage Border;
    
    public virtual Type[] LinkWindowTypes { get; }

    public virtual bool CanClose => CloseButton != null;

    public virtual bool FullDrag { get; set; } = false;

    protected virtual float DragTitleHeight { get; set; } = 20f;

    public abstract string BaseTexturePath { get; }
    
    public bool IsClosing { get; private set; }

    public void InitWindow(Vector2 pos)
    {
        AllowDrag = true;

        Append(Border = new MSImage($"{BaseTexturePath}.backgrnd"));

        DoInit();

        Width.Set(Border.Texture.Width(), 0f);
        Height.Set(Border.Texture.Height(), 0f);
        Left.Set(pos.X, 0f);
        Top.Set(pos.Y, 0f);

        OnUpdate += element =>
        {
            var mouse = new Vector2(Main.mouseX, Main.mouseY);
            if (ContainsPoint(mouse))
                PlayerInput.LockVanillaMouseScroll($"MSEnchant/UI/{GetType().Name}");
        };

        Recalculate();
    }

    protected virtual void DoInit()
    {
    }

    protected MSButton CloseButton;
    
    protected void AddCloseButton(float left = 0f, float top = 4f, string tooltip = "")
    {
        CloseButton = new MSButton("MSEnchant/Assets/buttonexit", left, top)
        {
            Tooltip = tooltip
        };
        CloseButton.OnClick += (evt, element) => { Close(); };
        Append(CloseButton);
    }

    protected override bool PrepareDrag(Vector2 mouse)
    {
        if (FullDrag)
        {
            var e = GetElementAt(mouse);
            if (e is MSButton button && button.IsPressing)
                return false;

            return true;
        }

        var pointElement = GetElementAt(mouse);
        if (pointElement != Border)
            return false;

        var dimension = Border.GetDimensions();
        var mouseScaled = Vector2.Transform(mouse, Main.UIScaleMatrix);
        var windowScaled = Vector2.Transform(new Vector2(dimension.X, dimension.Y), Main.UIScaleMatrix);

        var inRange = windowScaled - mouseScaled;
        if (inRange.Y >= 0 || Math.Abs(inRange.Y) >= DragTitleHeight)
            return false;

        return true;
    }

    protected MSImage AddBackGroundTexture(string texture, float left = 0f, float top = 0f)
    {
        var inner = new MSImage($"{BaseTexturePath}.{texture}", left, top);
        Append(inner);
        return inner;
    }

    public bool Close(bool force = false)
    {
        if (!force && (!CanClose || IsLocked || State.HasVisibleAlwaysTop))
            return false;

        if (IsClosing)
            return false;

        IsClosing = true;
        this.RemoveNextFrame();
        return true;
    }
}
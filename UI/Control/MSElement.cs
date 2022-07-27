using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Component;
using MSEnchant.UI.State;
using MSEnchant.UI.Window;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;

namespace MSEnchant.UI.Control;

public class MSElement : UIElement
{
    public MSEnchantUI State => MSEnchantUI.Instance;

    public bool IsDragging { get; protected set; }
    public Vector2 DragOffset { get; set; }

    public bool IsMouseContaining { get; private set; } = false;

    public virtual bool Visible { get; set; } = true;

    public virtual bool Disabled { get; set; }

    public bool AllowDrag { get; set; } = false;

    public bool AlwaysTop { get; set; } = false;

    private bool _isLocked;

    public bool IsLocked
    {
        get => Parent is MSElement e ? e.IsLocked : _isLocked;
        set
        {
            if (Parent is MSElement e)
                e.IsLocked = value;
            else
                _isLocked = value;
        }
    }


    private bool _displayDebugBox;

    public bool DisplayDebugBox
    {
        get => Parent is MSElement e ? e.DisplayDebugBox : _displayDebugBox;
        set
        {
            if (Parent is MSElement e)
                e.DisplayDebugBox = value;
            else
                _displayDebugBox = value;
        }
    }

    public string Tooltip
    {
        get => TooltipComponent.Content;
        set => TooltipComponent.Content = value;
    }

    private MSTooltip TooltipComponent;

    public MSElement(float left = 0f, float top = 0f)
    {
        Left.Set(left, 0f);
        Top.Set(top, 0f);

        if (this is not MSTooltip and not MSText)
        {
            TooltipComponent = new MSTooltip(string.Empty)
            {
                Visible = false
            };
        }

        OnMouseDown += (e, element) =>
        {
            if (!AllowDrag)
                return;

            if (!PrepareDrag(e.MousePosition))
                return;

            DragOffset = new Vector2(e.MousePosition.X - Left.Pixels,
                e.MousePosition.Y - Top.Pixels);
            IsDragging = true;
        };

        OnMouseUp += (e, element) =>
        {
            if (!AllowDrag || !IsDragging)
                return;

            var end = e.MousePosition;
            IsDragging = false;

            Left.Set(end.X - DragOffset.X, 0f);
            Top.Set(end.Y - DragOffset.Y, 0f);

            Recalculate();
        };
    }

    public override void Update(GameTime gameTime)
    {
        IsMouseContaining = !PlayerInput.IgnoreMouseInterface && ContainsPoint(new Vector2(Main.mouseX, Main.mouseY));
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        base.Draw(spriteBatch);
    }

    public void DrawTooltip(SpriteBatch spriteBatch)
    {
        if (TooltipComponent == null || !Visible || Disabled)
            return;

        var dimension = GetDimensions();
        var width = dimension.Width;

        TooltipComponent.Visible = IsMouseHovering && !string.IsNullOrEmpty(Tooltip);
        if (width != TooltipComponent.Left.Pixels)
        {
            TooltipComponent.Left.Set(dimension.X + width, 0f);
            TooltipComponent.Top.Set(dimension.Y, 0f);
            TooltipComponent.Recalculate();
        }

        TooltipComponent?.Draw(spriteBatch);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IsMouseContaining)
        {
            Main.LocalPlayer.mouseInterface = true;
        }

        if (IsDragging)
        {
            var mouse = new Vector2(Main.mouseX, Main.mouseY);
            Left.Set(mouse.X - DragOffset.X, 0f);
            Top.Set(mouse.Y - DragOffset.Y, 0f);
            Recalculate();
        }

        if (DisplayDebugBox && IsMouseHovering)
        {
            var dimension = GetDimensions();
            var width = dimension.Width;
            var height = dimension.Height;

            var color = Color.Red;

            DrawLine(spriteBatch, dimension.Position(), dimension.Position() + new Vector2(width, 0.0f), 1f, color);
            DrawLine(spriteBatch, dimension.Position() + new Vector2(width, 0.0f),
                dimension.Position() + new Vector2(width, height), 1f, color);
            DrawLine(spriteBatch, dimension.Position() + new Vector2(width, height),
                dimension.Position() + new Vector2(0.0f, height), 1f, color);
            DrawLine(spriteBatch, dimension.Position() + new Vector2(0.0f, height), dimension.Position(), 1f, color);

            var debugText = $"{width}x{height} L:{Left.Pixels} T:{Top.Pixels}";
            if (MarginLeft != 0)
                debugText += $" ML:{MarginLeft}";

            if (MarginTop != 0)
                debugText += $" MT:{MarginTop}";

            ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, debugText,
                dimension.Position() - new Vector2(0, 20f), Color.Red, 0f, Vector2.Zero, Vector2.One);
        }
    }

    protected void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float width, Color color)
    {
        var pos = end - start;
        var rotation = (float)Math.Atan2(pos.Y, pos.X);
        var scale = new Vector2(pos.Length(), width);
        // if (Global.PixelTexture == null)
        // {
        //     Global.PixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 4, 4);
        //     var data = new Color[16];
        //     for (var index = 0; index < data.Length; ++index)
        //         data[index] = Color.White;
        //     Global.PixelTexture.SetData(data);
        // }

        spriteBatch.Draw(Global.PixelTexture, start, new Rectangle?(), color, rotation, new Vector2(0.0f, 2f),
            scale / 4f, SpriteEffects.None, 1f);
    }

    protected virtual bool PrepareDrag(Vector2 mouse)
    {
        if (!this.InFront())
            return false;

        return true;
    }

    public bool CanProcessInput => !IsLocked && (AlwaysTop || this.InFront()) && !PlayerInput.IgnoreMouseInterface;

    public bool CanPressed => !PlayerInput.IgnoreMouseInterface && Visible && !Disabled;

    public override void MouseOut(UIMouseEvent evt)
    {
        if (!CanProcessInput)
            return;

        base.MouseOut(evt);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        if (!CanProcessInput)
            return;

        base.MouseOver(evt);
    }

    public override void MiddleDoubleClick(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.MiddleDoubleClick(evt);
    }

    public override void MiddleMouseDown(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.MiddleMouseDown(evt);
    }

    public override void MiddleMouseUp(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.MiddleMouseUp(evt);
    }

    public override void RightMouseUp(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.RightMouseUp(evt);
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.ScrollWheel(evt);
    }

    public override void MiddleClick(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.MiddleClick(evt);
    }

    public override void RightClick(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.RightClick(evt);
    }

    public override void DoubleClick(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.DoubleClick(evt);
    }

    public override void Click(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.Click(evt);
    }

    public override void MouseUp(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
            return;

        base.MouseUp(evt);
    }

    public override void MouseDown(UIMouseEvent evt)
    {
        if (!CanProcessInput || !CanPressed)
        {
            State.BringToFront(this.FindTopElement());
            return;
        }

        base.MouseDown(evt);
    }
}
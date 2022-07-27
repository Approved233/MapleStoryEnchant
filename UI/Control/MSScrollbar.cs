using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using ReLogic.Content;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant.UI.Control;

public class MSScrollbar : MSElement
{
    private float _viewPosition;
    private float _viewSize = 1f;
    private float _maxViewSize;
    private bool _isDragging;
    private bool _isHoveringOverHandle;
    private float _dragYOffset;
    private Asset<Texture2D> barTexture;
    private Asset<Texture2D> thumbTexture;

    public float ViewPosition
    {
        get => _viewPosition;
        set => _viewPosition = MathHelper.Clamp(value, 0.0f, MaxView);
    }

    public float MaxView => _maxViewSize - _viewSize;

    public bool DrawThumb => _maxViewSize > barTexture.Height();
    
    public bool CanScroll => DrawThumb;

    public void GoToBottom() => ViewPosition = MaxView;

    public Vector2 ScrollPower = Vector2.Zero;

    public MSScrollbar(string texture, float left = 0f, float top = 0f) : base(left, top)
    {
        barTexture = ModContent.Request<Texture2D>($"{texture}.base", AssetRequestMode.ImmediateLoad);
        thumbTexture = ModContent.Request<Texture2D>($"{texture}.thumb", AssetRequestMode.ImmediateLoad);
        Width.Set(barTexture.Width(), 0.0f);
        MaxWidth.Set(barTexture.Width(), 0.0f);
        Height.Set(barTexture.Height(), 0f);
        MaxHeight.Set(barTexture.Height(), 0f);
    }

    public void SetView(float viewSize, float maxViewSize)
    {
        viewSize = MathHelper.Clamp(viewSize, 0.0f, maxViewSize);
        _viewPosition = MathHelper.Clamp(_viewPosition, 0.0f, maxViewSize - viewSize);
        _viewSize = viewSize;
        _maxViewSize = maxViewSize;
    }

    public float GetValue() => _viewPosition;

    public float ThumbOffset
    {
        get
        {
            var barTexture = this.barTexture.Value;
            var thumbTexture = this.thumbTexture.Value;
            var percent = ViewPosition / MaxView;
            return 1 * percent * (barTexture.Height - thumbTexture.Height);
        }
    }
    
    public Rectangle GetThumbRectangle()
    {
        var dimension = GetInnerDimensions();
        var thumbTexture = this.thumbTexture.Value;

        return new Rectangle((int)dimension.X, (int)(dimension.Y + ThumbOffset), thumbTexture.Width,
            thumbTexture.Height);
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        CalculatedStyle dimensions = GetDimensions();
        CalculatedStyle innerDimensions = GetInnerDimensions();
        if (_isDragging)
            ViewPosition = (Main.mouseY - innerDimensions.Y - _dragYOffset) / innerDimensions.Height * _maxViewSize;

        _isHoveringOverHandle = IsThumbContains(Main.MouseScreen);

        var barTexture = this.barTexture.Value;
        var thumbTexture = this.thumbTexture.Value;
        var percent = ViewPosition / MaxView;

        spriteBatch.UseNonPremultiplied(() =>
        {
            spriteBatch.Draw(this.barTexture.Value, dimensions.Position(), Color.White);
            if (DrawThumb)
            {
                var thumbOffset = 1 * percent * (barTexture.Height - thumbTexture.Height);
            
                spriteBatch.Draw(
                    this.thumbTexture.Value,
                    new Rectangle((int)innerDimensions.X, (int)(innerDimensions.Y + thumbOffset), thumbTexture.Width, thumbTexture.Height),
                    new Rectangle(0, 0, thumbTexture.Width, thumbTexture.Height),
                    Color.White
                );
            }
        });
        
    }

    public override void MouseDown(UIMouseEvent evt)
    {
        base.MouseDown(evt);

        if (!CanScroll)
            return;
        
        var handleRectangle = GetThumbRectangle();
        if (IsThumbContains(evt.MousePosition))
        {
            _isDragging = true;
            _dragYOffset = evt.MousePosition.Y - handleRectangle.Y;
        }
        else
        {
            var innerDimensions = GetInnerDimensions();
            ViewPosition = (evt.MousePosition.Y - innerDimensions.Y - handleRectangle.Height) / innerDimensions.Height * _maxViewSize;
        }
    }

    public override void MouseUp(UIMouseEvent evt)
    {
        base.MouseUp(evt);
        _isDragging = false;
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        PlayerInput.LockVanillaMouseScroll("MSEnchant/UI/Control/MSScrollbar");
    }

    public void Scroll(int power)
    {
        if (!CanScroll)
            return;
        
        var down = power < 0;
        var p = Math.Abs(power) / 120;
        var result = ScrollPower.Y * p;
        if (down)
            ViewPosition += result;
        else
            ViewPosition -= result;
    }

    public bool IsThumbContains(Vector2 pos)
    {
        var rect = GetThumbRectangle();
        return rect.Contains(new Point((int)pos.X, (int)pos.Y));
    }
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MSEnchant.UI.Control;

public class MSItem : MSElement
{
    public Texture2D Texture { get; set; }

    private Item _displayItem;

    public Item DisplayItem
    {
        get => _displayItem;
        set
        {
            _displayItem = value;
            Texture = value.IsNullOrAir() ? null : TextureAssets.Item[value.type].Value;
        }
    }

    protected Asset<Texture2D> Shadow;

    public bool DisplayShadow { get; set; } = true;

    public bool DisplayTooltip { get; set; } = true;

    public Color DrawColor { get; set; } = Color.White;

    public MSItem(float width, float height, float left = 0f, float top = 0f) : base(left, top)
    {
        Shadow = "Item.shadow".LoadLocaleTexture(AssetRequestMode.ImmediateLoad);
        MaxWidth.Set(width, 0f);
        MaxHeight.Set(height, 0f);
        Width.Set(MaxWidth.Pixels, 0f);
        Height.Set(MaxHeight.Pixels, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (Texture == null)
            return;

        if (DisplayTooltip && IsMouseContaining && this.InFront())
        {
            DisplayItem?.DrawTooltipHackedQueue();
        }

        var dimensions = GetDimensions();
        var pos = dimensions.Position();

        var maxWidth = MaxWidth.Pixels;
        var maxHeight = MaxHeight.Pixels;

        var width = Texture.Width;
        var height = Texture.Height;

        var ratioX = maxWidth / width;
        var ratioY = maxHeight / height;
        var ratio = Math.Min(ratioX, ratioY);

        var diff = Math.Abs(width - height) * ratio / 2f;
        var offset = Vector2.Zero;
        if (width > height)
            offset.Y = diff;

        if (height > width)
            offset.X = diff;

        spriteBatch.UseNonPremultiplied(() =>
        {
            if (DisplayShadow)
            {
                var shadow = Shadow.Value;
                var shadowRatio = maxWidth / shadow.Width;

                spriteBatch.Draw(shadow,
                    pos + new Vector2(maxWidth / 2f - (shadow.Width * shadowRatio / 2), maxHeight - MarginTop - 10f),
                    new Rectangle?(),
                    Color.White, 0f, Vector2.Zero, shadowRatio, SpriteEffects.None, 0f);
            }
        
            spriteBatch.Draw(Texture, pos + offset, new Rectangle?(), DrawColor, 0f, Vector2.Zero, ratio,
                SpriteEffects.None, 0.0f);
        });
    }
}
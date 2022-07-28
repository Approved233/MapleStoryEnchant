using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant.UI.Component;

public class MiniGameGaugeBar : UIElement
{
    public string BaseTexturePath { get; set; }

    protected Asset<Texture2D> InnerTexture { get; set; }
    protected Asset<Texture2D> OuterTexture { get; set; }

    public MiniGameGaugeBar(string texture)
    {
        BaseTexturePath = texture;
        InnerTexture = $"{BaseTexturePath}.0".LoadLocaleTexture(AssetRequestMode.ImmediateLoad);
        OuterTexture = $"{BaseTexturePath}.1".LoadLocaleTexture(AssetRequestMode.ImmediateLoad);

        GaugeOuterStart = new MSImage(OuterTexture);
        Append(GaugeOuterStart);

        GaugeOuterEnd = new MSImage(OuterTexture);
        Append(GaugeOuterEnd);

        GaugeInner = new MSImage(InnerTexture);
        Append(GaugeInner);
    }

    protected MSImage GaugeOuterStart;
    protected MSImage GaugeOuterEnd;
    protected MSImage GaugeInner;

    public float GaugeWidth { get; set; }

    public Vector2 Center { get; set; }

    public override void Update(GameTime gameTime)
    {
        Left.Set(Center.X, 0f);
        Top.Set(Center.Y, 0f);
        GaugeOuterStart.Left.Set(-(GaugeWidth / 2f), 0f);
        GaugeOuterEnd.Left.Set(GaugeWidth / 2f, 0f);
        GaugeInner.ImageScale = new Vector2(GaugeWidth / InnerTexture.Width(), 1f);

        Recalculate();

        base.Update(gameTime);
    }

    public Vector2 SuccessZoneStart => GaugeOuterStart.GetDimensions().Position() +
                                       new Vector2(GaugeOuterStart.Width.Pixels, GaugeOuterStart.Height.Pixels);

    public Vector2 SuccessZoneEnd => SuccessZoneStart + new Vector2(GaugeInner.Width.Pixels, GaugeInner.Height.Pixels);

    public bool InSuccessZone(Vector2 point)
    {
        var start = SuccessZoneStart;
        var end = SuccessZoneEnd;
        return point.X > start.X && point.X < end.X;
    }
}
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using Terraria.UI;

namespace MSEnchant.UI.Component;

public class MiniGameStar : UIElement
{
    public bool Visible { get; set; } = true;

    protected MSImage Star;

    protected MSAnimationImage Particle;
    
    public SpriteEffects Effect { get; set; } = SpriteEffects.None;

    public int ParticleSpeed { get; set; }
    
    public MiniGameStar(string star, string particle, float left = 0f, float top = 0f)
    {
        Star = new MSImage(star, 0, -15);
        Append(Star);

        Particle = new MSAnimationImage(particle)
        {
            Loop = true
        };
        SetParticleSpeed(ParticleSpeed);
        Append(Particle);
        
        Left.Set(left, 0f);
        Top.Set(top, 0f);
    }

    protected int[][] particleSpeedMap = new int[][]
    {
        new[] { 150, 210, 210, 210, 150, 120 },
        new[] { 120, 180, 180, 180, 120, 120 },
        new[] { 120, 150, 150, 150, 90, 90 },
        new[] { 90, 120, 120, 120, 90, 60 },
        new[] { 60, 60, 60, 60, 30, 30 },
    };

    public void SetParticleSpeed(int level)
    {
        var speed = particleSpeedMap.ElementAtOrDefault(level);
        if (speed == null)
            return;

        Particle.SetFrames(new[]
        {
            new MSFrameData(speed[0], -22, -12),
            new MSFrameData(speed[1], -22, -11),
            new MSFrameData(speed[2], -17, -13),
            new MSFrameData(speed[3], -9, -12),
            new MSFrameData(speed[4], -9, -12),
            new MSFrameData(speed[5], -22, -12),
        });
    }

    public Vector2 StarCenter => Star.Center() + this.Center();
    
    public Vector2 StarPosition => Star.GetDimensions().Position() + new Vector2(Star.Width.Pixels / 2, Star.Height.Pixels / 2);

    public override void Update(GameTime gameTime)
    {
        Star.Effect = Effect;
        Star.Visible = Visible;
        Particle.Effect = Effect;
        Particle.Visible = Visible;

        base.Update(gameTime);
    }
}
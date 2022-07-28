using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant.UI.Control;

public struct MSFrameData
{
    public int Delay;
    public float Left;
    public float Top;

    public MSFrameData(int delay, float left = 0f, float top = 0f)
    {
        Delay = delay;
        Left = left;
        Top = top;
    }
}

public class MSAnimationImage : MSElement
{
    protected Dictionary<Asset<Texture2D>, MSFrameData> Frames = new();

    public string BaseTexturePath { get; set; }

    public bool Loop { get; set; }

    public bool InvisibleWhenEnd { get; set; } = true;

    public bool PlayEnded { get; protected set; }

    public event AnimationEndEvent OnAnimationEnded;

    public Vector2 AnimationOffset { get; set; } = Vector2.Zero;

    public MSAnimationImage(string texture, MSFrameData[] frames = null)
    {
        BaseTexturePath = texture;
        if (frames != null)
            SetFrames(frames);
    }

    public void AddFrame(MSFrameData frame)
    {
        var i = Frames.Count;
        var texture = $"{BaseTexturePath}.{i}";
        var origin = texture.GetTextureOrigin(new Vector2(frame.Left, frame.Top));
        Frames[texture.LoadLocaleTexture(AssetRequestMode.ImmediateLoad)] = new MSFrameData
        {
            Delay = frame.Delay,
            Left = origin.X,
            Top = origin.Y
        };
    }

    public void SetFrames(MSFrameData[] frames)
    {
        Frames.Clear();
        foreach (var frame in frames)
        {
            AddFrame(frame);
        }

        Reset();
    }

    private TimeSpan? lastFrameTime;

    private int _currentFrame;

    public int CurrentFrame
    {
        get => _currentFrame;
        set => _currentFrame = Math.Clamp(value, 0, MaxFrame - 1);
    }

    public int NextFrame => Math.Clamp(CurrentFrame + 1, 0, MaxFrame - 1);

    public int MaxFrame => Frames.Count;

    public bool Stopped { get; private set; }

    public MSFrameData CurrentFrameData => Frames.Values.ElementAt(CurrentFrame);

    public SpriteEffects Effect { get; set; } = SpriteEffects.None;

    public Vector2 Center()
    {
        var frame = CurrentFrame;
        var texture = Frames.Keys.ElementAtOrDefault(frame);
        if (texture == null)
            return Vector2.Zero;

        var data = CurrentFrameData;
        return new Vector2(data.Left + texture.Width() * 0.5f, data.Top + texture.Height() * 0.5f);
    }

    public override void Update(GameTime gameTime)
    {
        if (!Visible || PlayEnded || Stopped || Frames.Count == 0)
            return;

        if (lastFrameTime == null)
            lastFrameTime = gameTime.TotalGameTime;

        var frame = CurrentFrameData;
        if ((gameTime.TotalGameTime - lastFrameTime).Value.TotalMilliseconds < frame.Delay)
            return;

        lastFrameTime = gameTime.TotalGameTime;

        if (CurrentFrame + 1 > (MaxFrame - 1))
        {
            if (!Loop)
                PlayEnded = true;
            else
                Reset();

            OnAnimationEnded?.Invoke(this);
        }
        else
            CurrentFrame += 1;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (!Visible || Frames.Count == 0 || (PlayEnded && InvisibleWhenEnd))
            return;

        var texture = Frames.Keys.ElementAt(CurrentFrame);
        var frame = Frames[texture];
        Left.Set(frame.Left + AnimationOffset.X, 0f);
        if (Effect.HasFlag(SpriteEffects.FlipHorizontally))
            Left.Set(-Left.Pixels, Left.Precent);
        Top.Set(frame.Top + AnimationOffset.Y, 0f);
        if (Effect.HasFlag(SpriteEffects.FlipVertically))
            Top.Set(-Top.Pixels, Top.Percent);
        Width.Set(texture.Width(), 0f);
        Height.Set(texture.Height(), 0f);

        Recalculate();

        var dimensions = GetDimensions();

        spriteBatch.UseNonPremultiplied(() => { spriteBatch.Draw(texture.Value, dimensions.Position(), Color.White); });
    }

    public void RegisterAnimationEndOnce(AnimationEndEvent d)
    {
        void AnimationEnd(UIElement element)
        {
            OnAnimationEnded -= AnimationEnd;
            d.Invoke(element);
        };

        OnAnimationEnded += AnimationEnd;
    }

    public void Reset()
    {
        CurrentFrame = 0;
        lastFrameTime = null;
        PlayEnded = false;
    }

    public void Play()
    {
        Reset();
        Stopped = false;
        Visible = true;
    }

    public void Stop()
    {
        CurrentFrame = MaxFrame - 1;
        Visible = true;
    }

    public void Pause()
    {
        Stopped = true;
    }

    public delegate void AnimationEndEvent(UIElement affectedElement);
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MSEnchant.Effects;

public class WorldEffectAnimation
{
    public string BaseTexturePath { get; set; }

    protected Dictionary<Asset<Texture2D>, MSFrameData> Frames = new();

    private DateTime? lastFrameTime;

    private int _currentFrame;

    public int CurrentFrame
    {
        get => _currentFrame;
        set => _currentFrame = Math.Clamp(value, 0, MaxFrame - 1);
    }

    public int MaxFrame => Frames.Count;

    public bool Loop { get; set; }

    public bool InvisibleWhenEnd { get; set; } = true;

    public bool PlayEnded { get; protected set; }
    
    public virtual bool Visible { get; set; } = true;
    
    public bool Stopped { get; private set; }
    
    public event AnimationEndEvent OnAnimationEnded;
    
    public Vector2 AnimationOffset { get; set; } = Vector2.Zero;
    
    public MSFrameData CurrentFrameData => Frames.Values.ElementAt(CurrentFrame);

    public WorldEffectAnimation(string texture, MSFrameData[] frames = null)
    {
        BaseTexturePath = texture;
        if (frames != null)
            SetFrames(frames);
    }

    public WorldEffectAnimation Clone()
    {
        return new WorldEffectAnimation(BaseTexturePath, Frames.Values.ToArray());
    }

    public void UpdateFrame()
    {
        if (!Visible || PlayEnded || Stopped || Frames.Count == 0)
            return;

        if (lastFrameTime == null)
            lastFrameTime = DateTime.Now;

        var frame = CurrentFrameData;
        if ((DateTime.Now - lastFrameTime).Value.TotalMilliseconds < frame.Delay)
            return;

        lastFrameTime = DateTime.Now;

        if (CurrentFrame + 1 > (MaxFrame - 1))
        {
            if (!Loop)
                PlayEnded = true;
            else
                Reset();

            OnAnimationEnded?.Invoke();
        }
        else
            CurrentFrame += 1;
    }

    public void DrawFrame(SpriteBatch spriteBatch, Vector2 pos)
    {
        if (!Visible || Frames.Count == 0 || (PlayEnded && InvisibleWhenEnd))
            return;

        var texture = Frames.Keys.ElementAt(CurrentFrame);
        var frame = Frames[texture];
        var left = frame.Left + AnimationOffset.X;
        var top = frame.Top + AnimationOffset.Y;

        pos.X -= left;
        pos.Y -= top;

        spriteBatch.UseNonPremultiplied(() => { spriteBatch.Draw(texture.Value, pos.ForDraw(), Color.White); });
    }
    
    public void AddFrame(MSFrameData frame)
    {
        var i = Frames.Count;
        Frames[ModContent.Request<Texture2D>($"{BaseTexturePath}.{i}", AssetRequestMode.ImmediateLoad)] = frame;
    }

    public void SetFrames(MSFrameData[] frames)
    {
        Frames.Clear();
        foreach (var delay in frames)
        {
            AddFrame(delay);
        }

        Reset();
    }

    public void Reset()
    {
        CurrentFrame = 0;
        lastFrameTime = null;
        PlayEnded = false;
    }
    
    public delegate void AnimationEndEvent();
}
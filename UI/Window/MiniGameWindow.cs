using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MSEnchant.UI.Component;
using MSEnchant.UI.Control;
using Terraria;
using Terraria.Audio;
using Terraria.UI;

namespace MSEnchant.UI.Window;

public class MiniGameWindow : MSWindow
{
    public override string BaseTexturePath => "MSEnchant/Assets/enchantUI.miniGame";

    public override Type[] LinkWindowTypes => new[]
    {
        typeof(MainWindow),
        typeof(StarForceWindow),
        typeof(TransmissionWindow)
    };
    
    public override bool FullDrag => true;

    protected MSAnimationImage StartEffect;

    protected MSAnimationImage TimeEffect;

    protected MSAnimationImage StopEffect;

    protected MSAnimationImage SuccessEffect;

    protected MSButton StopButton;

    protected MiniGameStar Star;

    protected MiniGameGaugeBar GaugeBar;
    
    protected MSItem EnchantItemComponent;

    private State _gameState = State.Start;

    protected State GameState
    {
        get => _gameState;
        set
        {
            if (value == _gameState)
                return;

            _gameState = value;
            ChangeGameState(_gameState);
        }
    }

    protected Vector2 StarStartPosition = new(17, 192);
    protected Vector2 StarEndPosition = new(215, 192);
    protected Vector2 StarDirection = new(3.2f, 0);

    protected SoundStyle StarSound;
    protected SoundStyle EnchantStarSuccessSound;
    protected SoundStyle EnchantStarStopSound;

    public bool ClickedEndButton { get; protected set; } = false;

    public Item DisplayItem
    {
        get => EnchantItemComponent.DisplayItem;
        set => EnchantItemComponent.DisplayItem = value;
    }

    protected override void DoInit()
    {
        AlwaysTop = true;
        
        StarSound = new SoundStyle("MSEnchant/Assets/EnchantStar1");
        EnchantStarSuccessSound = new SoundStyle("MSEnchant/Assets/EnchantSuccess");
        EnchantStarStopSound = new SoundStyle("MSEnchant/Assets/EnchantStarStop");

        AddBackGroundTexture("backgrnd2", 3, 10);
        AddBackGroundTexture("backgrnd3", 41, 16);

        Append(EnchantItemComponent = new MSItem(68, 68, 100, 80));
        
        StartEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.miniGame.startEff",
            new[]
            {
                new MSFrameData(150, 63, 88),
                new MSFrameData(150, 59, 87),
                new MSFrameData(150, 58, 87),
                new MSFrameData(300, 77, 91),
                new MSFrameData(150, 78, 91),
                new MSFrameData(150, 78, 91)
            });
        StartEffect.OnAnimationEnded += element => { GameState = State.Catching; };
        Append(StartEffect);

        TimeEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.miniGame.time", new[]
        {
            new MSFrameData(840, 109, 82),
            new MSFrameData(840, 102, 82),
            new MSFrameData(840, 109, 81),
            new MSFrameData(840, 106, 82),
            new MSFrameData(840, 106, 81),
            new MSFrameData(800, 104, 81)
        })
        {
            Visible = false
        };
        TimeEffect.OnAnimationEnded += element => { GameState = State.End; };
        Append(TimeEffect);

        StopEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.miniGame.stopEff", new[]
        {
            new MSFrameData(120, 54, 115),
            new MSFrameData(120, 55, 116),
            new MSFrameData(120, 53, 119),
            new MSFrameData(120, 53, 126),
            new MSFrameData(120, 71, 132),
            new MSFrameData(120, 74, 135)
        })
        {
            Visible = false
        };
        StopEffect.OnAnimationEnded += element => { GameState = State.Close; };
        Append(StopEffect);

        SuccessEffect = new MSAnimationImage("MSEnchant/Assets/enchantUI.miniGame.successEff", new[]
        {
            new MSFrameData(120, 44, 131),
            new MSFrameData(120, 44, 121),
            new MSFrameData(120, 44, 116),
            new MSFrameData(120, 45, 113),
            new MSFrameData(120, 46, 112),
            new MSFrameData(120, 47, 114),
            new MSFrameData(120, 52, 118),
        })
        {
            Visible = false
        };
        Append(SuccessEffect);

        GaugeBar = new MiniGameGaugeBar("MSEnchant/Assets/enchantUI.miniGame.gauge")
        {
            Center = new Vector2(130, 183)
        };
        Append(GaugeBar);

        Star = new MiniGameStar("MSEnchant/Assets/enchantUI.miniGame.star.STAR",
            "MSEnchant/Assets/enchantUI.miniGame.particle", 17, 192)
        {
            Visible = false
        };
        Append(Star);

        StopButton = new MSButton("MSEnchant/Assets/enchantUI.miniGame.buttonstop", 85, 212)
        {
            Disabled = true,
            ClickSound = "",
            Tooltip = "移动中的星星将留在当前位置。"
        };
        StopButton.OnClick += (evt, element) =>
        {
            if (ClickedEndButton)
                return;
            
            ClickedEndButton = true;

            StopEffect.Reset();

            var starCenter = Star.StarCenter;
            var effectCenter = StopEffect.Center();
            var diff = effectCenter.X - starCenter.X;

            StopEffect.AnimationOffset = new Vector2(-diff, 0f);
            StopEffect.Play();

            GameState = State.End;
        };
        Append(StopButton);
        
        ChangeDifficulty(1);
    }

    public void Reset()
    {
        GameState = State.Start;
    }

    public bool InSuccessZone() => GaugeBar.InSuccessZone(Star.StarPosition);

    public bool IsSuccess = false;

    public override void Update(GameTime gameTime)
    {
        if (Star.Visible)
        {
            var pos = new Vector2(Star.Left.Pixels, Star.Top.Pixels);
            var nextPos = pos + StarDirection;
            var updateNextPos = false;
            if (nextPos.X > StarEndPosition.X || nextPos.X < StarStartPosition.X)
            {
                StarDirection.X = -StarDirection.X;
                updateNextPos = true;
            }

            if (nextPos.Y > StarEndPosition.Y || nextPos.Y < StarStartPosition.Y)
            {
                StarDirection.Y = -StarDirection.Y;
                updateNextPos = true;
            }

            if (updateNextPos)
                nextPos = pos + StarDirection;

            Star.Effect = StarDirection.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Star.Left.Set(nextPos.X, 0f);
            Star.Top.Set(nextPos.Y, 0f);
            Star.Recalculate();
        }

        if (GameState == State.Catching)
        {
            if (SoundEngine.FindActiveSound(StarSound) == null)
                SoundEngine.PlaySound(StarSound);
        }

        if (Main.oldKeyState.IsKeyDown(Keys.Space) && Main.keyState.IsKeyUp(Keys.Space))
        {
            StopButton.Click(null);
        }

        base.Update(gameTime);
    }

    public void ResetStarPosition()
    {
        StarDirection = new Vector2(Math.Abs(StarDirection.X), Math.Abs(StarDirection.Y));
        Star.Left.Set(StarStartPosition.X, 0f);
        Star.Top.Set(StarStartPosition.Y, 0f);
        Star.Recalculate();
    }

    protected void ChangeGameState(State state)
    {
        if (state == State.Start)
        {
            ClickedEndButton = false;
            Star.Visible = false;
            StopEffect.Visible = false;
            StopButton.Disabled = true;
            TimeEffect.Visible = false;
            StartEffect.Play();
        }
        else if (state == State.Catching)
        {
            ResetStarPosition();
            Star.Visible = true;
            StopButton.Disabled = false;
            TimeEffect.Play();
        }
        else if (state == State.End)
        {
            SoundEngine.FindActiveSound(StarSound)?.Stop();
            Star.Visible = false;
            TimeEffect.Visible = false;
            StopButton.Disabled = true;

            IsSuccess = InSuccessZone();
            var sound = IsSuccess ? EnchantStarSuccessSound : EnchantStarStopSound;

            if (IsSuccess)
            {
                SuccessEffect.Play();
            }

            SoundEngine.PlaySound(sound);

            if (!ClickedEndButton)
                GameState = State.Close;
        }
        else if (state == State.Close)
        {
            OnMiniGameEnd?.Invoke();
            Close(true);
        }
    }

    public event Global.VoidDelegate OnMiniGameEnd;
    
    private static readonly DifficultyLevelData[] DifficultyData = {
        new(70, 3.2f, 1),
        new(65, 3.7f, 2),
        new(60, 4.2f, 3),
        new(55, 4.7f, 4),
        new(50, 5.2f, 5),
        new(40, 5.7f, 5),
    };

    public void ChangeDifficulty(int level)
    {
        level = Math.Clamp(level, 1, DifficultyData.Length);
        var data = DifficultyData[level - 1];
        ResetStarPosition();
        
        GaugeBar.GaugeWidth = data.GaugeWidth;
        StarDirection = new Vector2(data.StarSpeed, 0f);
        Star.SetParticleSpeed(data.StarParticleSpeed - 1);
    }
    
    public enum State
    {
        Start,
        Catching,
        End,
        Close
    }

    public struct DifficultyLevelData
    {
        public int GaugeWidth;
        public float StarSpeed;
        public int StarParticleSpeed;

        public DifficultyLevelData(int gaugeWidth, float starSpeed, int starParticleSpeed)
        {
            GaugeWidth = gaugeWidth;
            StarSpeed = starSpeed;
            StarParticleSpeed = starParticleSpeed;
        }
    }
}
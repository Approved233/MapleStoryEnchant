#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace MSEnchant.Helper;

public static class GraphicsHelper
{
    private static readonly List<FieldInfo> SpriteBatchFieldCache = new List<FieldInfo>();

    private static FieldInfo? GetSpriteBatchField(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        var field = SpriteBatchFieldCache.FirstOrDefault(f => f.Name == name);
        if (field != null)
            return field;

        field = typeof(SpriteBatch).GetField(name, flags);
        if (field == null)
            return null;

        SpriteBatchFieldCache.Add(field);
        return field;
    }

    private static T? GetValueOrDefault<T>(this FieldInfo? field, object obj)
    {
        var value = field?.GetValue(obj);
        if (value is T value1)
            return value1;
        
        return default;
    }
    
    public static void UseNonPremultiplied(this SpriteBatch spriteBatch, Action draw)
    {
        var sortMode = GetSpriteBatchField("sortMode").GetValueOrDefault<SpriteSortMode>(spriteBatch);
        var blendState = GetSpriteBatchField("blendState").GetValueOrDefault<BlendState>(spriteBatch);
        var samplerState = GetSpriteBatchField("samplerState").GetValueOrDefault<SamplerState>(spriteBatch);
        var depthStencilState = GetSpriteBatchField("depthStencilState").GetValueOrDefault<DepthStencilState>(spriteBatch);
        var rasterizerState = GetSpriteBatchField("rasterizerState").GetValueOrDefault<RasterizerState>(spriteBatch);
        var effect = GetSpriteBatchField("customEffect").GetValueOrDefault<Effect>(spriteBatch);
        var transformMatrix = GetSpriteBatchField("transformMatrix").GetValueOrDefault<Matrix>(spriteBatch);
        spriteBatch.End();
        spriteBatch.Begin(sortMode, BlendState.NonPremultiplied, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        draw();
        spriteBatch.End();
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    }

    public static void SetBlendState(this SpriteBatch spriteBatch, BlendState state, out BlendState oldState)
    {
        var sortMode = GetSpriteBatchField("sortMode").GetValueOrDefault<SpriteSortMode>(spriteBatch);
        oldState = GetSpriteBatchField("blendState").GetValueOrDefault<BlendState>(spriteBatch);
        var samplerState = GetSpriteBatchField("samplerState").GetValueOrDefault<SamplerState>(spriteBatch);
        var depthStencilState = GetSpriteBatchField("depthStencilState").GetValueOrDefault<DepthStencilState>(spriteBatch);
        var rasterizerState = GetSpriteBatchField("rasterizerState").GetValueOrDefault<RasterizerState>(spriteBatch);
        var effect = GetSpriteBatchField("customEffect").GetValueOrDefault<Effect>(spriteBatch);
        var transformMatrix = GetSpriteBatchField("transformMatrix").GetValueOrDefault<Matrix>(spriteBatch);
        
        spriteBatch.End();
        spriteBatch.Begin(sortMode, state, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    }

    public static Vector2 Center(this UIElement element)
    {
        return new Vector2(element.Left.Pixels + element.Width.Pixels * 0.5f, element.Top.Pixels + element.Height.Pixels * 0.5f);
    }

    public static Vector2 Position(this UIElement element)
    {
        return new Vector2(element.Left.Pixels + element.MarginLeft, element.Top.Pixels + element.MarginTop);
    }

    public static Vector2 PixelSize(this UIElement element)
    {
        return new Vector2(element.Width.Pixels, element.Height.Pixels);
    }
    
    public static Vector2 DimensionSize(this UIElement element)
    {
        var dimension = element.GetDimensions();
        return new Vector2(dimension.Width, dimension.Height);
    }
    
    public static Vector2 PositionOfDimension(this UIElement element)
    {
        var dimension = element.GetDimensions();
        return dimension.Position();
    }
    
    public static Vector2 CenterOfDimension(this UIElement element)
    {
        var dimension = element.GetDimensions();
        return dimension.Center();
    }

    public static Vector2 GetScreenSize()
    {
        return new Vector2(Main.screenWidth, Main.screenHeight);
    }
    
    public static Vector2 GetScreenSizeWithUIScale()
    {
        return new Vector2(Main.screenWidth, Main.screenHeight) / Main.UIScale;
    }

    public static void RemoveNextFrame(this UIElement element)
    {
        Global.RemoveElementQueue.Enqueue(element);
    }

    public static void AppendNextFrame(this UIElement element)
    {
        Global.AppendElementQueue.Enqueue(element);
    }

    public static Vector2 DiffElementDimension(this UIElement element, UIElement other)
    {
        return element.DimensionSize() - other.DimensionSize();
    }

    public static bool InFront(this UIElement element)
    {
        if (element == null)
            return false;
        
        if (element.Parent is UIState state)
            return state.Children.LastOrDefault() == element;
                
        if (element.Parent != null)
            return element.Parent.InFront();

        return false;
    }

    public static UIElement FindTopElement(this UIElement element)
    {
        if (element.Parent == null)
            return element;
        
        if (element.Parent is UIState)
            return element;

        return element.Parent;
    }
}
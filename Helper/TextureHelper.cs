using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MSEnchant.Helper;

public static class TextureHelper
{
    public static Asset<Texture2D> LoadLocaleTexture(this string name,
        AssetRequestMode mode = AssetRequestMode.AsyncLoad)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var culture = LanguageManager.Instance.ActiveCulture;
        const string basePath = "MSEnchant/Assets/";
        var filePath = name;
        if (filePath.StartsWith(basePath))
            filePath = filePath.Replace(basePath, "");

        var requestPaths = new[]
        {
            Path.Combine(basePath, culture.Name, filePath),
            Path.Combine(basePath, "zh-Hans", filePath),
            Path.Combine(basePath, filePath)
        };
        foreach (var path in requestPaths)
        {
            if (ModContent.RequestIfExists<Texture2D>(path, out var r, mode))
                return r;
        }

        return null;
    }

    public static bool LoadLocaleTextureIfExists(this string name, out Asset<Texture2D> result,
        AssetRequestMode mode = AssetRequestMode.AsyncLoad)
    {
        try
        {
            result = LoadLocaleTexture(name, mode);
            return result != null;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public static Vector2 GetTextureOrigin(this string name, Vector2 defaultValue = default)
    {
        Global.CurrentCultureUISetting.Origins.TryGetValue(name, out var v);

        if (v == null)
            Global.FallbackCultureUISetting.Origins.TryGetValue(name, out v);

        return v != null ? new Vector2(v[0], v[1]) : defaultValue;
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.ModLoader;

namespace MSEnchant.Models;

public struct FontSettings
{
    [JsonProperty("bold")] public string Bold;

    [JsonProperty("regular")] public string Regular;

    [JsonProperty("requireFixHeight")] public bool RequireFixHeight;

    private Asset<DynamicSpriteFont> loadFont(string file) =>
        ModContent.Request<DynamicSpriteFont>($"MSEnchant/Assets/{file}", AssetRequestMode.ImmediateLoad);

    private Asset<DynamicSpriteFont> _boldAsset;
    public Asset<DynamicSpriteFont> BoldAsset => _boldAsset ??= loadFont(Bold);
    
    private Asset<DynamicSpriteFont> _regularAsset;
    public Asset<DynamicSpriteFont> RegularAsset => _regularAsset ??= loadFont(Regular);
}
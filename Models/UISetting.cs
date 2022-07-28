using System.Collections.Generic;
using Newtonsoft.Json;

namespace MSEnchant.Models;

public class UISetting
{
    [JsonProperty("origins")]
    public Dictionary<string, int[]> Origins = new Dictionary<string, int[]>();

    [JsonProperty("font")] public FontSettings Font = default;
}
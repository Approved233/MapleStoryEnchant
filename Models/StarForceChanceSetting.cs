using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace MSEnchant.Models;

public class StarForceChanceSetting : ICloneable
{
    public double FailKeep;
    public double FailDowngrade;
    public double FailDestroy;

    public bool AllowProtect;

    public StarForceChanceSetting(double failKeep = 0, double failDowngrade = 0, double failDestroy = 0,
        bool allowProtect = false)
    {
        FailKeep = failKeep;
        FailDowngrade = failDowngrade;
        FailDestroy = failDestroy;
        AllowProtect = allowProtect;
    }

    public double SuccessRate => 100.0 - (FailKeep + FailDowngrade + FailDestroy);

    public string[] DetailText
    {
        get
        {
            var r = new List<string>
            {
                Language.GetTextValue("Mods.MSEnchant.UIText.DetailText_SuccessRate", SuccessRate.ToString("0.0"))
            };
            if (FailKeep > 0)
                r.Add( Language.GetTextValue("Mods.MSEnchant.UIText.DetailText_KeepRate", FailKeep.ToString("0.0")));
            if (FailDowngrade > 0)
                r.Add(Language.GetTextValue("Mods.MSEnchant.UIText.DetailText_DowngradeRate", FailDowngrade.ToString("0.0")));
            if (FailDestroy > 0)
                r.Add(Language.GetTextValue("Mods.MSEnchant.UIText.DetailText_DestroyRate", FailDestroy.ToString("0.0")));

            return r.ToArray();
        }
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
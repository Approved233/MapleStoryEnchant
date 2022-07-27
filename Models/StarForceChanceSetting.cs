using System;
using System.Collections.Generic;

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
                $"成功概率： {SuccessRate:0.0}%"
            };
            if (FailKeep > 0)
                r.Add($"失败(保持)概率： {FailKeep:0.0}%");
            if (FailDowngrade > 0)
                r.Add($"失败(下降)概率： {FailDowngrade:0.0}%");
            if (FailDestroy > 0)
                r.Add($"损坏概率： {FailDestroy:0.0}%");

            return r.ToArray();
        }
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
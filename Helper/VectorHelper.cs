using Microsoft.Xna.Framework;
using Terraria;

namespace MSEnchant.Helper;

public static class VectorHelper
{
    public static Vector2 ForDraw(this Vector2 vec) => vec - Main.screenPosition;
}
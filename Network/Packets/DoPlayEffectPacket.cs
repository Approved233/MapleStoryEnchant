using System.IO;
using System.Linq;
using MSEnchant.Effects;
using MSEnchant.Helper;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace MSEnchant.Network.Packets;

public class DoPlayEffectPacket : MSNetPacket
{
    public EffectType Effect { get; set; }

    public string Sound { get; set; } = string.Empty;
    
    public int Target { get; set; }
    
    public DoPlayEffectPacket(PacketType type, int sender) : base(type, sender)
    {
    }

    public override void ReadPacket(BinaryReader reader)
    {
        Effect = (EffectType)reader.ReadInt32();
        Sound = reader.ReadString();
        Target = reader.ReadByte();
    }

    protected override void WritePacket(ModPacket writer)
    {
        writer.Write((int)Effect);
        writer.Write(Sound);
        writer.Write((byte)Target);
    }

    public override void HandlePacket(int whoAmI)
    {
        if (Target == whoAmI)
            return;
        
        var player = Main.player.ElementAtOrDefault(Target);
        if (player == null)
            return;

        player.PlayEffect(Effect);
        if (!string.IsNullOrEmpty(Sound))
        {
            try
            {
                var sound = new SoundStyle(Sound);
                SoundEngine.PlaySound(sound, player.position);
            }
            catch
            {
                // ignored
            }
        }
    }
}
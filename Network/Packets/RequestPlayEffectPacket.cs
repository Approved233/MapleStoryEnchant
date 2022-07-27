using System.IO;
using MSEnchant.Effects;
using Terraria.ModLoader;

namespace MSEnchant.Network.Packets;

public class RequestPlayEffectPacket : MSNetPacket
{
    public EffectType Effect { get; set; }
    
    public string Sound { get; set; }

    public RequestPlayEffectPacket(PacketType type, int sender) : base(type, sender)
    {
    }

    public override void ReadPacket(BinaryReader reader)
    {
        Effect = (EffectType)reader.ReadInt32();
        Sound = reader.ReadString();
    }

    protected override void WritePacket(ModPacket writer)
    {
        writer.Write((int)Effect);
        writer.Write(Sound);
    }

    public override void HandlePacket(int whoAmI)
    {
        Mod.SendPacket<DoPlayEffectPacket>(PacketType.DoPlayEffect, packet =>
        {
            packet.Effect = Effect;
            packet.Target = Sender;
            packet.Sound = Sound;
        });
    }
}
using System.IO;
using Terraria.ModLoader;

namespace MSEnchant.Network;

public abstract class MSNetPacket
{
    public PacketType Type { get; init; }
    
    public int Sender { get; set; }

    public MSEnchant Mod => Global.Mod;
    
    public MSNetPacket(PacketType type, int sender)
    {
        Type = type;
        Sender = sender;
    }

    public abstract void ReadPacket(BinaryReader reader);

    protected abstract void WritePacket(ModPacket writer);

    public abstract void HandlePacket(int whoAmI);

    public ModPacket GetPacket()
    {
        var p = Global.Mod.GetPacket();
        p.Write((int)Type);
        p.Write((byte)Sender);
        WritePacket(p);

        return p;
    }
}
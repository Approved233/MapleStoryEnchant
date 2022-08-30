using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MSEnchant.Effects;
using MSEnchant.Models;
using MSEnchant.Network;
using MSEnchant.Network.Packets;
using MSEnchant.UI.Control;
using Newtonsoft.Json;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MSEnchant
{
    public class MSEnchant : Mod
    {
        public override void Load()
        {
            Global.Mod = this;
            Global.Logger = Logger;

            LoadClientResources();
            RegisterPackets();
        }

        public override void Unload()
        {
            Global.Mod = null;
            Global.Logger = null;
            Global.DestroyEffect = null;
            Global.SuccessEffect = null;
            Global.FailEffect = null;
            Global.WorldEffectAnimations.Clear();
            Global.StarTexture = null;
            Global.GrayStarTexture = null;
            Global.SuperiorStarTexture = null;
            Global.PixelTexture = null;
            Global.DragEndSound = default;
            Global.TooltipTextures.Clear();
            Global.EnableEnchantUIKey = null;
            Global.StarScrollLootSettings.Clear();
            Global.CultureUISettings.Clear();
            Global.ItemGroupCache.Clear();
        }

        protected void LoadClientResources()
        {
            if (Main.dedServ)
                return;

            Global.UpdateScheduleQueue.Enqueue(() =>
            {
                Global.PixelTexture = new Texture2D(Main.spriteBatch.GraphicsDevice, 4, 4);
                var data = new Color[16];
                for (var index = 0; index < data.Length; ++index)
                    data[index] = Color.White;
                Global.PixelTexture.SetData(data);
            });

            Global.DragEndSound = new SoundStyle("MSEnchant/Assets/DragEnd");

            LoadCultureUISetting("en-US");
            LoadCultureUISetting("zh-Hans");
            
            Global.StarTexture = ModContent.Request<Texture2D>("MSEnchant/Assets/Item.Equip.Star.Star");
            Global.GrayStarTexture = ModContent.Request<Texture2D>("MSEnchant/Assets/Item.Equip.Star.Star0");
            Global.SuperiorStarTexture = ModContent.Request<Texture2D>("MSEnchant/Assets/Item.Equip.Star.Star1");

            ReadTooltipFrames();

            Global.SuccessEffect = new MSAnimationImage("enchantUI.successEffect", new[]
            {
                new MSFrameData(60, -263, -40),
                new MSFrameData(60, -217, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -183, -40),
                new MSFrameData(60, -185, -40),
                new MSFrameData(60, -189, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(90, -192, -131),
                new MSFrameData(90, -192, -133),
                new MSFrameData(90, -192, -138),
                new MSFrameData(90, -192, -143),
                new MSFrameData(90, -192, -143),
                new MSFrameData(60, -303, -143),
                new MSFrameData(60, -392, -140),
                new MSFrameData(60, -143, -125),
                new MSFrameData(60, -124, -124)
            })
            {
                Visible = false
            };

            Global.DestroyEffect = new MSAnimationImage("enchantUI.DestroyEffect", new[]
            {
                new MSFrameData(60, -263, -40),
                new MSFrameData(60, -217, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -183, -40),
                new MSFrameData(60, -185, -40),
                new MSFrameData(60, -189, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(90, -192, -41),
                new MSFrameData(90, -192, -41),
                new MSFrameData(90, -192, -41),
                new MSFrameData(90, -192, -40),
                new MSFrameData(90, -192, -40),
                new MSFrameData(60, -280, -40),
                new MSFrameData(60, -378, -56),
                new MSFrameData(60, -73, -28),
                new MSFrameData(60, -137, -128)
            })
            {
                Visible = false
            };

            Global.FailEffect = new MSAnimationImage("enchantUI.failEffect", new[]
            {
                new MSFrameData(60, -263, -40),
                new MSFrameData(60, -217, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -183, -40),
                new MSFrameData(60, -185, -40),
                new MSFrameData(60, -189, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(60, -192, -40),
                new MSFrameData(90, -192, -41),
                new MSFrameData(90, -192, -41),
                new MSFrameData(90, -192, -41),
                new MSFrameData(90, -192, -40),
                new MSFrameData(90, -192, -40),
                new MSFrameData(60, -280, -40),
                new MSFrameData(60, -378, -56),
                new MSFrameData(60, -74, -29),
                new MSFrameData(60, -137, -128)
            })
            {
                Visible = false
            };

            RegisterHotKeys();
            RegisterEffects();
        }

        protected void LoadCultureUISetting(string culture)
        {
            var bytes = ModContent.GetFileBytes($"MSEnchant/UI/Setting/{culture}.json");
            var json = Encoding.UTF8.GetString(bytes);
            Global.CultureUISettings[culture] = JsonConvert.DeserializeObject<UISetting>(json);
        }

        protected void RegisterHotKeys()
        {
            Global.EnableEnchantUIKey = KeybindLoader.RegisterKeybind(this, "Enable Enchant UI", Keys.O);
        }

        protected void RegisterEffects()
        {
            RegisterEffect(EffectType.ScrollSuccess, new WorldEffectAnimation(
                "MSEnchant/Assets/Scroll/Enchant.Success", new[]
                {
                    new MSFrameData(50, 16, 130),
                    new MSFrameData(50, 83, 195),
                    new MSFrameData(50, 83, 192),
                    new MSFrameData(50, 82, 189),
                    new MSFrameData(50, 53, 167),
                    new MSFrameData(50, 36, 151),
                    new MSFrameData(50, 39, 158),
                    new MSFrameData(50, 49, 170),
                    new MSFrameData(50, 62, 179),
                    new MSFrameData(50, 61, 183),
                    new MSFrameData(50, 63, 183),
                    new MSFrameData(50, 66, 187),
                    new MSFrameData(50, 69, 191),
                    new MSFrameData(50, 37, 153),
                    new MSFrameData(50, 41, 152),
                    new MSFrameData(50, 41, 149),
                    new MSFrameData(50, 44, 141),
                    new MSFrameData(50, 43, 134),
                    new MSFrameData(50, 41, 119),
                    new MSFrameData(50, 30, 104),
                    new MSFrameData(50, 30, 104)
                }));

            RegisterEffect(EffectType.ScrollFailure, new WorldEffectAnimation(
                "MSEnchant/Assets/Scroll/Enchant.Failure", new[]
                {
                    new MSFrameData(50, 16, 130),
                    new MSFrameData(50, 16, 130),
                    new MSFrameData(50, 16, 130),
                    new MSFrameData(50, 20, 141),
                    new MSFrameData(50, 20, 155),
                    new MSFrameData(50, 24, 154),
                    new MSFrameData(50, 31, 153),
                    new MSFrameData(50, 35, 153),
                    new MSFrameData(50, 38, 152),
                    new MSFrameData(50, 42, 151),
                    new MSFrameData(50, 42, 148),
                    new MSFrameData(50, 45, 139),
                    new MSFrameData(50, 44, 132),
                    new MSFrameData(50, 43, 123),
                    new MSFrameData(50, 62, 108),
                    new MSFrameData(50, 62, 108)
                }));
        }

        protected void RegisterEffect(EffectType type, WorldEffectAnimation animation)
        {
            Global.WorldEffectAnimations[type] = animation;
        }

        protected void ReadTooltipFrames()
        {
            Global.TooltipTextures.Clear();
            var loc = new[] { "n", "ne", "e", "se", "s", "sw", "w", "nw", "c", "cover" };
            foreach (var s in loc)
            {
                Global.TooltipTextures[s] = ModContent.Request<Texture2D>($"MSEnchant/Assets/Tooltip/Item.Frame2.{s}",
                    AssetRequestMode.ImmediateLoad);
            }
        }

        private Dictionary<PacketType, Type> packetsClientSide = new Dictionary<PacketType, Type>();
        private Dictionary<PacketType, Type> packetsServerSide = new Dictionary<PacketType, Type>();

        public PacketSide CurrentSide => Main.netMode == NetmodeID.Server ? PacketSide.Server : PacketSide.Client;

        public void SendPacket<T>(PacketType type, Action<T> doPacketInit, int target = -1, int ignore = -1)
            where T : MSNetPacket
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var side = Main.netMode == NetmodeID.Server ? PacketSide.Client : PacketSide.Server;
            var sender = Main.netMode == NetmodeID.Server ? -1 : Main.myPlayer;
            var packet = CreatePacket<T>(type, side, sender);
            if (packet == null)
                return;

            doPacketInit.Invoke(packet);

            var netPacket = packet.GetPacket();
            netPacket.Send(target, ignore);
        }

        public T? CreatePacket<T>(PacketType type, PacketSide side, int sender) where T : MSNetPacket
        {
            var dic = side == PacketSide.Client ? packetsClientSide : packetsServerSide;
            if (!dic.TryGetValue(type, out var packetType))
                return null;

            var packet = Activator.CreateInstance(packetType, type, sender) as T;
            return packet;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var type = (PacketType)reader.ReadInt32();
            var sender = (int)reader.ReadByte();

            var side = CurrentSide;
            var packet = CreatePacket<MSNetPacket>(type, side, sender);
            if (packet == null)
                return;

            packet.ReadPacket(reader);
            packet.HandlePacket(whoAmI);
        }

        protected void RegisterPackets()
        {
            RegisterPacket<RequestPlayEffectPacket>(PacketType.RequestPlayEffect, PacketSide.Server);
            RegisterPacket<DoPlayEffectPacket>(PacketType.DoPlayEffect, PacketSide.Client);
        }

        protected void RegisterPacket<T>(PacketType type, PacketSide side) where T : MSNetPacket
        {
            var dic = side == PacketSide.Client ? packetsClientSide : packetsServerSide;
            dic.Add(type, typeof(T));
        }
    }
}
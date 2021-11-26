using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Effects;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Kourindou
{
    public class KourindouPlayer : ModPlayer
    {
        // Determines the power mode of all the plushies
        public byte plushiePower;

        // When a player joins a world
        public override void OnEnterWorld(Player player)
        {
            // PlushiePower Client Config
            plushiePower = (byte)Kourindou.KourindouConfigClient.plushiePower;

            // If joining a server, also send a packet
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)Kourindou.KourindouMessageType.ClientConfig);
                packet.Write((byte)player.whoAmI);
                packet.Write((byte)plushiePower);
                packet.Send();
            }

            base.OnEnterWorld(player);
        }
    }
}
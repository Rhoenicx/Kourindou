using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Kourindou
{
    [Label("Settings")]
    public class KourindouConfigClient : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // Header Personalization
        [Header("Personalization of the Kourindou mod")]

        // Fumo Power Mode Setting
        [Range(0, 2)]
        [Increment(1)]
        [DefaultValue(1)]
        [Slider]
        [Label("Plushie Power Mode")]
        [Tooltip("0 = Regular, 1 = Magical, 2 = Overpowered")]
        public int plushiePower;

        //Old Textures
        [DefaultValue(false)]
        [Label("Use old textures")]
        public bool UseOldTextures;

        public override void OnLoaded()
        {
            Kourindou.KourindouConfigClient = this;
        }

        public override void OnChanged()
        {
            // When the settings are changed while playing, update the modplayer variable(s)
            if (!Main.gameMenu && Main.playerLoaded)
            {
                Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower = (byte)plushiePower;

                // When the setting is changed during multiplayer, also send a packet
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.ClientConfig);
                    packet.Write((byte)Main.LocalPlayer.whoAmI);
                    packet.Write((byte)plushiePower);
                    packet.Send();
                }

                ModContent.GetInstance<Kourindou>().LoadTextures();
            } 
        }
    }
}
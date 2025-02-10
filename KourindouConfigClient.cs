using System;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Kourindou
{
    //[Label("Settings")]
    public class KourindouConfigClient : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // Header Personalization
        //[Header("Personalization of the Kourindou mod")]

        // Plushie Power Mode Setting
        [Increment(1)]
        [Range((byte)0, (byte)3)]
        [DefaultValue(0)]
        [Slider]
        public byte plushiePower;

        //Old Textures
        [DefaultValue(false)]
        public bool UseOldTextures;

        public override void OnLoaded()
        {
            Kourindou.KourindouConfigClient = this;
        }

        public override void OnChanged()
        {
            // When the settings are changed while playing, update the modplayer variable(s)
            if (!Main.gameMenu)
            {
                Main.LocalPlayer.GetModPlayer<KourindouPlayer>().plushiePower = plushiePower;

                // When the setting is changed during multiplayer, also send a packet
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.ClientConfig);
                    packet.Write((byte)Main.LocalPlayer.whoAmI);
                    packet.Write((byte)plushiePower);
                    packet.Send();
                }

                Kourindou.SwitchPlushieTextures();
            } 
        }
    }
}
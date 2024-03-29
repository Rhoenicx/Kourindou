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
        [DefaultValue(false)]
        //[Label("Plushie special effects")]
        public bool plushiePower;

        //Old Textures
        [DefaultValue(false)]
        //[Label("Use old Fumomod textures (when available)")]
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
                    packet.Write((bool)plushiePower);
                    packet.Send();
                }

                Kourindou.SwitchPlushieTextures();
            } 
        }
    }
}
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Kourindou.Items
{
    public abstract class SecondaryFireItem : ModItem
    {
        // Allow for custom alt fire button = default right click
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        
        // Synchronize
        protected void SynchronizeSecondary(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte) KourindouMessageType.SecondaryFire);
                packet.Write((byte) player.whoAmI);
                packet.Send();
            }
        }
        
        // Change stats for normal use
        public virtual void SetNormalStats()
        {

        }

        // Change stats for alt use
        public virtual void SetSecondaryStats()
        {

        }
    }
}

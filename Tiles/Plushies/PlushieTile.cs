using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Tiles.Plushies
{
    public abstract class PlushieTile : ModTile
    {
        public override void NearbyEffects(int i, int j, bool closer)
        {

        }

        public override bool Drop(int i, int j)
        {
            return false;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) 
        {
            if (Kourindou.KourindouConfigClient.UseOldTextures)
            {
                frameXOffset += 36;
            }
            else
            {
                frameXOffset = 0;
            }
		}
    }
}
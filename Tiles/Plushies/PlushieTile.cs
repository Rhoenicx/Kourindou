using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Kourindou.Buffs;

namespace Kourindou.Tiles.Plushies
{
    public abstract class PlushieTile : ModTile
    {
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                Player player = Main.LocalPlayer;
                if (player.GetModPlayer<KourindouPlayer>().plushiePower == 1)
                {
                    player.AddBuff(BuffType<Buff_PlushieInRange>(), 5);
                }
            }
        }

        public override bool Drop(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) {
            num = 0;
		}
    }
}
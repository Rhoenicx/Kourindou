using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Kourindou
{
	class KourindouGlobalProjectile : GlobalProjectile
	{
        internal static int?[] ReimuPlushieHomingTarget = new int?[1024];

        public override void Kill(Projectile projectile, int timeLeft)
        {
            ReimuPlushieHomingTarget[projectile.whoAmI] = null;
        }
    }
}
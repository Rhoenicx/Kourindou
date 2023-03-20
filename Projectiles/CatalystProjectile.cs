using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Items;

namespace Kourindou.Projectiles
{
    public abstract class CatalystProjectile : ModProjectile
    {
        public Cast cast;

        public int LifeTime
        {
            get => (int)Projectile.ai[0]; 
            set => Projectile.ai[0] = value;
        }

        public override void AI()
        {
            Projectile.timeLeft = 5;


            // LifeTime has expired
            if (LifeTime <= 0)
            { 
                Projectile.Kill();
            }

            // On the end of the AI
            LifeTime--;
        }
    }
}

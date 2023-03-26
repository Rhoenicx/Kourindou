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

namespace Kourindou.Projectiles.Catalysts
{
    public class DebugCatalyst_Catalyst : CatalystProjectile
    {
        public override void SetDefaults()
        {
            // AI
            Projectile.aiStyle = -1;

            // Entity Interaction
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;

            // Hitbox
            Projectile.width = 8;
            Projectile.height = 8;

            // Movement
            Projectile.timeLeft = 5;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // Held offset - Texture width * 1.414f
            HeldProjectileOffset = new Vector2(25f, 25f);
            HeldProjectileRotation = 45f;
        }
    }
}

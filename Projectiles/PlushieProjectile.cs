using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles
{
    public abstract class PlushieProjectile : ModProjectile
    {
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
		    
			spriteBatch.Draw(
				texture, 
				projectile.Center - Main.screenPosition, 
				texture.Bounds, 
				lightColor, 
				projectile.rotation, 
				texture.Size() * 0.5f, 
				projectile.scale, 
				SpriteEffects.None, 
				0);
			return false;
		}

        public override bool OnTileCollide (Vector2 oldVelocity)
        {
            return false;
        }
    }
}
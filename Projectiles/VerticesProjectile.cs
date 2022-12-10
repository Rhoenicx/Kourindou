using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Projectiles
{
	public struct VertexInfo2 : IVertexType
	{
		private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
		{ 
			new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
			new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
		});

		public Vector2 Posistion;
		public Color Color;
		public Vector3 TexCoord;

		public VertexInfo2(Vector2 position, Vector3 texCoord, Color color)
		{
			Posistion = position;
			TexCoord = texCoord;
			Color = color;
		}

		public VertexDeclaration VertexDeclaration
		{
			get => _vertexDeclaration;
		}
	}

    public class VerticesProjectile : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vertices projectile");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1000;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			// AI
			Projectile.aiStyle = -1;

			// Entity Interaction
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;

			// Hitbox
			Projectile.width = 32;
			Projectile.height = 32;

			// Movement
			Projectile.timeLeft = 6000;
			Projectile.tileCollide = true;

			// Visual
			Projectile.scale = 1f;
		}

        public override bool PreDraw(ref Color lightColor)
        {
			// Create a new list of vertices
			List<VertexInfo2> vertices = new List<VertexInfo2>();

			// Fill the list with vertices, for each oldposition we add 2 vertices
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				// if the oldposition does not have a 'real' position yet we should skip it to prevent drawing a line to 0,0
				if (Projectile.oldPos[i] != Vector2.Zero)
				{
					vertices.Add(new VertexInfo2(Projectile.oldPos[i] - Main.screenPosition + new Vector2(24f, 0f).RotatedBy(Projectile.oldRot[i] + MathHelper.ToRadians(-90)), new Vector3(0, 0, 0), Color.Red));
					vertices.Add(new VertexInfo2(Projectile.oldPos[i] - Main.screenPosition + new Vector2(24f, 0f).RotatedBy(Projectile.oldRot[i] + MathHelper.ToRadians(90)), new Vector3(0, 1, 1), Color.Red));
				}
			}

			// End the default spriteBatch drawing
			Main.spriteBatch.End();

			// Start a new  spriteBatch with the following settings
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			
			// Set the texture
			Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("Kourindou/Projectiles/VerticesProjectileTex").Value;
			if (vertices.Count >= 3)
			{
				Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
        }

        public override void AI()
        {
			Projectile.ai[0]++;

			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);

			Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(10 * (float)Math.Sin(0.2 * Projectile.ai[0])));
        }
    }
}

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class FlandreScarlet_Plushie_Item : PlushieItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Red;

            // Hitbox
            Item.width = 32;
            Item.height = 32;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Tile placement fields
            Item.consumable = true;
            Item.createTile = TileType<FlandreScarlet_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<FlandreScarlet_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddRecipeGroup("Kourindou:Gemstone", 1)
                .AddIngredient(ItemType<RainbowFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemID.Silk, 3)
                .AddIngredient(ItemType<RedThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.50f;

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            // Increase crit by 10 percent
            player.GetCritChance(DamageClass.Generic) += 10;

            // Crit hits explode, excluding player pvp hits since they cannot crit
        }

        public override void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (hit.Crit)
            {
                SpawnExplosion(target, target.Center, hit.SourceDamage);
            }
        }

        public override void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (proj.type != ProjectileType<FlandreScarlet_Plushie_Explosion>() && hit.Crit)
            {
                SpawnExplosion(target, target.Center, hit.SourceDamage);
            }
        }

        public void SpawnExplosion(Entity victim, Vector2 position, int damage)
        {
            Projectile.NewProjectile(
                Item.GetSource_OnHit(victim),
                position,
                Vector2.Zero,
                ProjectileType<FlandreScarlet_Plushie_Explosion>(),
                damage * 3,
                0f,
                Main.myPlayer
            );
        }
    }
}
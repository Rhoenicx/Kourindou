using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Terraria.Map;

namespace Kourindou.Items.Plushies
{
    public class ToyosatomimiNoMiko_Plushie_Item : PlushieItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Purple;

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
            Item.createTile = TileType<ToyosatomimiNoMiko_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true; 
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ToyosatomimiNoMiko_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BrownFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<PurpleFabric>(), 1)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemType<BrownThread>(), 1)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // discount for listening to all people (at once)
            player.discountEquipped = true;
        }

        public override void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            SpawnBeam(target, target.Center, hit.SourceDamage, hit.Knockback, hit.Crit);
        }

        public override void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (proj.type != ProjectileType<ToyosatomimiNoMiko_Plushie_LaserBeam>())
            {
                SpawnBeam(target, target.Center, hit.SourceDamage, hit.Knockback, hit.Crit);
            }
        }

        public void SpawnBeam(Entity victim, Vector2 position, int damage, float knockback, bool crit = false)
        {
            Projectile.NewProjectile(
                Item.GetSource_OnHit(victim),
                position,
                Vector2.Zero,
                ProjectileType<ToyosatomimiNoMiko_Plushie_LaserBeam>(),
                damage,
                knockback,
                Main.myPlayer,
                0f,
                crit ? 1f : 0f
            );
        }
    }
}
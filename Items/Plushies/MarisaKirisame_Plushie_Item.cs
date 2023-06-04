using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Terraria.Map;

namespace Kourindou.Items.Plushies
{
    public class MarisaKirisame_Plushie_Item : PlushieItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Yellow;

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
            Item.createTile = TileType<MarisaKirisame_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {            
                shootSpeed = 8f;
                projectileType = ProjectileType<MarisaKirisame_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase magic damage by 25 percent
            player.GetDamage(DamageClass.Magic) += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase magic crit by 30%
            player.GetCritChance(DamageClass.Magic) += 15;

            // Reduced mana cost by 25 percent
            player.manaCost -= 0.25f;

            // On crit spawns a star projectile that is aimed at the target hit
        }

        public override void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (hit.Crit)
            {
                target.immune[player.whoAmI] = 0;
                SpawnStar(target, player.RotatedRelativePoint(player.MountedCenter), hit.SourceDamage);
            }
        }

        public override void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            if (hit.Crit)
            {
                target.immune[player.whoAmI] = 0;
                SpawnStar(target, player.RotatedRelativePoint(player.MountedCenter), hit.SourceDamage);
            }
        }

        public void SpawnStar(Entity victim,Vector2 position, int damage)
        {
            int star = Projectile.NewProjectile(
                Item.GetSource_OnHit(victim),
                position,
                Vector2.Normalize(Main.MouseWorld - position) * 10f,
                ProjectileID.StarCannonStar,
                damage,
                0f,
                Main.myPlayer
            );
            Main.projectile[star].netUpdate = true;
        }
    }
}
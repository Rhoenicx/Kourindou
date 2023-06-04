using System;
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
    public class Kourindou_RemiliaScarlet_Plushie_Item : PlushieItem
    {
        public const float HealPercentage = 0.05f;

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
            Item.createTile = TileType<Kourindou_RemiliaScarlet_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Kourindou_RemiliaScarlet_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<SkyBlueFabric>(), 1)
                .AddIngredient(ItemID.Silk, 3)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemType<RedThread>(), 2)
                .AddIngredient(ItemType<SkyBlueThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // All damage heals for 5% 
        }

        public override void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            Heal(player, hit.Damage);
        }

        public override void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            Heal(player, hit.Damage);
        }

        public static void Heal(Player player, int damage)
        {
            if (player.statLife < player.statLifeMax2)
            {
                int healAmount = (int)Math.Ceiling((double)((damage * HealPercentage) < player.statLifeMax2 - player.statLife ? (int)(damage * HealPercentage) : player.statLifeMax2 - player.statLife));
                player.statLife += healAmount;
                player.HealEffect(healAmount, true);
            }
        }
    }
}
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class HongMeiling_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hong Meiling Plushie");
            // Tooltip.SetDefault("The scarlet mansion's gatekeeper");
        }

        public override string AddEffectTooltip()
        {
            return "Tremendously increases life regen when not moving, but you are dazed and slowed \r\n" +
                    "+25% damage, +10% melee speed, 10% melee crit";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;

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
            Item.createTile = TileType<HongMeiling_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<HongMeiling_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.GoldCoin, 1)
                .AddIngredient(ItemType<GreenFabric>(), 2)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.GreenThread, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<YellowThread>(), 2)
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

            // Increase melee speed by 10 percent
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;

            // Increase melee critrate by 10 percent
            player.GetCritChance(DamageClass.Melee) += 10;

            if (player.velocity.Length() < 0.1f)
            {
                // Increase life regen by 20 additional points
                player.lifeRegen += 50;

                // add debuffs because you're not moving!
                player.AddBuff(BuffID.Dazed, 60);
                player.AddBuff(BuffID.Slow, 60);
            }
        }
    }
}
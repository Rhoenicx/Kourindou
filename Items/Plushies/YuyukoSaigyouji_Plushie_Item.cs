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
    public class YuyukoSaigyouji_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Yuyuko Saigyouji Plushie");
            Tooltip.SetDefault("The princess of the Netherworld");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;

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
            Item.createTile = TileType<YuyukoSaigyouji_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YuyukoSaigyouji_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieUpdateEquips(Player player)
        {
            // Increase minion knockback by 2
            player.GetKnockback(DamageClass.Summon) += 2;

            // Increase sentry slots by 3
            player.maxTurrets += 3;

            // Increase minion slots by 3
            player.maxMinions += 3;

            // Increased minion damage by 25 percent
            player.GetDamage(DamageClass.Summon) += 0.25f;

            // Well fed buff
            if (player.HasBuff(BuffID.WellFed) || player.HasBuff(BuffID.WellFed2) || player.HasBuff(BuffID.WellFed3))
            {
                // Increase attack damage by 25 percent
                player.GetDamage(DamageClass.Generic) += 0.25f;

                // Increase life regen by 1 point
                player.lifeRegen += 1;

                // Increase movement speed by 30 percent
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 1.3f;
                    player.maxRunSpeed *= 1.3f;
                }
                else
                {
                    player.maxRunSpeed *= 1.3f;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
            else
            {
                // Decrease attack damage by 25 percent
                player.GetDamage(DamageClass.Generic) -= 0.25f;

                // Decrease life regen by 1 point
                if (player.lifeRegen > 1)
                {
                    player.lifeRegen -= 1;
                }

                // Decrease movement speed by 30 percent
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 0.7f;
                    player.maxRunSpeed *= 0.7f;
                }
                else
                {
                    player.maxRunSpeed *= 0.7f;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
        }
        
        public override string AddEffectTooltip()
        {
            return "Increased minion knockback and minion & turret slots. + 25% minion damage\r\n" +
                    "With active food buff: +25% damage and increased movement speed. Reversed effects when no food buff!";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 1)
                .AddIngredient(ItemType<SkyBlueFabric>(), 3)
                .AddIngredient(ItemType<PinkFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<SkyBlueThread>(), 3)
                .AddIngredient(ItemID.PinkThread, 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}
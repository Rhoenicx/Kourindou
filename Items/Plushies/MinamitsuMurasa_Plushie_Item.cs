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
    public class MinamitsuMurasa_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Minamitsu Murasa Plushie");
            Tooltip.SetDefault("A phantom, and the Palanquin Ship's captain");
        }

        public override string AddEffectTooltip()
        {
            return "Ability to swim, doubled breath and permanent fishron mount speed buff\r\n" +
                    "+10% damage, when in water: +10% damage, +5% crit and increased hp regen";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Cyan;

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
            Item.createTile = TileType<MinamitsuMurasa_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {            
                shootSpeed = 8f;
                projectileType = ProjectileType<MinamitsuMurasa_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<BlueFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<BlueThread>(), 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 10 percent
            player.GetDamage(DamageClass.Generic) += 0.10f;

            // Increase Life regen by 1 point
            player.lifeRegen += 1;

            // Increase breathmax by 200 points (double)
            player.breathMax += 200;

            // Act like flipper accessory
            player.accFlipper = true;

            // Makes Fishron mount speed permanent
            if (player.mount.Type == MountID.CuteFishron && player.mount.Active)
            {
                player.MountFishronSpecialCounter += 1;
            }

            // Boosted stats in water
            if ((player.wet || player.honeyWet) && !player.lavaWet)
            {
                // Additional 10 percent damage
                player.GetDamage(DamageClass.Generic) += 0.10f;

                // Increase all crit by 5 points
                player.GetCritChance(DamageClass.Melee) += 5;
                player.GetCritChance(DamageClass.Magic) += 5;
                player.GetCritChance(DamageClass.Throwing) += 5;
                player.GetCritChance(DamageClass.Ranged) += 5;

                //Increase life regen by 5 points
                player.lifeRegen += 5;
            }
        }
    }
}
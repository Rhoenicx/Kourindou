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
    public class ReisenUdongeinInaba_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reisen Udongein Inaba Plushie");
            Tooltip.SetDefault("The Lunar War deserter. A self-proclaimed Earth rabbit");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.LightPurple;

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
            Item.createTile = TileType<ReisenUdongeinInaba_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ReisenUdongeinInaba_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase bullet damage by 20 percent
            player.GetDamage(DamageClass.Ranged) += 0.25f;

            // Increase ranged crit by 20 percent
            player.GetCritChance(DamageClass.Ranged) += 20;

            // During blood moon gain additional stats:
            if (Main.bloodMoon)
            {
                // Increase defense by 10 points
                player.statDefense += 10;

                // Increase movement speed by 100%
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 2f;
                    player.maxRunSpeed *= 2f;
                }
                else
                {
                    player.maxRunSpeed *= 2f;
                    player.accRunSpeed = player.maxRunSpeed;
                }

                // Increase armor penetration by 15 points
                player.GetArmorPenetration(DamageClass.Generic) += 15;

                // Increase damage by an additional 15 percent
                player.GetDamage(DamageClass.Generic) += 0.15f;

                // Increase life regen by 3 points
                player.lifeRegen += 3;

                // Increase max HP by 25 points
                player.statLifeMax2 += 25;

                // Permanent NightOwl buff
                player.AddBuff(BuffID.NightOwl, 60, true);
            }
        }

        public override string AddEffectTooltip()
        {
            return "During a blood moon gain a massive stat boost!\r\n" +
                    "+25% Ranged damage, +20% ranged crit, +15 penetration";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 1)
                .AddIngredient(ItemType<PinkFabric>(), 1)
                .AddIngredient(ItemType<PurpleFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemType<PurpleThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}
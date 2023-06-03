using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Terraria.WorldBuilding;

namespace Kourindou.Items.Plushies
{
    public class ShionYorigami_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Shion Yorigami Plushie");
            // Tooltip.SetDefault("A poverty god. It doesn't seem to take your money, though...");
        }

        public override string AddEffectTooltip()
        {
            return "Small chance to instantly kill the enemy hit!\r\n" +
                    "-25% damage and also cannot crit";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 0, 0, 1);
            Item.rare = ItemRarityID.Blue;

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
            Item.createTile = TileType<ShionYorigami_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ShionYorigami_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.CopperCoin, 1)
                .AddIngredient(ItemType<BlueFabric>(), 3)
                .AddIngredient(ItemType<SilverFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BlueThread>(), 2)
                .AddIngredient(ItemType<SilverThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Decrease damage by 25 percent
            player.GetDamage(DamageClass.Generic) -= 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Reduce crit chance by 100 percent
            player.GetCritChance(DamageClass.Generic) -= 100f;
        }

        public override void PlushieModifyHitNPCWithItem(Player player, Item item, NPC target, NPC.HitModifiers modifiers, int amountEquipped)
        {
            if ((int)Main.rand.Next(0, 1000) == 0)
            {
                modifiers.SourceDamage *= Main.rand.NextFloat(1000f, 1000000f);
            }

            modifiers.DisableCrit();
        }

        public override void PlushieModifyHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitModifiers modifiers, int amountEquipped)
        {
            if ((int)Main.rand.Next(0, 1000) == 0)
            {
                modifiers.SourceDamage *= Main.rand.NextFloat(1000f, 1000000f);
            }

            modifiers.DisableCrit();
        }

        public override void PlushieOnHurtPvp(Player targetPlayer, Player sourcePlayer, Player.HurtInfo info, int amountEquipped)
        {
            if ((int)Main.rand.Next(0, 1000) == 0)
            {
                info.SourceDamage = (int)(info.SourceDamage * Main.rand.NextFloat(1000f, 1000000f));
            }
        }
    }
}
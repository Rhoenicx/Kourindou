using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Kourindou.Buffs;

namespace Kourindou.Items.Plushies
{
    public class KaguyaHouraisan_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Kaguya Houraisan Plushie");
            // Tooltip.SetDefault("The exiled princess of the moon");
        }

        public override string AddEffectTooltip()
        {
            return "When killed heal all your HP, 60 second cooldown \r\n" +
                    "+25% damage, -25% damage taken, +15 defense";
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
            Item.createTile = TileType<KaguyaHouraisan_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<KaguyaHouraisan_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<PinkFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemID.PinkThread, 2)
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

            // Decrease incoming damage by 25 percent
            player.endurance += 0.25f;

            // Increase defense by 15 points
            player.statDefense += 15;

            // When you get damage that should kill you, heal for maxhp and get mortality debuff
        }

        public override bool PlushiePreKill(Player myPlayer, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, int amountEquipped)
        {
            if (myPlayer.HasBuff(BuffType<DeBuff_Mortality>()))
            {
                return true;
            }

            myPlayer.AddBuff(BuffType<DeBuff_Mortality>(), 3600, true);
            myPlayer.statLife += myPlayer.statLifeMax2;
            myPlayer.HealEffect(myPlayer.statLifeMax2, true);

            return false;
        }
    }
}
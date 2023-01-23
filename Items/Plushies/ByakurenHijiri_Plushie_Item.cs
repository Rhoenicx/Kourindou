using System;
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
    public class ByakurenHijiri_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Byakuren Hijiri Plushie");
            Tooltip.SetDefault("A Buddhist nun and magician. Currently, she's then Myouren Temple's head priest"); 
        }

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
            Item.createTile = TileType<ByakurenHijiri_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ByakurenHijiri_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieUpdateEquips(Player player)
        {
            // Increase Life regen by +1 
            player.lifeRegen += 1;

            // Reduce damage from all sources by 50%
            player.GetDamage(DamageClass.Generic) -= 0.50f;

            // but increase all melee damage with 100% (+50% offset from generic)
            player.GetDamage(DamageClass.Melee) += 1.50f;

            // Increase max HP by 25%
            player.statLifeMax2 = (int)Math.Floor((double)player.statLifeMax2 * 1.25);
        }

        public override void PlushiePostUpdateEquips(Player player)
        {
            // Set critrate to 0, generic has 4% as default
            player.GetCritChance(DamageClass.Default) = 0f;
            player.GetCritChance(DamageClass.Melee) = 0f;
            player.GetCritChance(DamageClass.Magic) = 0f;
            player.GetCritChance(DamageClass.Ranged) = 0f;
            player.GetCritChance(DamageClass.Summon) = 0f;
            player.GetCritChance(DamageClass.MagicSummonHybrid) = 0f;
        }

        public override string AddEffectTooltip()
        {
            return "Increases HP by 25%. Increases melee damage by 100%, but decrease all other damage by 50% \r\n"
                    + "Multiply knockback by 3. Doubles melee crit damage, but can no longer receive critrate buffs";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<BrownFabric>(), 2)
                .AddIngredient(ItemType<PurpleFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BrownThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}
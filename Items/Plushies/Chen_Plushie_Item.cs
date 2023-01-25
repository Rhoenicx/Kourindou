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
    public class Chen_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chen Plushie");
            Tooltip.SetDefault("The cutest shikigami's shikigami!");
        }

        public override string AddEffectTooltip()
        {
            return "Kills grant 25 HP, well fed and rapid healing for 12 seconds.\r\n" + "When hurt gain shadow dodge for 3 seconds. +25% damage";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Orange;

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
            Item.createTile = TileType<Chen_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;

        }
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Chen_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<GreenFabric>(), 2)
                .AddIngredient(ItemType<SilverFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<BrownFabric>(), 2)
                .AddIngredient(ItemType<RedThread>(), 2) 
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

            // Increase running speed by 9 mph
            player.maxRunSpeed += 0.5f;

            // Increase movement speed by 35 percent
            player.moveSpeed += 0.35f;

            // On Kill effect handled in player
        }

        public override void PlushieHurt(Player myPlayer, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter, int amountEquipped)
        {
            myPlayer.AddBuff(BuffID.ShadowDodge, 180);
        }

        public override void PlushieOnHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, int damage, float knockback, bool crit, int amountEquipped)
        {
            if (npc != null && npc.life <= 0 && !npc.friendly && npc.lifeMax > 5)
            {
                // On kill gain rapid healing, well fed and 25 health
                myPlayer.AddBuff(BuffID.RapidHealing, 720);
                myPlayer.AddBuff(BuffID.WellFed, 720);
                myPlayer.statLife += 25;
                myPlayer.HealEffect(25, true);
            }

            if (player != null && (player.statLife <= 0 || player.dead))
            {
                // On kill gain rapid healing, well fed and 25 health
                myPlayer.AddBuff(BuffID.RapidHealing, 720);
                myPlayer.AddBuff(BuffID.WellFed, 720);
                myPlayer.statLife += 25;
                myPlayer.HealEffect(25, true);
            }
        }
    }
}
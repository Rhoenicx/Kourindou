using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Kourindou.Items.Plushies;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles.Plushies.PlushieEffects;

namespace Kourindou
{
    public class KourindouGlobalNPC : GlobalNPC
    {
        
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (GetInstance<PlushieEquipSlot>().FunctionalItem.type == ItemType<PatchouliKnowledge_Plushie_Item>())
            {
                if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Throwing))
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (GetInstance<PlushieEquipSlot>().FunctionalItem.type == ItemType<ShionYorigami_Plushie_Item>())
            {
                if ((int)Main.rand.Next(1,1000) == 1)
                {
                    damage = (int)(damage * Main.rand.NextFloat(1000f,1000000f));
                }
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (GetInstance<PlushieEquipSlot>().FunctionalItem.type == ItemType<PatchouliKnowledge_Plushie_Item>())
            {
                if (projectile.CountsAsClass(DamageClass.Melee) || projectile.CountsAsClass(DamageClass.Ranged) || projectile.CountsAsClass(DamageClass.Throwing) || projectile.minion)
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (GetInstance<PlushieEquipSlot>().FunctionalItem.type == ItemType<ShionYorigami_Plushie_Item>())
            {
                if ((int)Main.rand.Next(1,1000) == 1)
                {
                    damage = (int)(damage * Main.rand.NextFloat(1000f,1000000f));
                }
            }

            // Disable crit for Flandre Scarlet Plushie effect
            if (projectile.type == ProjectileType<FlandreScarlet_Plushie_Explosion>())
            {
                crit = false;
            }
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            // Chen Plushie Effect
            if (GetInstance<PlushieEquipSlot>().FunctionalItem.type == ItemType<Chen_Plushie_Item>())
            {
                if (npc.life <= 0 && !npc.friendly)
                {
                    // On kill gain rapid healing, well fed and 25 health
                    player.AddBuff(BuffID.RapidHealing, 720);
                    player.AddBuff(BuffID.WellFed, 720);
                    player.statLife += 25;
                    player.HealEffect(25, true);
                }
            }    
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
        {
            // Chen Plushie Effect
            if (GetInstance<PlushieEquipSlot>().FunctionalItem.type == ItemType<Chen_Plushie_Item>())
            {
                if (npc.life <= 0 && !npc.friendly)
                {
                    // On kill gain rapid healing, well fed and 25 health
                    Main.player[projectile.owner].AddBuff(BuffID.RapidHealing, 720);
                    Main.player[projectile.owner].AddBuff(BuffID.WellFed, 720);
                    Main.player[projectile.owner].statLife += 25;
                    Main.player[projectile.owner].HealEffect(25, true);
                }
            }
        }

        // Remove items from shop
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            // Here we remove Pink and Black Thread items from the Clothier's shop
            if (type == NPCID.Clothier)
            {
                // Remove Item form shopinventory
                for (int i = 0; i < nextSlot; i++)
                {
                    if (shop.item[i].type == ItemID.BlackThread || shop.item[i].type == ItemID.PinkThread)
                    {
                        shop.item[i] = new Item();
                    }
                }

                // Remove empty slots from shopinventory
                for (int i = nextSlot; i >= 0 ; i--)
                {
                    if (shop.item[i].type == ItemID.None)
                    {
                        for (int a = i; a < nextSlot; a++)
                        {
                            shop.item[a] = shop.item[a + 1];
                        }
                    }
                }
            }
        }
    }
}
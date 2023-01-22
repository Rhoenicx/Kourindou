using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Kourindou.Items.Plushies;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using Kourindou.Buffs;
using Microsoft.Xna.Framework;

namespace Kourindou
{
    public class KourindouGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // Debuffs
        public bool DebuffMedicineMelancholy;
        public int DebuffMedicineMelancholyStacks;

        public override void ResetEffects(NPC npc)
        {
            DebuffMedicineMelancholy = false;

            if (!npc.HasBuff<DeBuff_MedicineMelancholy>())
            {
                DebuffMedicineMelancholyStacks = 0;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            // Medicine Melancholy debuff present increase damage
            if (DebuffMedicineMelancholy)
            {
                damage = (int)((float)damage * (1f + (0.04f * (DebuffMedicineMelancholyStacks + 1))));
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // Medicine Melancholy debuff present increase damage
            if (DebuffMedicineMelancholy)
            {
                damage = (int)((float)damage * (1f + (0.04f * (DebuffMedicineMelancholyStacks + 1))));
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (DebuffMedicineMelancholy && !npc.buffImmune[BuffID.Poisoned])
            {
                int damagePerSecond = 20;

                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                npc.lifeRegen -= damagePerSecond * (DebuffMedicineMelancholyStacks + 1) * (npc.HasBuff(BuffID.Venom)? 2 : 1);
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (DebuffMedicineMelancholy)
            {
                if (Main.rand.Next(0, 3) == 0)
                {
                    Dust.NewDust(
                        npc.position,
                        npc.width,
                        npc.height,
                        DustID.Cloud,
                        Main.rand.NextFloat(-2f, 2f),
                        Main.rand.NextFloat(-2f, 2f),
                        Main.rand.Next(10, 255),
                        new Color(193, 11, 136),
                        Main.rand.NextFloat(0.1f, 1f)
                    );
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

        // Change NPC loot
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (Kourindou.GensokyoLoaded)
            {
                if (npc.type == Kourindou.Gensokyo_AliceMargatroid_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<AliceMargatroid_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_Cirno_Type)
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new IsNotInHellBiome(), ItemType<Cirno_Plushie_Item>()));
                    npcLoot.Add(ItemDropRule.ByCondition(new IsInHellBiome(), ItemType<Tanned_Cirno_Plushie_Item>()));
                }

                if (npc.type == Kourindou.Gensokyo_EternityLarva_Type)
                {
                    //npcLoot.Add(ItemDropRule.Common(ItemType<>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_HinaKagiyama_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<HinaKagiyama_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_KaguyaHouraisan_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<KaguyaHouraisan_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_Kisume_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Kisume_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_LilyWhite_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<LilyWhite_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_MayumiJoutouguu_Type)
                {
                    //npcLoot.Add(ItemDropRule.Common(ItemType<>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_MedicineMelancholy_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<MedicineMelancholy_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_MinamitsuMurasa_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<MinamitsuMurasa_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_NitoriKawashiro_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<NitoriKawashiro_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_Rumia_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Rumia_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_SakuyaIzayoi_Type)
                {
                    npcLoot.Add(ItemDropRule.OneFromOptions(
                        ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>(),
                        ItemType<InuSakuyaIzayoi_Plushie_Item>()));
                }

                if (npc.type == Kourindou.Gensokyo_SeijaKijin_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<SeijaKijin_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_Sekibanki_Type)
                {
                    //npcLoot.Add(ItemDropRule.Common(ItemType<>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_TenshiHinanawi_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<TenshiHinanawi_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_ToyosatomimiNoMiko_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<ToyosatomimiNoMiko_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_UtsuhoReiuji_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<UtsuhoReiuji_Plushie_Item>(), 1));
                }

                if (npc.type == Kourindou.Gensokyo_CasterDoll_Type)
                {
                    npcLoot.Add(ItemDropRule.OneFromOptions(
                        100,
                        ItemType<BombDoll_Plushie_Item>(),
                        ItemType<ShanghaiDoll_Plushie_Item>()));
                }

                if (npc.type == Kourindou.Gensokyo_LancerDoll_Type)
                {
                    npcLoot.Add(ItemDropRule.OneFromOptions(
                        100,
                        ItemType<BombDoll_Plushie_Item>(),
                        ItemType<ShanghaiDoll_Plushie_Item>()));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Bone_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Bone_Fairy_Plushie_Item>(), 200));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Flower_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Flower_Fairy_Plushie_Item>(), 200));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Lava_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Lava_Fairy_Plushie_Item>(), 200));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Snow_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Snow_Fairy_Plushie_Item>(), 200));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Stone_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Stone_Fairy_Plushie_Item>(), 200));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Sunflower_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Sunflower_Fairy_Plushie_Item>(), 200));
                }

                if (npc.type == Kourindou.Gensokyo_Fairy_Thorn_Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Thorn_Fairy_Plushie_Item>(), 200));
                }
            }
        }
    }

    public class IsNotInHellBiome : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            if (!info.IsInSimulation)
            {
                return (int)(info.npc.position.Y / 16) <= Main.UnderworldLayer; ;
            }

            return !Main.player[Main.myPlayer].ZoneUnderworldHeight;
        }

        public bool CanShowItemDropInUI()
        {
            return !Main.player[Main.myPlayer].ZoneUnderworldHeight;
        }

        public string GetConditionDescription()
        {
            return "";
        }
    }
    public class IsInHellBiome : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            if (!info.IsInSimulation)
            {
                return (int)(info.npc.position.Y / 16) > Main.UnderworldLayer;
            }

            return Main.player[Main.myPlayer].ZoneUnderworldHeight;
        }

        public bool CanShowItemDropInUI()
        {
            return Main.player[Main.myPlayer].ZoneUnderworldHeight;
        }

        public string GetConditionDescription()
        {
            return "";
        }
    }
}
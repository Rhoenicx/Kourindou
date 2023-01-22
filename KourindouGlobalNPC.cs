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
            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<PatchouliKnowledge_Plushie_Item>()))
            {
                if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Throwing))
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<ShionYorigami_Plushie_Item>()) && (int)Main.rand.Next(1, 1000) == 1)
            {
                damage = (int)(damage * Main.rand.NextFloat(1000f,1000000f));
            }

            // Byakuren equipped = melee crits deal 200% DMG and 3 times more knockback
            if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()) && item.DamageType == DamageClass.Melee)
            {
                if (crit)
                {
                    damage *= 2;
                }
                knockback *= 3;
            }

            // Medicine Melancholy debuff present increase damage
            if (DebuffMedicineMelancholy)
            {
                damage = (int)((float)damage * (1f + (0.04f * (DebuffMedicineMelancholyStacks + 1))));
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // Hitting an NPC with Patchouli Knowledge Plushie equipped deals no damage except for magic type...
            if (Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<PatchouliKnowledge_Plushie_Item>()))
            {
                if (projectile.CountsAsClass(DamageClass.Melee) || projectile.CountsAsClass(DamageClass.Ranged) || projectile.CountsAsClass(DamageClass.Throwing) || projectile.minion)
                {
                    damage = 0;
                }
            }

            // Shion Yorigami random damage increase on NPC hits 0.1% chance
            if (Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<ShionYorigami_Plushie_Item>())&& (int)Main.rand.Next(1,1000) == 1)
            {
                damage = (int)(damage * Main.rand.NextFloat(1000f,1000000f));
            }

            // Disable crit for Flandre Scarlet Plushie effect
            if (projectile.type == ProjectileType<FlandreScarlet_Plushie_Explosion>())
            {
                crit = false;
            }

            // Byakuren equipped = melee crits deal 200% DMG and 3 times more knockback
            if (Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<ByakurenHijiri_Plushie_Item>()) && projectile.DamageType == DamageClass.Melee)
            {
                if (crit)
                {
                    damage *= 2;
                }
                knockback *= 3;
            }

            if (DebuffMedicineMelancholy)
            {
                damage = (int)((float)damage * (1f + (0.04f * (DebuffMedicineMelancholyStacks + 1))));
            }
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            // Chen Plushie Effect
            if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                if (npc.life <= 0 && !npc.friendly && npc.lifeMax > 5)
                {
                    // On kill gain rapid healing, well fed and 25 health
                    player.AddBuff(BuffID.RapidHealing, 720);
                    player.AddBuff(BuffID.WellFed, 720);
                    player.statLife += 25;
                    player.HealEffect(25, true);
                }
            }

            // Ran Plushie Effect
            if (player.GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                if (npc.life <= 0 && !npc.friendly && npc.lifeMax > 5)
                {
                    player.GetModPlayer<KourindouPlayer>().RanPlushie_EnemyKill();
                }
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
        {
            // Chen Plushie Effect
            if (Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<Chen_Plushie_Item>()))
            {
                if (npc.life <= 0 && !npc.friendly && npc.lifeMax > 5)
                {
                    // On kill gain rapid healing, well fed and 25 health
                    Main.player[projectile.owner].AddBuff(BuffID.RapidHealing, 720);
                    Main.player[projectile.owner].AddBuff(BuffID.WellFed, 720);
                    Main.player[projectile.owner].statLife += 25;
                    Main.player[projectile.owner].HealEffect(25, true);
                }
            }

            // Ran Plushie Effect
            if (Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().EquippedPlushies.Contains(ItemType<RanYakumo_Plushie_Item>()))
            {
                if (npc.life <= 0 && !npc.friendly && npc.lifeMax > 5)
                {
                    Main.player[projectile.owner].GetModPlayer<KourindouPlayer>().RanPlushie_EnemyKill();
                }
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
                Kourindou.Gensokyo.TryFind<ModNPC>("AliceMargatroid", out ModNPC Alice);
                if (Alice != null && npc.type == Alice.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<AliceMargatroid_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Cirno", out ModNPC Cirno);
                if (Cirno != null && npc.type == Cirno.Type)
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new IsNotInHellBiome(), ItemType<Cirno_Plushie_Item>()));
                    npcLoot.Add(ItemDropRule.ByCondition(new IsInHellBiome(), ItemType<Tanned_Cirno_Plushie_Item>()));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("EternityLarva", out ModNPC Larva);
                if (Larva != null && npc.type == Larva.Type)
                {
                    //npcLoot.Add(ItemDropRule.Common(ItemType<>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("HinaKagiyama", out ModNPC Hina);
                if (Hina != null && npc.type == Hina.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<HinaKagiyama_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("KaguyaHouraisan", out ModNPC Kaguya);
                if (Kaguya != null && npc.type == Kaguya.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<KaguyaHouraisan_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Kisume", out ModNPC Kisume);
                if (Kisume != null && npc.type == Kisume.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Kisume_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("LilyWhite", out ModNPC Lily);
                if (Lily != null && npc.type == Lily.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<LilyWhite_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("MayumiJoutouguu", out ModNPC Mayumi);
                if (Mayumi != null && npc.type == Mayumi.Type)
                {
                    //npcLoot.Add(ItemDropRule.Common(ItemType<>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("MedicineMelancholy", out ModNPC Medicine);
                if (Medicine != null && npc.type == Medicine.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<MedicineMelancholy_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("MinamitsuMurasa", out ModNPC Murasa);
                if (Murasa != null && npc.type == Murasa.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<MinamitsuMurasa_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("NitoriKawashiro", out ModNPC Nitori);
                if (Nitori != null && npc.type == Nitori.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<NitoriKawashiro_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Rumia", out ModNPC Rumia);
                if (Rumia != null && npc.type == Rumia.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Rumia_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("SakuyaIzayoi", out ModNPC Sakuya);
                if (Sakuya != null && npc.type == Sakuya.Type)
                {
                    npcLoot.Add(ItemDropRule.OneFromOptions(
                        ItemType<Kourindou_SakuyaIzayoi_Plushie_Item>(),
                        ItemType<InuSakuyaIzayoi_Plushie_Item>()));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("SeijaKijin", out ModNPC Seija);
                if (Seija != null && npc.type == Seija.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<SeijaKijin_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Sekibanki", out ModNPC Sekibanki);
                if (Sekibanki != null && npc.type == Sekibanki.Type)
                {
                    //npcLoot.Add(ItemDropRule.Common(ItemType<>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("TenshiHinanawi", out ModNPC Tenshi);
                if (Tenshi != null && npc.type == Tenshi.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<TenshiHinanawi_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("ToyosatomimiNoMiko", out ModNPC Miko);
                if (Miko != null && npc.type == Miko.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<ToyosatomimiNoMiko_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("UtsuhoReiuji", out ModNPC Utsuho);
                if (Utsuho != null && npc.type == Utsuho.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<UtsuhoReiuji_Plushie_Item>(), 1));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("CasterDoll", out ModNPC Caster);
                if (Caster != null && npc.type == Caster.Type)
                {
                    npcLoot.Add(ItemDropRule.OneFromOptions(
                        100,
                        ItemType<BombDoll_Plushie_Item>(),
                        ItemType<ShanghaiDoll_Plushie_Item>()));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("LancerDoll", out ModNPC Lancer);
                if (Lancer != null && npc.type == Lancer.Type)
                {
                    npcLoot.Add(ItemDropRule.OneFromOptions(
                        100,
                        ItemType<BombDoll_Plushie_Item>(),
                        ItemType<ShanghaiDoll_Plushie_Item>()));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Bone", out ModNPC Fairy_Bone);
                if (Fairy_Bone != null && npc.type == Fairy_Bone.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Bone_Fairy_Plushie_Item>(), 200));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Flower", out ModNPC Fairy_Flower);
                if (Fairy_Flower != null && npc.type == Fairy_Flower.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Flower_Fairy_Plushie_Item>(), 200));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Lava", out ModNPC Fairy_Lava);
                if (Fairy_Lava != null && npc.type == Fairy_Lava.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Lava_Fairy_Plushie_Item>(), 200));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Snow", out ModNPC Fairy_Snow);
                if (Fairy_Snow != null && npc.type == Fairy_Snow.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Snow_Fairy_Plushie_Item>(), 200));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Stone", out ModNPC Fairy_Stone);
                if (Fairy_Stone != null && npc.type == Fairy_Stone.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Stone_Fairy_Plushie_Item>(), 200));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Sunflower", out ModNPC Fairy_Sunflower);
                if (Fairy_Sunflower != null && npc.type == Fairy_Sunflower.Type)
                {
                    npcLoot.Add(ItemDropRule.Common(ItemType<Gensokyo_Sunflower_Fairy_Plushie_Item>(), 200));
                }

                Kourindou.Gensokyo.TryFind<ModNPC>("Fairy_Thorn", out ModNPC Fairy_Thorn);
                if (Fairy_Thorn != null && npc.type == Fairy_Thorn.Type)
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
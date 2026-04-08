using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
    public class CosmicDaybreak : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 66;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.ResearchUnlockCount = 1;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(1, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<CosmicDaybreakProj>();
            Item.shootSpeed = 16f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    public class CosmicDaybreakProj : ModProjectile
    {
        private int stuckTimer;
        private bool stuck;
        private bool exploded;
        private NPC target;
        private Vector2 offset;
        private int storedDamage;

        private const float SpearLength = 40f;
        private const int TimeBeforeFall = 50;
        private const int ExplosionDamage = 20;
        private int airTime;

        public override void SetDefaults()
        {
            Projectile.width = 59;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (exploded)
                return;

            if (!stuck)
            {
                if (airTime < 2)
                {
                    Projectile.tileCollide = false;
                }
                else
                {
                    Projectile.tileCollide = true;
                }

                Projectile.rotation = Projectile.velocity.ToRotation();

                airTime++;

                if (airTime > 50)
                    Projectile.velocity.Y += 0.15f;

                Projectile.velocity *= 0.995f;

                return;
            }

            if (target == null || !target.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = target.Center + offset;
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            Projectile.damage = 0;

            Projectile.rotation = offset.ToRotation() + MathHelper.Pi;

            stuckTimer++;
            if (stuckTimer >= 90)
                Explode();
        }

        public override bool? CanDamage()
        {
            return !stuck;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Vector2 tip = GetTip();

            if (Vector2.Distance(target.Center, tip) > 20f)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            Vector2 tip = GetTip();

            return target.Hitbox.Contains(tip.ToPoint());
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck && !exploded && airTime > 2)
            {
                Explode();
            }

            return false;
        }

        public override void OnHitNPC(NPC npc, NPC.HitInfo hit, int damageDone)
        {
            if (stuck || exploded)
                return;

            stuck = true;
            target = npc;
            offset = Projectile.Center - npc.Center;
            storedDamage = Projectile.damage;

            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.damage = 0;

            npc.buffImmune[BuffID.Poisoned] = false;
            npc.AddBuff(BuffID.Poisoned, 150);

            Projectile.netUpdate = true;
        }

        private Vector2 GetTip()
        {
            return Projectile.Center + new Vector2(Projectile.width * 0.5f, 0).RotatedBy(Projectile.rotation);
        }

        private void Explode()
        {
            if (exploded)
                return;

            exploded = true;

            Vector2 center = GetTip();

            int damage = ExplosionDamage;
            float radius = 80f;

            SoundEngine.PlaySound(SoundID.Item14, center);

            for (int i = 0; i < 18; i++)
            {
                int dust = Dust.NewDust(center - new Vector2(8f, 8f), 16, 16, DustID.Torch);
                Main.dust[dust].velocity *= 2.2f;
                Main.dust[dust].scale = 1.4f;
            }

            for (int i = 0; i < 8; i++)
            {
                int dust = Dust.NewDust(center - new Vector2(8f, 8f), 16, 16, DustID.Smoke);
                Main.dust[dust].velocity *= 1.2f;
                Main.dust[dust].scale = 1.2f;
            }

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                if (Vector2.Distance(npc.Center, center) > radius)
                    continue;

                int dir = npc.Center.X >= center.X ? 1 : -1;

                if (npc.type == NPCID.TargetDummy)
                {
                    npc.SimpleStrikeNPC(damage, dir, false, 0f);
                }
                else
                {
                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = damage,
                        Knockback = 1f,
                        HitDirection = dir
                    };
                    npc.StrikeNPC(hitInfo, true, true);
                }
            }

            if (target != null && target.active)
                target.immune[Projectile.owner] = 0;

            Projectile.Kill();
        }
    }
}
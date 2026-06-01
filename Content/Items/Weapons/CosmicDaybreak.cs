using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ConstellationsOfOrion.Graphics;
using MonoMod.Utils;
using System.Linq;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
    public class CosmicDaybreak : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 38;
            Item.height = 38;
            Item.ResearchUnlockCount = 1;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(1, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CosmicDaybreakProj>();
            Item.shootSpeed = 20f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Materials.ConstelliteBar>(), 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("ConstellationsOfOrion/Content/Items/Weapons/CosmicDaybreakGlowmask").Value;
            spriteBatch.Draw(
                texture,
                Item.Center - Main.screenPosition,
                new Rectangle(0, 0, texture.Width, texture.Height),
                Color.White,
                rotation,
                texture.Size() * 0.5f,
                scale,
                SpriteEffects.None,
                0
            );
        }
    }

    public class CosmicDaybreakProj : ModProjectile
    {
        private const int AnimationTime = 30;
        private const int MaxHoldTime = 12;
        private const int ExplosionRadius = 80;
        private const int ExplosionDamage = 20;
        private const int StuckDuration = 90;
        private const int TileCollideDelay = 2;
        private const int GravityDelay = 50;
        private const float GravityStrength = 0.15f;
        private const float VelocityDamping = 0.995f;
        private const float SwingBackFraction = 0.5f;
        private const float SwingForwardFraction = 6f;

        private static readonly Color FlameColor = Color.White;
        private static readonly Color InnerColor = Color.OrangeRed;
        private static readonly Color OuterColor = new(114, 47, 110);
        private static readonly Vector3 LightColor = Color.OrangeRed.ToVector3() * 0.25f;
        private const float FlameSpeed = 20f;

        private Vector2 shootVelocity;
        private float heldRotation;
        private float swingBackRotation;
        private float endRotation;
        private int holdTimer;
        private int airTime;
        private int stuckTimer;
        private bool stuck;
        private bool exploded;
        private NPC target;
        private Vector2 stuckOffset;
        private int storedDamage;

        private int ShootDirection => shootVelocity.X >= 0 ? 1 : -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override bool ShouldUpdatePosition() => holdTimer > MaxHoldTime;

        public override void OnSpawn(IEntitySource source)
        {
            shootVelocity = Projectile.velocity;
            int dir = ShootDirection;

            float aimAngle = shootVelocity.ToRotation() - MathHelper.Pi * dir;
            if (dir == -1)
                aimAngle += MathHelper.Pi;

            heldRotation = aimAngle;
            swingBackRotation = aimAngle - MathHelper.PiOver4 * SwingBackFraction * dir;
            endRotation = aimAngle + MathHelper.Pi * dir;
        }

        public override void AI()
        {
            if (!Projectile.TryGetOwner(out Player player))
            {
                Projectile.Kill();
                return;
            }

            UpdateSwingAnimation(player);

            // Lighting is always applied once the projectile is in flight
            if (holdTimer > MaxHoldTime)
                Lighting.AddLight(Projectile.Center, LightColor);

            if (exploded)
                return;

            if (stuck)
                UpdateStuck();
            else
                UpdateFlight();
        }

        private void UpdateSwingAnimation(Player player)
        {
            if (holdTimer > AnimationTime)
                return;

            holdTimer++;
            int dir = ShootDirection;
            player.ChangeDir(dir);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, heldRotation);

            if (holdTimer < MaxHoldTime)
            {
                // Hold phase: position at hand, animate swing-back
                Projectile.Center = player.GetFrontHandPosition(player.compositeFrontArm.stretch, heldRotation);
                Projectile.rotation = Projectile.velocity.ToRotation();

                if (holdTimer < SwingForwardFraction)
                {
                    float progress = holdTimer / SwingForwardFraction;
                    heldRotation = MathHelper.Lerp(heldRotation, swingBackRotation, progress);
                }
            }
            else if (holdTimer == MaxHoldTime)
            {
                // Release: launch projectile
                Projectile.velocity = shootVelocity;
            }
            else
            {
                // Follow-through swing animation
                float progress = (holdTimer - MaxHoldTime) / (float)(AnimationTime - MaxHoldTime);
                float loggedProgress = (float)Math.Log10(1 + 9 * progress);
                heldRotation = MathHelper.Lerp(heldRotation, endRotation, loggedProgress);
            }
        }

        private void UpdateFlight()
        {
            // Delay tile collision
            Projectile.tileCollide = airTime >= TileCollideDelay;
            Projectile.rotation = Projectile.velocity.ToRotation();
            airTime++;

            if (airTime > GravityDelay)
                Projectile.velocity.Y += GravityStrength;

            Projectile.velocity *= VelocityDamping;
        }

        private void UpdateStuck()
        {
            if (target == null || !target.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = target.Center + stuckOffset;
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            Projectile.damage = 0;

            stuckTimer++;
            if (stuckTimer >= StuckDuration)
                Explode();
        }

        public override bool? CanDamage() => stuck ? false : null;

        public override bool? CanHitNPC(NPC target)
        {
            // Only the tip of the spear can connect
            return target.Hitbox.Contains(GetTip().ToPoint());
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Penalize non-tip hits
            if (Vector2.Distance(target.Center, GetTip()) > 20f)
                modifiers.FinalDamage *= 0.5f;
        }

        public override void OnHitNPC(NPC npc, NPC.HitInfo hit, int damageDone)
        {
            if (stuck || exploded)
                return;

            stuck = true;
            target = npc;
            storedDamage = Projectile.damage;

            // Embed spear slightly into NPC
            Projectile.Center += Projectile.velocity.SafeNormalize(Vector2.Zero) * 6f;
            stuckOffset = Projectile.Center - npc.Center;

            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.damage = 0;

            // Apply poison
            npc.buffImmune[BuffID.Poisoned] = false;
            npc.AddBuff(BuffID.Poisoned, 150);

            Projectile.netUpdate = true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck && !exploded && holdTimer > MaxHoldTime)
                Explode();

            return false;
        }

        private Vector2 GetTip() =>
            Projectile.Center + new Vector2(Projectile.width * 0.5f, 0).RotatedBy(Projectile.rotation);

        private void Explode()
        {
            if (exploded)
                return;

            exploded = true;
            Vector2 center = GetTip();

            SoundEngine.PlaySound(SoundID.Item14, center);
            SpawnExplosionDust(center);
            DamageNearbyNPCs(center);

            // Reset stuck target immunity so damage registers correctly
            if (target != null && target.active)
                target.immune[Projectile.owner] = 0;

            Projectile.Kill();
        }

        private static void SpawnExplosionDust(Vector2 center)
        {
            Vector2 dustOrigin = center - new Vector2(8f);

            for (int i = 0; i < 18; i++)
            {
                int d = Dust.NewDust(dustOrigin, 16, 16, DustID.Torch);
                Main.dust[d].velocity *= 2.2f;
                Main.dust[d].scale = 1.4f;
            }

            for (int i = 0; i < 8; i++)
            {
                int d = Dust.NewDust(dustOrigin, 16, 16, DustID.Smoke);
                Main.dust[d].velocity *= 1.2f;
                Main.dust[d].scale = 1.2f;
            }
        }

        private void DamageNearbyNPCs(Vector2 center)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                if (Vector2.Distance(npc.Center, center) > ExplosionRadius)
                    continue;

                int dir = npc.Center.X >= center.X ? 1 : -1;

                if (npc.type == NPCID.TargetDummy)
                {
                    npc.SimpleStrikeNPC(ExplosionDamage, dir, false, 0f);
                }
                else
                {
                    var knockBack = 1f * npc.knockBackResist;
                    npc.StrikeNPC(new NPC.HitInfo
                    {
                        Damage = ExplosionDamage,
                        Knockback = knockBack,
                        HitDirection = dir
                    }, true, true);
                }
            }
        }

        // --- Drawing ---

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bool inFlight = target == null && holdTimer > MaxHoldTime;

            if (inFlight)
                DrawFlameTrail();
            else
                ClearOldPositions();

            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            DrawGlowmask();
        }

        private void DrawFlameTrail()
        {
            var flameShader = GameShaders.Misc["ConstellationsOfOrion:Flame"];
            flameShader.UseColor(FlameColor);
            flameShader.UseSecondaryColor(InnerColor);
            flameShader.UseOpacity(0.8f);
            flameShader.uLightSource(OuterColor.ToVector3());
            flameShader.UseShaderSpecificData(new Vector4(FlameSpeed, 12f, 16f, 1f));

            Vector2 halfSize = Projectile.Size * 0.5f;
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
            float tipPadding = 32f * MathHelper.Clamp(Projectile.velocity.Length() / 10f, 0, 1);
            Vector2 tipOffset = direction * (Projectile.width * 0.5f + tipPadding);

            // Build positions without LINQ to avoid per-frame allocation
            Vector2[] oldPos = Projectile.oldPos;
            Vector2[] positions = new Vector2[oldPos.Length];
            for (int i = 0; i < oldPos.Length; i++)
                positions[i] = oldPos[i] + halfSize + tipOffset;

            var settings = new PrimitiveSettings(
                flameShader,
                x => (1 - x * x) * Projectile.height * 2f,
                _ => Color.White
            );

            PrimitiveRenderer.RenderTrail(positions, settings, null, true, true);

            // Ghost trail (3 fading copies)
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 texCenter = texture.Size() * 0.5f;

            for (int i = 0; i < 3; i++)
            {
                float alpha = (1f - i / 3f) * 0.5f;
                Vector2 drawPos = oldPos[i] + halfSize - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, Color.White * alpha, Projectile.oldRot[i], texCenter, Projectile.scale, SpriteEffects.None, 0);
            }
        }

        private void ClearOldPositions()
        {
            Vector2[] oldPos = Projectile.oldPos;
            for (int i = 0; i < oldPos.Length; i++)
                oldPos[i] = Vector2.Zero;
        }

        private void DrawGlowmask(float scaleMul = 1f, float alphaMult = 1f)
        {
            Texture2D glowmask = ModContent.Request<Texture2D>("ConstellationsOfOrion/Content/Items/Weapons/CosmicDaybreakProjGlowmask").Value;
            Main.EntitySpriteDraw(
                glowmask,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, glowmask.Width, glowmask.Height),
                Color.White * alphaMult,
                Projectile.rotation,
                glowmask.Size() * 0.5f,
                Projectile.scale * scaleMul,
                SpriteEffects.None,
                0
            );
        }
    }
}
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using JetBrains.Annotations;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Humanizer;
using System;
using ConstellationsOfOrion.Content.Projectiles;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
    public class AetherGun : ModItem
    {
        private static int ProjectileType => ModContent.ProjectileType<AetherGunHeld>();
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Aether Gun");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 62;
            Item.DamageType = DamageClass.Ranged;

            Item.width = 50;
            Item.height = 26;

            Item.useTime = 6;
            Item.useAnimation = 6;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.knockBack = 3f;

            Item.value = Item.sellPrice(0, 12, 0, 0);
            Item.rare = ItemRarityID.LightRed;

            Item.UseSound = SoundID.Item158;
            Item.autoReuse = true;

            Item.channel = true;

            Item.useAmmo = AmmoID.Bullet;

            Item.shoot = ProjectileType;
            Item.shootSpeed = 16f;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => false;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType] < 1;

        public override bool Shoot(Player player,
            Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            Projectile.NewProjectile(
                source,
                position,
                velocity,
                ProjectileType,
                damage,
                knockback,
                player.whoAmI
            );

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class AetherGunHeld : ModProjectile
    {
        private int ChargeTime
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float timingModifier = 1f;
        private const int MaxChargeTime = 30;
        private const int RecoilTime = 10;
        private static int ItemType => ModContent.ItemType<AetherGun>();

        public override string Texture => "ConstellationsOfOrion/Content/Items/Weapons/AetherGun";
        public override void SetStaticDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            if (!Projectile.TryGetOwner(out Player player))
            {
                Projectile.Kill();
                return;
            }

            var playerDisabled = player.dead || !player.active || player.CCed || player.noItems;

            if (player.HeldItem.type == ItemType && !playerDisabled && player.channel)
            {
                Projectile.timeLeft = 2;
            }

            UpdatePlayer(player);

            if (++ChargeTime >= MaxChargeTime * timingModifier &&
                player.PickAmmo(player.HeldItem, out _, out _, out _, out _, out int usedAmmoId) &&
                player.HasItem(usedAmmoId)
            )
            {
                UpdateShoot(player);
                Projectile.frameCounter++;
            }
            else
            {
                UpdateAim(player);
            }
        }
        private void UpdateAim(Player player)
        {
            var center = player.RotatedRelativePoint(player.MountedCenter);
            var direction = center.DirectionTo(Main.MouseWorld);
            Projectile.velocity = direction;
            Projectile.rotation = direction.ToRotation();

            var offset = Projectile.velocity * -12f;

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true) + offset;
        }

        private void UpdatePlayer(Player player)
        {
            var aimDirection = Projectile.velocity.X > 0 ? 1 : -1;
            player.heldProj = Projectile.whoAmI;
            player.itemRotation = (Projectile.velocity * aimDirection).ToRotation();
            Projectile.direction = aimDirection;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.ChangeDir(aimDirection);
        }

        private float recoilStartRotation;
        private Vector2 recoilStartCenter;

        private void UpdateShoot(Player player)
        {
            var center = player.RotatedRelativePoint(player.MountedCenter, true) + Projectile.velocity * -12f;
            var totalRecoilTime = MathF.Max(RecoilTime * timingModifier, 2f);

            if (Projectile.frameCounter == 0)
                InitShot(player, center);

            ApplyRecoilTransform(center, totalRecoilTime);

            if (Projectile.frameCounter >= totalRecoilTime)
                FinishRecoil();
        }

        private void InitShot(Player player, Vector2 center)
        {
            recoilStartRotation = Projectile.rotation;
            recoilStartCenter = center;

            if (!player.PickAmmo(player.HeldItem, out var ammoType, out var speed, out var damage, out var knockback, out int usedAmmoId)
                || !player.HasItem(usedAmmoId))
                return;

            var useAmmo = ammoType == ProjectileID.Bullet
                ? ModContent.ProjectileType<ConstelliteBulletProj>()
                : ammoType;

            var spread = MathHelper.ToRadians(8) * timingModifier;
            var shootVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-spread, spread)) * speed;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center + Projectile.velocity * 16f,
                shootVelocity,
                useAmmo,
                Projectile.damage + damage,
                Projectile.knockBack + knockback,
                player.whoAmI
            );

            player.ConsumeItem(usedAmmoId);
        }

        private void ApplyRecoilTransform(Vector2 center, float totalRecoilTime)
        {
            var shootRotation = Projectile.velocity.ToRotation();
            var recoilAngle = MathHelper.ToRadians(14) * Projectile.direction * timingModifier;

            var kickTime = MathF.Max(totalRecoilTime * 2f / 3f, 1f);
            var returnTime = MathF.Max(totalRecoilTime - kickTime, 1f);

            float t;

            if (totalRecoilTime <= 2f)
            {
                t = Projectile.frameCounter == 0 ? 1f : 0f;
            }
            else if (Projectile.frameCounter < kickTime)
            {
                t = MathF.Sqrt(Projectile.frameCounter / kickTime);
            }
            else
            {
                var prog = (Projectile.frameCounter - kickTime) / returnTime;
                t = 1f - prog * prog * (3f - 2f * prog);
            }

            Projectile.rotation = MathHelper.Lerp(shootRotation, shootRotation - recoilAngle, t);
            Projectile.Center = center + Projectile.velocity * -t; // pure offset from live center
        }

        private void FinishRecoil()
        {
            Projectile.frameCounter = -1;
            ChargeTime = 0;
            timingModifier = MathF.Max(timingModifier - 0.05f, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = TextureAssets.Projectile[Type].Value;
            var origin = new Vector2(0, texture.Height / 2f);
            var se = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                se,
                0
            );

            return false;
        }
    }
}

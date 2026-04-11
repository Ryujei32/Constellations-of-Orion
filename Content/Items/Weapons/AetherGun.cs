using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using JetBrains.Annotations;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Humanizer;
using System;

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

        private const int MaxChargeTime = 60;

        private const int RecoilTime = 20;
        private static int ItemType => ModContent.ItemType<AetherGun>();

        public override string Texture => "ConstellationsOfOrion/Content/Items/Weapons/AetherGun";
        public override void SetStaticDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 40;
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

            if (++ChargeTime >= MaxChargeTime)
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

        private void UpdateShoot(Player player)
        {
            var recoilAngle = MathHelper.ToRadians(14) * Projectile.direction;
            var shootRotation = Projectile.velocity.ToRotation();

            var twoThirdsRecoilTime = RecoilTime * 2f / 3f;
            var oneThirdRecoilTime = RecoilTime / 3f;

            var offset = Projectile.velocity * -12f;
            var center = player.RotatedRelativePoint(player.MountedCenter, true) + offset;

            if (Projectile.frameCounter == 0)
            {
                var shootVelocity = Projectile.velocity * 16f;
                var source = Projectile.GetSource_FromThis();
                Projectile.NewProjectile(
                    source,
                    Projectile.Center + Projectile.velocity * 16f,
                    shootVelocity,
                    ProjectileID.WoodenArrowFriendly,
                    Projectile.damage,
                    Projectile.knockBack,
                    player.whoAmI
                );
            }
            if (Projectile.frameCounter < twoThirdsRecoilTime)
            {
                var prog = Projectile.frameCounter / twoThirdsRecoilTime;
                prog = MathF.Log(1 + 9 * prog);

                Projectile.rotation = MathHelper.Lerp(
                    Projectile.rotation,
                    shootRotation - recoilAngle,
                    prog
                );

                Projectile.Center = Vector2.Lerp(
                    center,
                    center - Projectile.velocity * 4f,
                    prog
                );
                return;
            }
            else
            {
                var prog = (Projectile.frameCounter - twoThirdsRecoilTime) / oneThirdRecoilTime;
                Projectile.rotation = MathHelper.Lerp(
                    Projectile.rotation,
                    shootRotation,
                    prog * prog
                );
                Projectile.Center = Vector2.Lerp(
                    center - Projectile.velocity * 4f,
                    center,
                    prog * prog
                );
            }

            if (Projectile.frameCounter >= RecoilTime)
            {
                Projectile.frameCounter = -1;
                ChargeTime = 0;
            }
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

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles
{
	public class CosmicCrushProj : ModProjectile
	{
		public override void SetDefaults()
		{
			// 1.4.4.9 correct method:
			Projectile.CloneDefaults(ProjectileID.DripplerFlail);

			// Makes it behave exactly like Drippler Crippler
			AIType = ProjectileID.DripplerFlail;

			Projectile.width = 28;
			Projectile.height = 28;

			Projectile.friendly = true;
			Projectile.hostile = false;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.MeleeNoSpeed;

			Projectile.scale = 1f;
		}

		public override void AI()
		{
			// Pink cosmic dust
			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.PinkTorch
				);

				dust.noGravity = true;
				dust.scale = 1.15f;
				dust.velocity *= 0.35f;
			}

			// Golden sparkle dust
			if (Main.rand.NextBool(7))
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.GoldFlame
				);

				dust.noGravity = true;
				dust.scale = 1f;
				dust.velocity *= 0.25f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			for (int i = 0; i < 8; i++)
			{
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.t_Slime
				);
			}

			return base.OnTileCollide(oldVelocity);
		}
	}
}
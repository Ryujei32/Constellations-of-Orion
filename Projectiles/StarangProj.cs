using System.Linq;
using ConstellationsOfOrion.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles
{
	public class StarangProj : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 26;
			Projectile.height = 26;

			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;

			Projectile.penetrate = -1;

			Projectile.aiStyle = ProjAIStyleID.Boomerang; // ⭐ TRUE BOOMERANG
			AIType = ProjectileID.EnchantedBoomerang;

			Projectile.timeLeft = 300;
		}

		public override void AI()
		{
			Projectile.rotation += 0.45f * Projectile.direction;

			// ⭐ CONSTELLATION DUST
			if (Main.rand.NextBool(4))
			{
				int dustType = Main.rand.NextBool(2) ? DustID.GemAmethyst : DustID.GoldFlame;

				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					dustType
				);

				dust.noGravity = true;
				dust.scale = 1.1f;
				dust.velocity *= 0.3f;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			var glow = ModContent.Request<Texture2D>("ConstellationsOfOrion/Content/Projectiles/StarangProjGlow").Value;
			Main.EntitySpriteDraw(
				glow,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation,
				glow.Size() / 2,
				Projectile.scale,
				SpriteEffects.None,
				0f
			);
		}
	}
}

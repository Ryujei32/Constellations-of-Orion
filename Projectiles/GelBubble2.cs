using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ConstellationsOfOrion.Common.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles
{
	public class GelBubble2 : ModProjectile
	{
		private const int FadeLifetime = 180;
		private const int ScaleAnimTicks = 30;
		private const float PeakScale = 1.28f;
		private const int TrailCacheLength = 12;
		private readonly List<Vector2> trailCache = new();

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 12;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;

			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;

			Projectile.penetrate = 3;
			Projectile.timeLeft = FadeLifetime;

			Projectile.tileCollide = false;
			Projectile.alpha = 100;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			UpdateTrailCache();

			if (Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				Projectile.localAI[1] = Projectile.velocity.X;
				Projectile.localAI[2] = Projectile.velocity.Y;
			}

			float progress = 1f - Projectile.timeLeft / (float)FadeLifetime;
			float speedFactor = MathHelper.Lerp(1f, 0f, progress);
			Vector2 targetVelocity = new Vector2(Projectile.localAI[1], Projectile.localAI[2]) * speedFactor;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.22f);

			if (Projectile.timeLeft > ScaleAnimTicks) {
				Projectile.scale = 1f;
				Projectile.alpha = 100;
			}
			else {
				float scaleProgress = 1f - Projectile.timeLeft / (float)ScaleAnimTicks;
				const float growEnd = 0.4f;
				if (scaleProgress <= growEnd)
					Projectile.scale = MathHelper.Lerp(1f, PeakScale, scaleProgress / growEnd);
				else
					Projectile.scale = MathHelper.Lerp(PeakScale, 0f, (scaleProgress - growEnd) / (1f - growEnd));

				Projectile.alpha = (int)MathHelper.Lerp(100f, 255f, scaleProgress);
			}

			if (Projectile.scale <= 0.08f) {
				Projectile.Kill();
				return;
			}

			if (Projectile.scale <= 0.2f) {
				Projectile.friendly = false;
				Projectile.tileCollide = false;
			}

			float radiusPixels = Math.Max(8f, Projectile.width * Projectile.scale * 1.15f);
			float scaleFactor = MathHelper.Clamp((Projectile.scale - 0.08f) / 1.2f, 0f, 1f);
			float alphaFactor = 1f - Projectile.alpha / 255f;
			float distortionStrength = scaleFactor * (0.3f + 0.55f * alphaFactor);
			GelBubbleOutlineDistortionSystem.RegisterBubble(Projectile.Center, radiusPixels, distortionStrength);

			Projectile.rotation += 0.06f + Projectile.velocity.Length() * 0.02f;

		}

		private void UpdateTrailCache()
		{
			Vector2 center = Projectile.Center;
			if (trailCache.Count == 0)
			{
				for (int i = 0; i < TrailCacheLength; i++)
					trailCache.Add(center);
			}

			trailCache.Add(center);
			while (trailCache.Count > TrailCacheLength)
				trailCache.RemoveAt(0);
		}

		private static Effect TryGetTrailEffect()
		{
			try
			{
				return Filters.Scene["StellarLashTrail"]?.GetShader()?.Shader;
			}
			catch
			{
				return null;
			}
		}

		private void DrawBubbleTrailSegments()
		{
			if (trailCache.Count < 2)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float lifeAlpha = 1f - Projectile.alpha / 255f;

			for (int i = 1; i < trailCache.Count; i++)
			{
				Vector2 start = trailCache[i - 1];
				Vector2 end = trailCache[i];
				Vector2 diff = end - start;
				float length = diff.Length();
				if (length <= 0.01f)
					continue;

				float t = i / (float)(trailCache.Count - 1);
				float width = MathHelper.Lerp(1.2f, 4.8f, t) * Projectile.scale;
				width = Math.Max(1f, (float)Math.Round(width));
				float alphaStep = MathHelper.Lerp(0.22f, 0.95f, t);
				alphaStep = (float)Math.Floor(alphaStep * 4f) / 4f; // 4-step pixel fade
				Color color = Color.Lerp(new Color(96, 58, 220, 255), new Color(196, 156, 255, 255), t);
				color *= alphaStep * lifeAlpha;
				color.A = (byte)(255f * alphaStep * lifeAlpha);
				Color softColor = color * 0.4f;

				Vector2 drawPos = start - Main.screenPosition;
				drawPos.X = (float)Math.Round(drawPos.X);
				drawPos.Y = (float)Math.Round(drawPos.Y);
				float rotation = diff.ToRotation();
				float segLenPx = (float)Math.Round(length + 0.6f);
				float softWidth = Math.Max(1f, (float)Math.Round(width * 1.45f));

				Main.EntitySpriteDraw(texture, drawPos, frame, softColor, rotation, new Vector2(0f, 0.5f), new Vector2(segLenPx, softWidth), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, color, rotation, new Vector2(0f, 0.5f), new Vector2(segLenPx, width), SpriteEffects.None, 0f);
			}

			DrawInterleavedHighlightStreaks(lifeAlpha);
		}

		private void DrawInterleavedHighlightStreaks(float lifeAlpha)
		{
			if (trailCache.Count < 3 || lifeAlpha <= 0.01f)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float[] laneOffsets = new float[] { -3.2f, 0f, 3.2f };

			for (int lane = 0; lane < laneOffsets.Length; lane++)
			{
				float laneOffset = laneOffsets[lane];
				float lanePhase = lane * 1.4f;
				for (int i = 1; i < trailCache.Count; i++)
				{
					Vector2 start = trailCache[i - 1];
					Vector2 end = trailCache[i];
					Vector2 seg = end - start;
					float segLen = seg.Length();
					if (segLen <= 0.01f)
						continue;

					float t = i / (float)(trailCache.Count - 1);
					Vector2 tangent = seg / segLen;
					Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);
					float weave = (float)Math.Sin(t * MathHelper.TwoPi * 1.8f + lanePhase + Main.GlobalTimeWrappedHourly * 1.2f) * 1.05f;
					Vector2 drawPos = start + normal * (laneOffset + weave) - Main.screenPosition;
					drawPos.X = (float)Math.Round(drawPos.X);
					drawPos.Y = (float)Math.Round(drawPos.Y);
					float rotation = seg.ToRotation();

					float pulse = 0.74f + 0.26f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6.8f + t * 8.2f + lanePhase);
					Color glow = Color.Lerp(new Color(198, 154, 255, 255), new Color(230, 210, 255, 255), t) * (0.30f * pulse * lifeAlpha);
					Color core = Color.White * (0.26f * pulse * lifeAlpha);

					Main.EntitySpriteDraw(texture, drawPos, frame, glow, rotation, new Vector2(0f, 0.5f), new Vector2(segLen + 0.5f, 1.55f), SpriteEffects.None, 0f);
					Main.EntitySpriteDraw(texture, drawPos, frame, core, rotation, new Vector2(0f, 0.5f), new Vector2(segLen + 0.5f, 0.72f), SpriteEffects.None, 0f);
				}
			}
		}

		private void DrawBubbleTrail()
		{
			if (trailCache.Count < 2)
				return;

			Effect trailEffect = TryGetTrailEffect();
			if (trailEffect == null)
			{
				DrawBubbleTrailSegments();
				return;
			}

			trailEffect.Parameters["scroll"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.85f + Projectile.whoAmI * 0.05f);
			trailEffect.Parameters["repeats"]?.SetValue(2.6f);
			trailEffect.Parameters["intensity"]?.SetValue(0.9f);
			trailEffect.Parameters["tintA"]?.SetValue(new Vector4(0.34f, 0.18f, 0.78f, 1f));
			trailEffect.Parameters["tintB"]?.SetValue(new Vector4(0.48f, 0.35f, 0.96f, 1f));
			trailEffect.Parameters["pixelStepsX"]?.SetValue(18f);
			trailEffect.Parameters["pixelStepsY"]?.SetValue(6f);
			trailEffect.Parameters["pixelMix"]?.SetValue(1f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.AlphaBlend,
				SamplerState.PointClamp,
				DepthStencilState.None,
				Main.Rasterizer,
				trailEffect,
				Main.GameViewMatrix.TransformationMatrix
			);
			DrawBubbleTrailSegments();
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawBubbleTrail();

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() * 0.5f;
			float wobble = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 7.6f + Projectile.whoAmI * 0.52f + 0.6f);
			float speed = Projectile.velocity.Length();
			float speedWobble = MathHelper.Clamp(speed * 0.015f, 0f, 0.12f);
			float stretchFactor = MathHelper.Clamp(speed * 0.08f, 0f, 0.65f);
			Vector2 motionDir = speed > 0.01f ? Projectile.velocity / speed : Vector2.UnitX;
			float motionRotation = speed > 0.01f ? motionDir.ToRotation() : Projectile.rotation;
			Vector2 visualScale = Projectile.scale * new Vector2(
				1f + 0.065f * wobble + speedWobble + stretchFactor,
				1f - 0.055f * wobble - speedWobble * 0.45f - stretchFactor * 0.35f
			);
			Color drawColor = Projectile.GetAlpha(lightColor);

			for (int i = 3; i >= 1; i--)
			{
				float trailT = i / 3f;
				Vector2 trailOffset = -motionDir * (2.4f * i * Projectile.scale);
				Color trailColor = drawColor * (0.14f + 0.12f * trailT);
				Vector2 trailScale = visualScale * new Vector2(0.92f + 0.06f * trailT, 0.88f + 0.05f * trailT);
				Main.EntitySpriteDraw(texture, drawPosition + trailOffset, null, trailColor, motionRotation, origin, trailScale, SpriteEffects.None, 0f);
			}

			Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, motionRotation, origin, visualScale, SpriteEffects.None, 0f);
			return false;
		}
	}
}

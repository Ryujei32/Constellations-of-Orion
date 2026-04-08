using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles
{
	public class StellarLashProj : ModProjectile
	{
		private ref float Timer => ref Projectile.ai[0];
		private static readonly byte[] NextSwingModeByPlayer = new byte[Main.maxPlayers];
		private byte swingMode;
		private const int TrailCacheLength = 20;
		private readonly List<Vector2> trailCache = new();

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.DefaultToWhip();

			// ⭐ MID-HARDMODE BALANCE
			Projectile.WhipSettings.Segments = 26;
			Projectile.WhipSettings.RangeMultiplier = 1.35f;
		}

		public override void AI()
		{
			ManageTrailCacheFromProjectile();
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
		{
			int owner = Projectile.owner;
			if (owner < 0 || owner >= Main.maxPlayers)
				return;

			swingMode = NextSwingModeByPlayer[owner];
			NextSwingModeByPlayer[owner] = (byte)((NextSwingModeByPlayer[owner] + 1) % 3);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player player = Main.player[Projectile.owner];

			player.MinionAttackTargetNPC = target.whoAmI;

			Projectile.damage = (int)(Projectile.damage * 0.65f);
		}

		private void ManageTrailCacheFromProjectile()
		{
			if (Main.dedServ)
				return;

			List<Vector2> controlPoints = new();
			PopulateSwingControlPoints(controlPoints);
			ManageTrailCache(controlPoints);
		}

		private void PopulateSwingControlPoints(List<Vector2> controlPoints)
		{
			Projectile.FillWhipControlPoints(Projectile, controlPoints);
			if (controlPoints.Count < 2)
				return;

			Vector2 origin = controlPoints[0];
			if (swingMode == 1)
			{
				Vector2 axis = Projectile.velocity;
				if (axis.LengthSquared() < 0.0001f)
				{
					Player player = Main.player[Projectile.owner];
					axis = new Vector2(player.direction, 0f);
				}
				axis.Normalize();

				// Swing #2: mirror on aim-axis, keep same side.
				for (int i = 1; i < controlPoints.Count; i++)
				{
					Vector2 v = controlPoints[i] - origin;
					float along = Vector2.Dot(v, axis);
					Vector2 projected = axis * along;
					Vector2 perpendicular = v - projected;
					controlPoints[i] = origin + projected - perpendicular;
				}
			}
			else if (swingMode == 2)
			{
				// Swing #3: jab/thrust toward aim direction.
				Projectile.GetWhipSettings(Projectile, out float flyTime, out _, out _);
				float progress = flyTime <= 0f ? 0f : MathHelper.Clamp(Timer / flyTime, 0f, 1f);
				float stab = (float)Math.Sin(progress * MathHelper.Pi); // out -> back
				Vector2 axis = Projectile.velocity;
				if (axis.LengthSquared() < 0.0001f)
				{
					Player player = Main.player[Projectile.owner];
					axis = new Vector2(player.direction, 0f);
				}
				axis.Normalize();

				for (int i = 1; i < controlPoints.Count; i++)
				{
					float tipWeight = i / (float)(controlPoints.Count - 1);
					Vector2 v = controlPoints[i] - origin;
					float along = Vector2.Dot(v, axis);
					Vector2 projected = axis * along;
					Vector2 perpendicular = v - projected;

					float thrustScale = 1f + stab * (0.16f + 0.48f * tipWeight);
					float straighten = MathHelper.SmoothStep(0f, 0.92f, stab);
					Vector2 jabVector = projected * thrustScale + perpendicular * (1f - straighten);
					controlPoints[i] = origin + jabVector;
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			List<Vector2> controlPoints = new();
			PopulateSwingControlPoints(controlPoints);
			if (controlPoints.Count < 2)
				return false;

			float collisionPoint = 0f;
			float lineWidth = 18f * Projectile.scale;
			for (int i = 0; i < controlPoints.Count - 1; i++)
			{
				if (Collision.CheckAABBvLineCollision(
					targetHitbox.TopLeft(),
					targetHitbox.Size(),
					controlPoints[i],
					controlPoints[i + 1],
					lineWidth,
					ref collisionPoint))
				{
					return true;
				}
			}

			return false;
		}

		private void ManageTrailCache(List<Vector2> controlPoints)
		{
			if (controlPoints.Count == 0)
				return;

			Vector2 tip = controlPoints[controlPoints.Count - 1];
			if (trailCache.Count == 0)
			{
				for (int i = 0; i < TrailCacheLength; i++)
					trailCache.Add(tip);
			}

			trailCache.Add(tip);
			while (trailCache.Count > TrailCacheLength)
				trailCache.RemoveAt(0);
		}

		private void DrawTrail()
		{
			if (trailCache.Count < 2)
				return;

			Effect trailEffect = TryGetTrailEffect();
			if (trailEffect == null)
			{
				DrawTrailSegments();
				return;
			}

			trailEffect.Parameters["scroll"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.2f);
			trailEffect.Parameters["repeats"]?.SetValue(3.0f);
			trailEffect.Parameters["intensity"]?.SetValue(0.85f);
			trailEffect.Parameters["tintA"]?.SetValue(new Vector4(0.34f, 0.18f, 0.78f, 1f));
			trailEffect.Parameters["tintB"]?.SetValue(new Vector4(0.48f, 0.35f, 0.96f, 1f));
			trailEffect.Parameters["pixelStepsX"]?.SetValue(18f);
			trailEffect.Parameters["pixelStepsY"]?.SetValue(14f);
			trailEffect.Parameters["pixelMix"]?.SetValue(0.12f); // B版: de-emphasize pixel quantization

			Main.spriteBatch.End();
			BlendState trailBlend = BlendState.AlphaBlend;
			Main.spriteBatch.Begin(
				SpriteSortMode.Immediate,
				trailBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				trailEffect,
				Main.GameViewMatrix.TransformationMatrix
			);
			DrawTrailSegments();
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

		private void DrawTrailSegments()
		{
			List<Vector2> smoothPoints = BuildSmoothTrailPoints();
			if (smoothPoints.Count < 2)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			Vector2 currentTip = trailCache.Count > 0 ? trailCache[trailCache.Count - 1] : smoothPoints[smoothPoints.Count - 1];
			bool newestAtEnd =
				Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], currentTip) <=
				Vector2.DistanceSquared(smoothPoints[0], currentTip);
			for (int i = 1; i < smoothPoints.Count; i++)
			{
				Vector2 start = smoothPoints[i - 1];
				Vector2 end = smoothPoints[i];
				Vector2 diff = end - start;
				float length = diff.Length();
				if (length <= 0.01f)
					continue;
				float t = i / (float)(smoothPoints.Count - 1);
				float freshness = newestAtEnd ? t : 1f - t; // 0=old, 1=new
				Vector2 tangent = diff / length;
				float width = MathHelper.Lerp(1.8f, 6.2f, t);
				width = Math.Max(1f, width);
				float alphaStep = MathHelper.Lerp(0.28f, 1f, freshness);
				float bendStart = 0f;
				float bendEnd = 0f;
				if (i > 1)
				{
					Vector2 prevSeg = smoothPoints[i - 1] - smoothPoints[i - 2];
					float prevLen = prevSeg.Length();
					if (prevLen > 0.001f)
					{
						prevSeg /= prevLen;
						bendStart = 1f - MathHelper.Clamp(Vector2.Dot(prevSeg, tangent), -1f, 1f);
					}
				}
				if (i < smoothPoints.Count - 1)
				{
					Vector2 nextSeg = smoothPoints[i + 1] - smoothPoints[i];
					float nextLen = nextSeg.Length();
					if (nextLen > 0.001f)
					{
						nextSeg /= nextLen;
						bendEnd = 1f - MathHelper.Clamp(Vector2.Dot(tangent, nextSeg), -1f, 1f);
					}
				}
				float bendHotspot = Math.Max(bendStart, bendEnd);
				float hotspotAtten = 1f - 0.35f * Utils.GetLerpValue(0.05f, 0.58f, bendHotspot, true);
				Color color = Color.Lerp(new Color(96, 58, 220, 255), new Color(196, 156, 255, 255), freshness);
				color *= MathHelper.Lerp(0.7f, 1.25f, freshness) * alphaStep * hotspotAtten;
				color.A = (byte)(255f * alphaStep * hotspotAtten);
				color *= 0.9f; // slight global brighten

				float rotation = diff.ToRotation();
				Color softColor = color * 0.32f;
				Color outerColor = color * 0.14f;
				Vector2 drawPos = (start + end) * 0.5f - Main.screenPosition;
				// Slight underlap to prevent endpoint overdraw hotspots.
				float segLenPx = Math.Max(0.2f, length - MathHelper.Clamp(length * 0.06f, 0.02f, 0.12f));
				float softWidth = width * 1.55f;
				float outerWidth = Math.Max(1f, width * 2.15f);
				Vector2 origin = new Vector2(0.5f, 0.5f);
				Main.EntitySpriteDraw(texture, drawPos, frame, outerColor, rotation, origin, new Vector2(segLenPx, outerWidth), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, softColor, rotation, origin, new Vector2(segLenPx, softWidth), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, color, rotation, origin, new Vector2(segLenPx, width), SpriteEffects.None, 0f);
			}

			if (swingMode == 2)
			{
				// Keep the base streak style, then layer jab-specific accents on top.
				DrawInterleavedHighlightStreaks(smoothPoints, newestAtEnd);
				DrawJabFanEffect(smoothPoints, newestAtEnd);
				DrawJabUnifiedTip(smoothPoints, newestAtEnd);
			}
			else
			{
				DrawInterleavedHighlightStreaks(smoothPoints, newestAtEnd);
				DrawTailDispersion(smoothPoints, newestAtEnd); // extra tiny peripheral lines
			}
		}

		private void DrawJabFanEffect(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 4)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float jabProgress = GetJabProgress();
			float stabForce = GetJabStabForce();
			float active = Utils.GetLerpValue(0.06f, 1f, jabProgress, true) * (0.34f + 0.46f * stabForce);

			// Fables-like structure: 1 thin main arc + 2 softer broken side arcs.
			float[] arcAmplitudes = new float[] { 0f, 3.2f, -3.2f };

			for (int arc = 0; arc < arcAmplitudes.Length; arc++)
			{
				float amp = arcAmplitudes[arc] * active;
				float phaseBase = Main.GlobalTimeWrappedHourly * 4.8f + arc * 1.27f + Projectile.whoAmI * 0.19f;

				for (int i = 1; i < smoothPoints.Count; i++)
				{
					Vector2 start = smoothPoints[i - 1];
					Vector2 end = smoothPoints[i];
					Vector2 seg = end - start;
					float segLen = seg.Length();
					if (segLen <= 0.01f)
						continue;

					float t = i / (float)(smoothPoints.Count - 1);
					float freshness = newestAtEnd ? t : 1f - t; // 1 near tip
					float tipWeight = Utils.GetLerpValue(0.25f, 1f, freshness, true);
					float bodyFade = (1f - 0.72f * (1f - freshness)); // keeps arcs near tip stronger
					Vector2 tangent = seg / segLen;
					Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);
					float wave = (float)Math.Sin(phaseBase + i * 0.21f + freshness * 3.4f) * (arc == 0 ? 0.25f : 0.9f);
					float offset = (amp * tipWeight) + wave * (0.7f + 0.4f * tipWeight);
					Vector2 drawPos = start + normal * offset - Main.screenPosition;
					float rotation = seg.ToRotation();

					Color backColor = new Color(96, 58, 220, 255);
					Color midColor = new Color(142, 110, 240, 255);
					Color frontColor = new Color(196, 156, 255, 255);
					Color rampColor = freshness < 0.5f
						? Color.Lerp(backColor, midColor, freshness / 0.5f)
						: Color.Lerp(midColor, frontColor, (freshness - 0.5f) / 0.5f);

					// Side arcs are intentionally broken/airier; center stays readable.
					bool isMainArc = arc == 0;
					bool drawSegment = isMainArc || ((i + arc) % 3 != 1);
					if (!drawSegment)
						continue;

					float laneAlpha = isMainArc
						? (0.16f + 0.14f * tipWeight) * bodyFade * (0.62f + 0.56f * stabForce)
						: (0.1f + 0.1f * tipWeight) * bodyFade * (0.58f + 0.46f * stabForce);
					float width = isMainArc
						? MathHelper.Lerp(0.46f, 0.2f, freshness)
						: MathHelper.Lerp(0.32f, 0.12f, freshness);
					float len = segLen + (isMainArc ? 0.08f : 0.02f);

					Main.EntitySpriteDraw(texture, drawPos, frame, rampColor * (laneAlpha * 0.62f), rotation, new Vector2(0f, 0.5f), new Vector2(len, width * 1.6f), SpriteEffects.None, 0f);
					Main.EntitySpriteDraw(texture, drawPos, frame, rampColor * (laneAlpha * 0.78f), rotation, new Vector2(0f, 0.5f), new Vector2(len, width), SpriteEffects.None, 0f);

					// Keep highlight, but avoid fixed "bamboo node" cadence:
					// use irregular, softer micro-streak accents instead of periodic bright dots.
					float sparkleGate = 0.5f + 0.5f * (float)Math.Sin(i * 1.71f + Projectile.whoAmI * 0.63f + Main.GlobalTimeWrappedHourly * 2.4f);
					if (isMainArc && sparkleGate > 0.83f)
					{
						Vector2 sparkPos = drawPos + tangent * (len * 0.28f);
						float streakLen = MathHelper.Lerp(1.6f, 2.8f, sparkleGate);
						float streakWidth = MathHelper.Lerp(0.45f, 0.7f, sparkleGate);
						Color spark = Color.Lerp(midColor, frontColor, 0.45f) * ((0.12f + 0.08f * stabForce) * bodyFade);
						Main.EntitySpriteDraw(texture, sparkPos, frame, spark, rotation, new Vector2(0f, 0.5f), new Vector2(streakLen, streakWidth), SpriteEffects.None, 0f);
					}
				}
			}
		}

		private void DrawJabUnifiedTip(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 3)
				return;

			float stabForce = GetJabStabForce();
			// Keep tip present through jab and retract, but brightest at peak.
			float tipActive = Math.Max(0.24f, 0.18f + 0.56f * stabForce);
			if (tipActive <= 0f)
				return;

			int tipIndex = newestAtEnd ? smoothPoints.Count - 1 : 0;
			int prevIndex = newestAtEnd ? smoothPoints.Count - 2 : 1;
			Vector2 tipWorld = smoothPoints[tipIndex];
			Vector2 prevWorld = smoothPoints[prevIndex];
			Vector2 forward = tipWorld - prevWorld;
			if (forward.LengthSquared() <= 0.0001f)
				return;
			forward.Normalize();

			Vector2 drawTip = tipWorld - Main.screenPosition;
			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);

			// / | \ rigid spearhead: no per-lane scatter on retract.
			float[] angles = new float[] { -0.44f, 0f, 0.44f };
			float[] lengths = new float[] { 7.8f, 11.4f, 7.8f };
			float[] widths = new float[] { 0.42f, 0.56f, 0.42f };

			for (int i = 0; i < angles.Length; i++)
			{
				Vector2 dir = forward.RotatedBy(angles[i]);
				float rot = dir.ToRotation();
				float len = lengths[i] * (0.78f + 0.62f * stabForce);
				float w = widths[i] * (0.86f + 0.28f * stabForce);
				Color glow = new Color(170, 110, 240, 255) * (0.2f + 0.2f * stabForce);
				Color core = new Color(208, 170, 255, 255) * (0.28f + 0.24f * stabForce);
				Main.EntitySpriteDraw(texture, drawTip, frame, glow, rot, new Vector2(0f, 0.5f), new Vector2(len, w * 2.2f), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawTip, frame, core, rot, new Vector2(0f, 0.5f), new Vector2(len, w), SpriteEffects.None, 0f);
			}
		}

		private void DrawJabBeamOverlay(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 2)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);

			for (int i = 1; i < smoothPoints.Count; i++)
			{
				Vector2 start = smoothPoints[i - 1];
				Vector2 end = smoothPoints[i];
				Vector2 seg = end - start;
				float segLen = seg.Length();
				if (segLen <= 0.01f)
					continue;

				float t = i / (float)(smoothPoints.Count - 1);
				float freshness = newestAtEnd ? t : 1f - t;
				Vector2 tangent = seg / segLen;
				float rotation = seg.ToRotation();
				Vector2 drawPos = start - Main.screenPosition;
				float len = segLen + 0.45f;

				// Beam profile: bright white core with magenta-violet bloom.
				float beamWidth = MathHelper.Lerp(7.2f, 3.0f, freshness);
				Color outer = new Color(181, 76, 255, 255) * 0.38f;
				Color mid = new Color(240, 128, 255, 255) * 0.52f;
				Color core = new Color(250, 236, 255, 255) * 0.86f;

				Main.EntitySpriteDraw(texture, drawPos, frame, outer, rotation, new Vector2(0f, 0.5f), new Vector2(len, beamWidth * 1.65f), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, mid, rotation, new Vector2(0f, 0.5f), new Vector2(len, beamWidth * 0.95f), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, core, rotation, new Vector2(0f, 0.5f), new Vector2(len, beamWidth * 0.34f), SpriteEffects.None, 0f);

				// Inner moving vein to mimic the reference's organic streak.
				float veinPhase = Main.GlobalTimeWrappedHourly * 12.2f + i * 0.93f + Projectile.whoAmI * 0.21f;
				float veinShift = (float)Math.Sin(veinPhase) * beamWidth * 0.12f;
				Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);
				Vector2 veinPos = drawPos + normal * veinShift + tangent * (segLen * 0.15f);
				float veinLen = Math.Max(2.5f, len * (0.32f + 0.22f * (0.5f + 0.5f * (float)Math.Sin(veinPhase * 0.71f))));
				Color vein = new Color(255, 246, 255, 255) * 0.74f;
				Main.EntitySpriteDraw(texture, veinPos, frame, vein, rotation, new Vector2(0f, 0.5f), new Vector2(veinLen, beamWidth * 0.13f), SpriteEffects.None, 0f);
			}
		}

		private void DrawInterleavedHighlightStreaks(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 3)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float[] laneOffsets = swingMode == 2
				? new float[] { -2.6f, 0f, 2.6f }
				: new float[] { -5.2f, 0f, 5.2f };
			Vector2 ownerCenter = Main.player[Projectile.owner].MountedCenter;
			float jabRetractWeight = 0f;
			if (swingMode == 2)
				jabRetractWeight = GetJabRetractWeight();

			float minDistance = float.MaxValue;
			float maxDistance = 0f;
			for (int i = 0; i < smoothPoints.Count; i++)
			{
				float d = Vector2.Distance(smoothPoints[i], ownerCenter);
				minDistance = Math.Min(minDistance, d);
				maxDistance = Math.Max(maxDistance, d);
			}
			float distanceSpan = Math.Max(1f, maxDistance - minDistance);
			for (int lane = 0; lane < laneOffsets.Length; lane++)
			{
				float laneOffset = laneOffsets[lane];
				float lanePhase = lane * 1.7f;
				float prevEffectiveOffset = laneOffset;
				for (int i = 1; i < smoothPoints.Count; i++)
				{
					Vector2 start = smoothPoints[i - 1];
					Vector2 end = smoothPoints[i];
					Vector2 seg = end - start;
					float segLen = seg.Length();
					if (segLen <= 0.01f)
						continue;

					float t = i / (float)(smoothPoints.Count - 1);
					float freshness = newestAtEnd ? t : 1f - t;
					Vector2 tangent = seg / segLen;
					Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);
					float distanceFromOwner = Vector2.Distance(start, ownerCenter);
					float outerness = MathHelper.Clamp((distanceFromOwner - minDistance) / distanceSpan, 0f, 1f);
					// Desired profile:
					// - Outer (straight whip): lines spread out heavily.
					// - Mid zone: strongest interleaving/weave.
					// - Inner (curled whip): lines nearly overlap.
					float effectiveOffset;
					if (swingMode == 2)
					{
						// Jab swing: keep lines in same area, but avoid rigid parallel spacing.
						float tipToBase = 1f - freshness;
						float converge = (float)Math.Pow(MathHelper.Clamp(tipToBase, 0f, 1f), 0.72f);
						float laneDrift = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6.2f + lanePhase + freshness * 7.5f) * 0.34f;
						float chaosPhase = Main.GlobalTimeWrappedHourly * 9.4f + i * 0.81f + lane * 1.73f;
						float chaos = ((float)Math.Sin(chaosPhase) * 1.1f + (float)Math.Cos(chaosPhase * 1.43f) * 0.7f) * jabRetractWeight;
						float retractScatter = chaos * (1f - freshness) * 0.92f;
						float clusterOffset = laneOffset * converge * 0.78f;
						float targetOffset = clusterOffset + laneDrift + retractScatter;
						effectiveOffset = MathHelper.Lerp(prevEffectiveOffset, targetOffset, 0.42f); // continuous offset, avoid "fracture"
						float tipSharpen = Utils.GetLerpValue(0.84f, 1f, freshness, true);
						effectiveOffset *= 1f - tipSharpen;
					}
					else
					{
						float laneBase = laneOffset * (float)Math.Pow(outerness, 1.8f) * 1.55f;
						float weaveIn = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.18f, 0.5f, outerness, true));
						float weaveOut = 1f - MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.68f, 0.92f, outerness, true));
						float weaveWeight = weaveIn * weaveOut;
						// Important: no spatial-frequency weave on t (segment index), otherwise it creates
						// periodic convergence knots that look like bamboo nodes.
						float weave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f + lanePhase + Projectile.whoAmI * 0.19f) * weaveWeight * 0.65f;
						effectiveOffset = laneBase + weave;
					}

					Vector2 p0 = start + normal * effectiveOffset;
					if (swingMode == 2)
					{
						// Organic in/out offset only near the tip, so the main body stays connected.
						float randomShift = (float)Math.Sin((i + 1) * 13.37f + lane * 7.91f + Main.GlobalTimeWrappedHourly * 5.4f);
						float tipWeight = Utils.GetLerpValue(0.62f, 1f, freshness, true);
						float retractShift = (float)Math.Cos(Main.GlobalTimeWrappedHourly * 8.6f + lane * 1.29f + i * 0.47f) * jabRetractWeight;
						float localWarp = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10.3f + i * 0.52f + lane * 0.9f) * jabRetractWeight;
						p0 += tangent * (randomShift * MathHelper.Lerp(0f, 0.16f, tipWeight) + retractShift * 0.52f);
						p0 += normal * (localWarp * (1f - freshness) * 0.85f);
					}
					Vector2 drawPos = p0 - Main.screenPosition;
					float rotation = seg.ToRotation();

					// Root cause of "bamboo nodes":
					// brightness oscillating along trail index (freshness/i) creates repeated bright bands.
					// Keep highlight alive, but pulse only over time (not along length).
					float pulse = 0.92f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6.2f + lanePhase + Projectile.whoAmI * 0.17f);
					// Root-cause fix for "bamboo nodes":
					// short spline segments were receiving similar highlight length as long ones,
					// causing periodic overdraw spikes at joints.
					float segEnergy = Utils.GetLerpValue(0.22f, 1.1f, segLen, true);
					float bendStart = 0f;
					float bendEnd = 0f;
					if (i > 1)
					{
						Vector2 prevSeg = smoothPoints[i - 1] - smoothPoints[i - 2];
						float prevLen = prevSeg.Length();
						if (prevLen > 0.001f)
						{
							prevSeg /= prevLen;
							bendStart = 1f - MathHelper.Clamp(Vector2.Dot(prevSeg, tangent), -1f, 1f);
						}
					}
					if (i < smoothPoints.Count - 1)
					{
						Vector2 nextSeg = smoothPoints[i + 1] - smoothPoints[i];
						float nextLen = nextSeg.Length();
						if (nextLen > 0.001f)
						{
							nextSeg /= nextLen;
							bendEnd = 1f - MathHelper.Clamp(Vector2.Dot(tangent, nextSeg), -1f, 1f);
						}
					}
					float bendHotspot = Math.Max(bendStart, bendEnd);
					float bendAtten = 1f - 0.45f * Utils.GetLerpValue(0.05f, 0.62f, bendHotspot, true);
					Color glow;
					Color core;
					if (swingMode == 2)
					{
						Color backColor = new Color(96, 58, 220, 255);
						Color midColor = new Color(142, 110, 240, 255);
						Color frontColor = new Color(196, 156, 255, 255);
						Color rampColor = freshness < 0.5f
							? Color.Lerp(backColor, midColor, freshness / 0.5f)
							: Color.Lerp(midColor, frontColor, (freshness - 0.5f) / 0.5f);

						// Avoid per-segment periodic bright spots on jab lanes.
						float lanePulse = 0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8.3f + lane * 1.63f + Projectile.whoAmI * 0.23f);
						glow = rampColor * (0.28f * pulse * lanePulse);
						core = Color.Lerp(rampColor, frontColor, 0.35f) * (0.16f * pulse * lanePulse);
					}
					else
					{
						Color baseColor = Color.Lerp(new Color(198, 154, 255, 255), new Color(230, 210, 255, 255), freshness);
						glow = baseColor * (0.3f * pulse);
						core = Color.Lerp(baseColor, new Color(236, 214, 255, 255), 0.25f) * (0.085f * pulse);
					}
					float streakAlpha = 1f;
					glow *= streakAlpha * segEnergy * bendAtten;
					core *= streakAlpha * segEnergy * MathHelper.Lerp(1f, bendAtten, 0.85f);
					float glowWidth = swingMode == 2 ? MathHelper.Lerp(1.52f, 0.72f, freshness) : 1.9f;
					float coreWidth = swingMode == 2 ? MathHelper.Lerp(0.52f, 0.14f, freshness) : 0.64f;
					float trimLen = MathHelper.Clamp(segLen * 0.12f, 0.03f, 0.16f);
					float drawLen = Math.Max(0.2f, segLen - trimLen);
					if (swingMode == 2)
					{
						float lenNoise = (float)Math.Sin((i + 1) * 9.73f + lane * 5.21f + Main.GlobalTimeWrappedHourly * 6.1f);
						drawLen = Math.Max(1.2f, drawLen + lenNoise * 0.06f);
						glowWidth *= 0.92f + 0.18f * (0.5f + 0.5f * lenNoise);
						coreWidth *= 0.9f + 0.2f * (0.5f + 0.5f * lenNoise);

						// Critical style rule: line tips must NOT end aligned.
						// Give each lane unique tip phase/length/position near the freshest part.
						float tipScatter = Utils.GetLerpValue(0.72f, 0.9f, freshness, true) * (1f - Utils.GetLerpValue(0.92f, 1f, freshness, true));
						float laneBaseShift = lane switch
						{
							0 => -2.6f,
							1 => 0.5f,
							_ => 2.2f
						};
						float laneWave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 7.8f + lane * 2.03f + i * 0.31f) * 0.95f;
						float finalShift = (laneBaseShift + laneWave) * tipScatter;
						drawPos += tangent * finalShift;

						float laneLenScale = lane switch
						{
							0 => 0.73f,
							1 => 0.92f,
							_ => 0.81f
						};
						drawLen *= MathHelper.Lerp(1f, laneLenScale, tipScatter);

						float tipAngleJitter = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5.4f + lane * 1.81f + i * 0.22f) * 0.085f * tipScatter;
						rotation += tipAngleJitter;
						float tipSharpen = Utils.GetLerpValue(0.84f, 1f, freshness, true);
						glowWidth = MathHelper.Lerp(glowWidth, 0.62f, tipSharpen);
						coreWidth = MathHelper.Lerp(coreWidth, 0.16f, tipSharpen);
					}

					Vector2 drawCenter = drawPos + tangent * (segLen * 0.5f);
					Vector2 streakOrigin = new Vector2(0.5f, 0.5f);

					Main.EntitySpriteDraw(
						texture,
						drawCenter,
						frame,
						glow,
						rotation,
						streakOrigin,
						new Vector2(drawLen, glowWidth),
						SpriteEffects.None,
						0f
					);

					Main.EntitySpriteDraw(
						texture,
						drawCenter,
						frame,
						core,
						rotation,
						streakOrigin,
						new Vector2(drawLen, coreWidth),
						SpriteEffects.None,
						0f
					);

					prevEffectiveOffset = effectiveOffset;
				}
			}

					if (swingMode == 2)
						DrawJabTipProngs(smoothPoints, newestAtEnd);
		}

		private void DrawJabSeamBridge(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 4)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			for (int i = 1; i < smoothPoints.Count - 1; i++)
			{
				Vector2 pPrev = smoothPoints[i - 1];
				Vector2 p = smoothPoints[i];
				Vector2 pNext = smoothPoints[i + 1];
				Vector2 a = p - pPrev;
				Vector2 b = pNext - p;
				float lenA = a.Length();
				float lenB = b.Length();
				if (lenA <= 0.001f || lenB <= 0.001f)
					continue;

				a /= lenA;
				b /= lenB;
				Vector2 dir = a + b;
				if (dir.LengthSquared() <= 0.0001f)
					dir = b;
				else
					dir.Normalize();

				float t = i / (float)(smoothPoints.Count - 1);
				float freshness = newestAtEnd ? t : 1f - t;
				float bend = 1f - MathHelper.Clamp(Vector2.Dot(a, b), -1f, 1f);
				if (bend < 0.1f)
					continue;
				float bendWeight = Utils.GetLerpValue(0.1f, 0.55f, bend, true);
				float bridgeLen = MathHelper.Lerp(3.8f, 2.1f, freshness);
				float bridgeWidth = MathHelper.Lerp(1.28f, 0.52f, freshness);
				Vector2 drawPos = p - Main.screenPosition - dir * (bridgeLen * 0.5f);
				float rot = dir.ToRotation();
				Color bridgeGlow = Color.Lerp(new Color(140, 98, 230, 255), new Color(196, 156, 255, 255), freshness) * (0.17f * bendWeight);
				Color bridgeCore = Color.Lerp(new Color(166, 128, 242, 255), new Color(224, 198, 255, 255), freshness) * (0.12f * bendWeight);

				Main.EntitySpriteDraw(texture, drawPos, frame, bridgeGlow, rot, new Vector2(0f, 0.5f), new Vector2(bridgeLen, bridgeWidth * 1.7f), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, bridgeCore, rot, new Vector2(0f, 0.5f), new Vector2(bridgeLen, bridgeWidth * 0.86f), SpriteEffects.None, 0f);
			}
		}

		private float GetJabRetractWeight()
		{
			Projectile.GetWhipSettings(Projectile, out float flyTime, out _, out _);
			if (flyTime <= 0f)
				return 0f;

			float progress = MathHelper.Clamp(Timer / flyTime, 0f, 1f);
			// 0 during jab-out, ramps up during return phase.
			return Utils.GetLerpValue(0.52f, 0.98f, progress, true);
		}

		private float GetJabProgress()
		{
			Projectile.GetWhipSettings(Projectile, out float flyTime, out _, out _);
			if (flyTime <= 0f)
				return 0f;
			return MathHelper.Clamp(Timer / flyTime, 0f, 1f);
		}

		private float GetJabStabForce()
		{
			float p = GetJabProgress();
			return (float)Math.Sin(p * MathHelper.Pi); // 0 -> 1 -> 0
		}

		private void DrawTailDispersion(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 4)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float[] branchOffsets = new float[] { -6.2f, -4.1f, -2.2f, 2.2f, 4.1f, 6.2f };

			for (int i = 1; i < smoothPoints.Count; i++)
			{
				float t = i / (float)(smoothPoints.Count - 1);
				float freshness = newestAtEnd ? t : 1f - t;
				float tailWeight = 1f - Utils.GetLerpValue(0.72f, 1f, freshness, true);
				if (tailWeight <= 0f)
					continue;

				Vector2 start = smoothPoints[i - 1];
				Vector2 end = smoothPoints[i];
				Vector2 seg = end - start;
				float segLen = seg.Length();
				if (segLen <= 0.01f)
					continue;

				Vector2 tangent = seg / segLen;
				Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);
				float rotation = seg.ToRotation();
				float branchAlpha = 0.19f * tailWeight;
				Color branchColor = Color.Lerp(new Color(126, 92, 220, 255), new Color(188, 148, 255, 255), t) * (branchAlpha * 0.88f);

				for (int b = 0; b < branchOffsets.Length; b++)
				{
					float phase = Main.GlobalTimeWrappedHourly * 3.6f + b * 1.9f + i * 0.35f;
					float jitter = (float)Math.Sin(phase) * 0.9f * tailWeight;
					Vector2 branchPos = start + normal * (branchOffsets[b] * tailWeight + jitter) - Main.screenPosition;
					float branchLen = segLen + 0.72f + tailWeight * 1.05f;
					float branchWidth = 0.42f + 0.44f * tailWeight;
					Main.EntitySpriteDraw(texture, branchPos, frame, branchColor, rotation, new Vector2(0f, 0.5f), new Vector2(branchLen, branchWidth), SpriteEffects.None, 0f);
				}
			}
		}

		private void DrawJabTipProngs(List<Vector2> smoothPoints, bool newestAtEnd)
		{
			if (smoothPoints.Count < 3)
				return;

			int tipIndex = newestAtEnd ? smoothPoints.Count - 1 : 0;
			int prevIndex = newestAtEnd ? smoothPoints.Count - 2 : 1;
			Vector2 tipWorld = smoothPoints[tipIndex];
			Vector2 prevWorld = smoothPoints[prevIndex];
			Vector2 forward = tipWorld - prevWorld;
			if (forward.LengthSquared() <= 0.0001f)
				return;

			forward.Normalize();
			Vector2 drawTip = tipWorld - Main.screenPosition;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float pulse = 0.86f + 0.14f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 11.5f + Projectile.whoAmI * 0.4f);

			float[] angles = new float[] { -0.36f, 0f, 0.36f }; // subtler / | \
			float[] lengths = new float[] { 10f, 14f, 10f };
			float[] widths = new float[] { 0.72f, 0.9f, 0.72f };

			for (int i = 0; i < angles.Length; i++)
			{
				float phase = Main.GlobalTimeWrappedHourly * 8.4f + Projectile.whoAmI * 0.31f + i * 2.1f;
				float dynamicAngle = angles[i] + (float)Math.Sin(phase) * 0.04f;
				float dynamicLen = lengths[i] * (0.95f + 0.1f * (0.5f + 0.5f * (float)Math.Sin(phase + 0.9f)));
				float forwardShift = (float)Math.Sin(phase + 1.35f) * 1.6f;
				Vector2 tipBase = drawTip + forward * forwardShift;
				Vector2 dir = forward.RotatedBy(dynamicAngle);
				float rotation = dir.ToRotation();
				Color glow = new Color(190, 152, 252, 255) * (0.2f * pulse);
				Color core = new Color(224, 196, 255, 255) * (0.24f * pulse);
				float wide = widths[i];
				const int taperSteps = 3;
				float cursor = 0f;
				for (int step = 0; step < taperSteps; step++)
				{
					float stepT0 = step / (float)taperSteps;
					float stepT1 = (step + 1) / (float)taperSteps;
					float segLen = dynamicLen * (stepT1 - stepT0);
					float segWidth = MathHelper.Lerp(wide, 0.12f, stepT1);
					Vector2 segPos = tipBase + dir * cursor;
					cursor += segLen;

					Main.EntitySpriteDraw(
						texture,
						segPos,
						frame,
						glow,
						rotation,
						new Vector2(0f, 0.5f),
						new Vector2(segLen + 0.25f, segWidth * 1.9f),
						SpriteEffects.None,
						0f
					);

					Main.EntitySpriteDraw(
						texture,
						segPos,
						frame,
						core,
						rotation,
						new Vector2(0f, 0.5f),
						new Vector2(segLen + 0.25f, segWidth),
						SpriteEffects.None,
						0f
					);
				}
			}
		}

		private List<Vector2> BuildSmoothTrailPoints()
		{
			List<Vector2> points = new();
			if (trailCache.Count < 2)
				return points;

			// Deduplicate near-identical source points before spline generation.
			List<Vector2> source = new();
			const float sourceMinSpacing = 0.05f;
			float sourceMinSpacingSq = sourceMinSpacing * sourceMinSpacing;
			for (int i = 0; i < trailCache.Count; i++)
			{
				Vector2 p = trailCache[i];
				if (source.Count == 0 || Vector2.DistanceSquared(source[source.Count - 1], p) >= sourceMinSpacingSq)
					source.Add(p);
			}
			if (source.Count < 2)
			{
				source.Add(trailCache[trailCache.Count - 1]);
			}

			if (source.Count < 4)
			{
				points.AddRange(source);
			}
			else
			{
				for (int i = 0; i < source.Count - 1; i++)
				{
					Vector2 p0 = source[Math.Max(i - 1, 0)];
					Vector2 p1 = source[i];
					Vector2 p2 = source[i + 1];
					Vector2 p3 = source[Math.Min(i + 2, source.Count - 1)];
					float segmentLength = Vector2.Distance(p1, p2);
					int stepsPerSegment = Math.Clamp((int)Math.Ceiling(segmentLength / 2.8f), 2, 8);

					Vector2 d01 = p1 - p0;
					Vector2 d12 = p2 - p1;
					Vector2 d23 = p3 - p2;
					float l01 = d01.Length();
					float l12 = d12.Length();
					float l23 = d23.Length();
					float dotA = 1f;
					float dotB = 1f;
					if (l01 > 0.0001f && l12 > 0.0001f)
						dotA = Vector2.Dot(d01 / l01, d12 / l12);
					if (l12 > 0.0001f && l23 > 0.0001f)
						dotB = Vector2.Dot(d12 / l12, d23 / l23);
					bool cuspRisk = dotA < -0.25f || dotB < -0.25f;

					for (int step = 0; step < stepsPerSegment; step++)
					{
						float t = step / (float)stepsPerSegment;
						Vector2 sample = cuspRisk ? Vector2.Lerp(p1, p2, t) : Vector2.CatmullRom(p0, p1, p2, p3, t);
						points.Add(sample);
					}
				}

				points.Add(source[source.Count - 1]);
			}
			// Remove ultra-short consecutive segments; they produce join hotspots ("bamboo nodes")
			// when rendered as many tiny quads with additive/overlay layering.
			List<Vector2> filtered = new();
			const float minSpacing = 0.06f;
			float minSpacingSq = minSpacing * minSpacing;
			filtered.Add(points[0]);
			for (int i = 1; i < points.Count; i++)
			{
				if (Vector2.DistanceSquared(points[i], filtered[filtered.Count - 1]) >= minSpacingSq)
					filtered.Add(points[i]);
			}
			if (Vector2.DistanceSquared(filtered[filtered.Count - 1], points[points.Count - 1]) > 0.0001f)
				filtered.Add(points[points.Count - 1]);

			return filtered;
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

		// ⭐ STRING DRAW
		private void DrawLine(List<Vector2> controlPoints)
		{
			Texture2D lineTexture = TextureAssets.FishingLine.Value;
			Rectangle frame = lineTexture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2f, 2f);

			Vector2 position = controlPoints[0];

			for (int i = 0; i < controlPoints.Count - 1; i++)
			{
				Vector2 start = controlPoints[i];
				Vector2 end = controlPoints[i + 1];
				Vector2 diff = end - start;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(start.ToTileCoordinates());

				Vector2 scale = new Vector2(1f, (diff.Length() + 2f) / frame.Height);

				Main.EntitySpriteDraw(
					lineTexture,
					position - Main.screenPosition,
					frame,
					color,
					rotation,
					origin,
					scale,
					SpriteEffects.None,
					0
				);

				position += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			List<Vector2> controlPoints = new();
			PopulateSwingControlPoints(controlPoints);
			ManageTrailCache(controlPoints);

			DrawLine(controlPoints);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			SpriteEffects spriteEffects = Projectile.spriteDirection < 0
				? SpriteEffects.None
				: SpriteEffects.FlipHorizontally;

			Vector2 position = controlPoints[0];

			for (int i = 0; i < controlPoints.Count - 1; i++)
			{
				Vector2 current = controlPoints[i];
				Vector2 next = controlPoints[i + 1];
				Vector2 diff = next - current;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(current.ToTileCoordinates());

				Rectangle frame = i switch {
    0 => new Rectangle(0, 0, 10, 26),
    int n when n == controlPoints.Count - 2 => new Rectangle(0, 74, 10, 18), // this
    >= 13 => new Rectangle(0, 58, 10, 16),
    >= 7 => new Rectangle(0, 42, 10, 16),
    _ => new Rectangle(0, 26, 10, 16),
};

				Vector2 origin = new Vector2(5, 8);
				float scale = 1f;

				if (i == controlPoints.Count - 2)
				{
					Projectile.GetWhipSettings(Projectile, out float flyTime, out _, out _);
					float t = Timer / flyTime;

					scale = MathHelper.Lerp(0.6f, 1.6f,
						Utils.GetLerpValue(0.1f, 0.7f, t, true) *
						Utils.GetLerpValue(0.9f, 0.7f, t, true));
				}

				Main.EntitySpriteDraw(
					texture,
					position - Main.screenPosition,
					frame,
					color,
					rotation,
					origin,
					scale,
					spriteEffects,
					0
				);

				position += diff;
			}

			// Draw trail after whip segments so it stays visible.
			DrawTrail();

			return false;
		}
	}
}

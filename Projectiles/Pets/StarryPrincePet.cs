using System;
using ConstellationsOfOrion.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles.Pets
{
	public class StarryPrincePet : ModProjectile
	{
		private const float TeleportTriggerDistance = 560f;
		private const int TeleportShrinkTicks = 12;
		private const int TeleportGrowTicks = 12;
		private const int MaxGroundBounceChunks = 48;
		private const int GroundBounceTilesPerImpact = 3; // single layer, 3 tiles on X
		private const float GroundBounceLayerYOffset = -7f;
		private readonly System.Collections.Generic.List<GroundBounceChunk> groundBounceChunks = new();
		private const int PetTrailCacheLength = 16;
		private readonly System.Collections.Generic.List<Vector2> petTrailCache = new();

		private ref float TeleportState => ref Projectile.localAI[0];
		private ref float TeleportTimer => ref Projectile.localAI[1];
		private ref float JumpCooldown => ref Projectile.localAI[2];
		private bool wasGrounded;
		private int landingImpactCooldown;

		private struct GroundBounceChunk
		{
			public Point TileCoord;
			public ushort TileType;
			public short FrameX;
			public short FrameY;
			public int CapHeight;
			public float Timer;
			public float Delay;
			public float Lifetime;
			public float Strength;
			public float LiftHeight;
		}

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 5;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.CharacterPreviewAnimations[Type] =
				ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Type], 6)
				.WhenNotSelected(0, 0)
				.WithOffset(-6f, -2f)
				.WithCode(CharacterPreviewBounce);
		}

		public static void CharacterPreviewBounce(Projectile proj, bool walking)
		{
			if (!walking)
				return;

			float timer = (float)Main.timeForVisualEffects;
			float bob = (float)Math.Sin(timer * MathHelper.TwoPi * (4f / 60f)) * 6.5f;
			proj.position.Y += bob;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;

			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;

			Projectile.penetrate = -1;
			Projectile.netImportant = true;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!player.active)
			{
				Projectile.Kill();
				return;
			}

			if (player.dead)
			{
				player.ClearBuff(ModContent.BuffType<Content.Buffs.StarryPrinceBuff>());
			}

			if (player.HasBuff(ModContent.BuffType<Content.Buffs.StarryPrinceBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			float distance = Vector2.Distance(Projectile.Center, player.Center);
			Vector2 target = player.Center + new Vector2(-40f * player.direction, 0f);
			float previousVerticalVelocity = Projectile.velocity.Y;

			if (TeleportState == 0f && distance > TeleportTriggerDistance)
			{
				Projectile.velocity = Vector2.Zero;
				TeleportState = 1f;
				TeleportTimer = 0f;
				Projectile.netUpdate = true;
			}

			if (HandleTeleportSequence(target, player.direction))
			{
				AnimateFrames();
				return;
			}

			Projectile.scale = 1f;

			Projectile.velocity.Y += 0.4f;
			if (Projectile.velocity.Y > 10f)
				Projectile.velocity.Y = 10f;

			float horizontalOffset = target.X - Projectile.Center.X;
			float absOffset = Math.Abs(horizontalOffset);
			float direction = Math.Sign(horizontalOffset);
			bool grounded = TryGetGroundTopY(out float groundTopY);

			if (grounded && Projectile.velocity.Y >= 0f)
			{
				float targetY = groundTopY - Projectile.height;
				if (Projectile.position.Y > targetY - 4f && Projectile.position.Y < targetY + 10f)
				{
					Projectile.position.Y = targetY;
					Projectile.velocity.Y = 0f;
				}
			}

			if (landingImpactCooldown > 0)
				landingImpactCooldown--;

			if (grounded && !wasGrounded && previousVerticalVelocity > 0.8f)
			{
				float strength = MathHelper.Clamp(previousVerticalVelocity / 8f, 0.35f, 1f);
				TriggerLandingImpact(strength);
			}

			if (grounded)
			{
				if (absOffset > 16f)
					Projectile.velocity.X = direction * (absOffset > 220f ? 5.4f : 4.1f);
				else
					Projectile.velocity.X *= 0.8f;
			}
			else
			{
				Projectile.velocity.X *= 0.985f;
			}

			if (JumpCooldown > 0f)
				JumpCooldown--;

			if (grounded && JumpCooldown <= 0f)
			{
				if (distance > 280f)
				{
					Projectile.velocity.Y = -8.8f;
					Projectile.velocity.X = direction * 5.2f;
					JumpCooldown = 10f;
				}
				else if (absOffset > 28f || Main.rand.NextBool(35))
				{
					Projectile.velocity.Y = -6.4f;
					Projectile.velocity.X = direction * 3.6f;
					JumpCooldown = 14f;
				}
			}

			if (Projectile.velocity.X > 0.2f)
				Projectile.spriteDirection = 1;
			else if (Projectile.velocity.X < -0.2f)
				Projectile.spriteDirection = -1;

			UpdateGroundBounceChunks();
			UpdatePetTrailCache();
			wasGrounded = grounded;
			AnimateFrames();
		}

		private void UpdatePetTrailCache()
		{
			Vector2 center = Projectile.Center;
			if (petTrailCache.Count == 0)
			{
				for (int i = 0; i < PetTrailCacheLength; i++)
					petTrailCache.Add(center);
			}

			petTrailCache.Add(center);
			while (petTrailCache.Count > PetTrailCacheLength)
				petTrailCache.RemoveAt(0);
		}

		private bool HandleTeleportSequence(Vector2 target, int playerDirection)
		{
			if (TeleportState == 1f)
			{
				wasGrounded = false;
				groundBounceChunks.Clear();
				petTrailCache.Clear();
				TeleportTimer++;
				float shrinkProgress = Utils.GetLerpValue(0f, TeleportShrinkTicks, TeleportTimer, true);
				Projectile.scale = MathHelper.Lerp(1f, 0f, shrinkProgress);
				Projectile.velocity *= 0.85f;

				if (TeleportTimer >= TeleportShrinkTicks)
				{
					int spawnDirection = playerDirection == 0 ? Projectile.spriteDirection : playerDirection;
					Projectile.spriteDirection = spawnDirection;
					Projectile.Center = target + new Vector2(-24f * spawnDirection, -16f);
					Projectile.velocity = Vector2.Zero;
					TeleportState = 2f;
					TeleportTimer = 0f;
					Projectile.scale = 0f;
					Projectile.netUpdate = true;
				}

				return true;
			}

			if (TeleportState == 2f)
			{
				wasGrounded = false;
				groundBounceChunks.Clear();
				petTrailCache.Clear();
				TeleportTimer++;
				float growProgress = Utils.GetLerpValue(0f, TeleportGrowTicks, TeleportTimer, true);
				Projectile.scale = MathHelper.Lerp(0f, 1f, growProgress);

				if (TeleportTimer >= TeleportGrowTicks)
				{
					TeleportState = 0f;
					TeleportTimer = 0f;
					Projectile.scale = 1f;
				}

				return true;
			}

			return false;
		}

		private bool TryGetGroundTopY(out float groundTopY)
		{
			groundTopY = 0f;

			if (Projectile.velocity.Y < 0f)
				return false;

			int minX = (int)((Projectile.position.X + 4f) / 16f);
			int maxX = (int)((Projectile.position.X + Projectile.width - 4f) / 16f);
			int tileY = (int)((Projectile.position.Y + Projectile.height + 2f) / 16f);
			bool foundGround = false;
			float bestTop = float.MaxValue;

			for (int x = minX; x <= maxX; x++)
			{
				Tile tile = Framing.GetTileSafely(x, tileY);
				if (!tile.HasTile || !tile.HasUnactuatedTile)
					continue;

				bool solidBlock = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
				bool platformTop = Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0;
				if (!solidBlock && !platformTop)
					continue;

				float topY = tileY * 16f;
				if (topY < bestTop)
				{
					bestTop = topY;
					foundGround = true;
				}
			}

			if (foundGround)
			{
				groundTopY = bestTop;
				return true;
			}

			if (Collision.SolidCollision(Projectile.position + Vector2.UnitY, Projectile.width, Projectile.height))
			{
				groundTopY = Projectile.position.Y + Projectile.height;
				return true;
			}

			return false;
		}

		private void AnimateFrames()
		{
			Projectile.frameCounter++;

			if (Projectile.frameCounter >= 6)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;

				if (Projectile.frame >= 5)
					Projectile.frame = 0;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y > 1.2f)
			{
				float strength = MathHelper.Clamp(oldVelocity.Y / 8f, 0.45f, 1f);
				TriggerLandingImpact(strength);
			}

			if (Math.Abs(oldVelocity.X) > 0.1f && Math.Abs(Projectile.velocity.X) < 0.1f)
				Projectile.velocity.X = oldVelocity.X * 0.5f;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawGroundBounceChunks();
			DrawPetTrail(lightColor);
			return true;
		}

		private void DrawPetTrail(Color lightColor)
		{
			if (Main.dedServ || petTrailCache.Count < 2)
				return;

			Effect trailEffect = TryGetSharedTrailEffect();
			if (trailEffect == null)
			{
				DrawPetTrailSegments(lightColor);
				return;
			}

			trailEffect.Parameters["scroll"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.7f);
			trailEffect.Parameters["repeats"]?.SetValue(2.4f);
			trailEffect.Parameters["intensity"]?.SetValue(0.85f);
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
			DrawPetTrailSegments(lightColor);
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

		private void DrawPetTrailSegments(Color lightColor)
		{
			if (petTrailCache.Count < 2)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);

			for (int i = 1; i < petTrailCache.Count; i++)
			{
				Vector2 start = petTrailCache[i - 1];
				Vector2 end = petTrailCache[i];
				Vector2 diff = end - start;
				float length = diff.Length();
				if (length <= 0.01f)
					continue;

				float t = i / (float)(petTrailCache.Count - 1);
				float width = MathHelper.Lerp(1.4f, 5f, t);
				width = Math.Max(1f, (float)Math.Round(width));
				float alphaStep = MathHelper.Lerp(0.22f, 0.95f, t);
				alphaStep = (float)Math.Floor(alphaStep * 4f) / 4f;
				Color color = Color.Lerp(new Color(96, 58, 220, 255), new Color(196, 156, 255, 255), t);
				color = Color.Lerp(lightColor, color, 0.8f);
				color *= alphaStep * (1f - Projectile.alpha / 255f);
				color.A = (byte)(255f * alphaStep);
				Color softColor = color * 0.35f;

				Vector2 drawPos = start - Main.screenPosition;
				drawPos.X = (float)Math.Round(drawPos.X);
				drawPos.Y = (float)Math.Round(drawPos.Y);
				float segLenPx = (float)Math.Round(length + 0.8f);
				float softWidth = Math.Max(1f, (float)Math.Round(width * 1.45f));
				float rotation = diff.ToRotation();

				Main.EntitySpriteDraw(texture, drawPos, frame, softColor, rotation, new Vector2(0f, 0.5f), new Vector2(segLenPx, softWidth), SpriteEffects.None, 0f);
				Main.EntitySpriteDraw(texture, drawPos, frame, color, rotation, new Vector2(0f, 0.5f), new Vector2(segLenPx, width), SpriteEffects.None, 0f);
			}

			DrawPetHighlightStreaks(lightColor);
		}

		private void DrawPetHighlightStreaks(Color lightColor)
		{
			if (petTrailCache.Count < 3)
				return;

			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle frame = new Rectangle(0, 0, 1, 1);
			float[] laneOffsets = new float[] { -4f, 0f, 4f };

			for (int lane = 0; lane < laneOffsets.Length; lane++)
			{
				float laneOffset = laneOffsets[lane];
				float lanePhase = lane * 1.55f;
				for (int i = 1; i < petTrailCache.Count; i++)
				{
					Vector2 start = petTrailCache[i - 1];
					Vector2 end = petTrailCache[i];
					Vector2 seg = end - start;
					float segLen = seg.Length();
					if (segLen <= 0.01f)
						continue;

					float t = i / (float)(petTrailCache.Count - 1);
					Vector2 tangent = seg / segLen;
					Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);
					float weave = (float)Math.Sin(t * MathHelper.TwoPi * 2.0f + lanePhase + Main.GlobalTimeWrappedHourly * 1.3f) * 1.2f;
					Vector2 drawPos = start + normal * (laneOffset + weave) - Main.screenPosition;
					drawPos.X = (float)Math.Round(drawPos.X);
					drawPos.Y = (float)Math.Round(drawPos.Y);
					float rotation = seg.ToRotation();

					float pulse = 0.72f + 0.28f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 7.0f + t * 8.7f + lanePhase);
					Color glow = Color.Lerp(new Color(198, 154, 255, 255), new Color(230, 210, 255, 255), t) * (0.34f * pulse);
					Color core = Color.Lerp(lightColor, Color.White, 0.82f) * (0.28f * pulse);
					float lifeAlpha = 1f - Projectile.alpha / 255f;
					glow *= lifeAlpha;
					core *= lifeAlpha;

					Main.EntitySpriteDraw(texture, drawPos, frame, glow, rotation, new Vector2(0f, 0.5f), new Vector2(segLen + 0.6f, 1.75f), SpriteEffects.None, 0f);
					Main.EntitySpriteDraw(texture, drawPos, frame, core, rotation, new Vector2(0f, 0.5f), new Vector2(segLen + 0.6f, 0.78f), SpriteEffects.None, 0f);
				}
			}
		}

		private static Effect TryGetSharedTrailEffect()
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

		private void TriggerLandingImpact(float strength)
		{
			if (landingImpactCooldown > 0)
				return;

			landingImpactCooldown = 7;
			SlimePetImpactShockwaveSystem.Trigger(Projectile.Bottom, strength);
			SpawnGroundTileBounceWave(strength);
		}

		private void SpawnGroundTileBounceWave(float strength)
		{
			if (Main.dedServ)
				return;

			if (groundBounceChunks.Count >= MaxGroundBounceChunks)
				return;

			int centerX = (int)(Projectile.Center.X / 16f);
			int startY = (int)((Projectile.Bottom.Y + 2f) / 16f);
			var occupiedTiles = new System.Collections.Generic.HashSet<Point>();
			for (int i = 0; i < groundBounceChunks.Count; i++)
				occupiedTiles.Add(groundBounceChunks[i].TileCoord);
			int spawnedCount = 0;
			int[] xOffsets = new[] { -1, 0, 1 }; // Y = 1, X = 3

			bool IsSolidOrPlatformTop(Tile tile)
			{
				if (!tile.HasTile || !tile.HasUnactuatedTile)
					return false;

				bool solidBlock = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
				bool platformTop = Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0;
				return solidBlock || platformTop;
			}

			bool TryAddChunk(int x, int y, float distance)
			{
				if (groundBounceChunks.Count >= MaxGroundBounceChunks || spawnedCount >= GroundBounceTilesPerImpact)
					return false;

				Tile tile = Framing.GetTileSafely(x, y);
				if (!IsSolidOrPlatformTop(tile))
					return false;

				Point tilePoint = new Point(x, y);
				if (occupiedTiles.Contains(tilePoint))
					return false;

				float distanceFactor = distance / 2f;
				float waveFactor = 1f - MathHelper.Clamp(distanceFactor, 0f, 1f);
				float influence = Math.Max(0.25f, waveFactor);

				groundBounceChunks.Add(new GroundBounceChunk
				{
					TileCoord = tilePoint,
					TileType = tile.TileType,
					FrameX = tile.TileFrameX,
					FrameY = tile.TileFrameY,
					CapHeight = 16,
					Timer = 0f,
					Delay = distance * 0.45f,
					Lifetime = 16f + 8f * strength,
					Strength = 0.62f + influence * 0.9f * strength,
					LiftHeight = 2.6f + 9.2f * influence * strength
				});

				occupiedTiles.Add(tilePoint);
				spawnedCount++;
				return true;
			}

			for (int i = 0; i < xOffsets.Length && spawnedCount < GroundBounceTilesPerImpact; i++)
			{
				int x = centerX + xOffsets[i];
				float distance = Math.Abs(xOffsets[i]);
				int surfaceY = int.MinValue;

				for (int y = startY - 1; y <= startY + 3; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (!IsSolidOrPlatformTop(tile))
						continue;

					Tile tileAbove = Framing.GetTileSafely(x, y - 1);
					bool blockedAbove = tileAbove.HasTile && tileAbove.HasUnactuatedTile &&
						(Main.tileSolid[tileAbove.TileType] || Main.tileSolidTop[tileAbove.TileType]);
					if (blockedAbove)
						continue;

					surfaceY = y;
					break;
				}

				if (surfaceY == int.MinValue)
					continue;

				// Y = 1 (surface only), X = 3
				TryAddChunk(x, surfaceY, distance);
			}
		}

		private void UpdateGroundBounceChunks()
		{
			for (int i = groundBounceChunks.Count - 1; i >= 0; i--)
			{
				GroundBounceChunk chunk = groundBounceChunks[i];
				chunk.Timer++;
				if (chunk.Timer >= chunk.Delay + chunk.Lifetime)
				{
					groundBounceChunks.RemoveAt(i);
					continue;
				}

				groundBounceChunks[i] = chunk;
			}
		}

		private void DrawGroundBounceChunks()
		{
			if (groundBounceChunks.Count == 0)
				return;

			for (int i = 0; i < groundBounceChunks.Count; i++)
			{
				GroundBounceChunk chunk = groundBounceChunks[i];
				float activeTime = chunk.Timer - chunk.Delay;
				if (activeTime <= 0f)
					continue;

				float progress = MathHelper.Clamp(activeTime / chunk.Lifetime, 0f, 1f);
				float sineBump = (float)Math.Sin(progress * MathHelper.Pi);
				float bounceDisplace = chunk.LiftHeight * sineBump;

				Vector2 tilePosition = chunk.TileCoord.ToWorldCoordinates();
				Texture2D texture = TextureAssets.Tile[chunk.TileType].Value;
				Color tileColor = Lighting.GetColor(chunk.TileCoord.X, chunk.TileCoord.Y);

				Vector2 drawPosition = tilePosition + new Vector2(8f, 16f) - Main.screenPosition;
				drawPosition.Y += GroundBounceLayerYOffset;
				drawPosition.Y -= bounceDisplace;

				float squash = (float)Math.Sin(progress * MathHelper.Pi * 1.15f);
				float scaleX = 1f + squash * 0.16f * chunk.Strength;
				float scaleY = MathHelper.Clamp(1f - squash * 0.2f * chunk.Strength, 0.78f, 1f);
				Vector2 drawScale = new Vector2(scaleX, scaleY);

				Rectangle frame = new Rectangle(chunk.FrameX, chunk.FrameY, 16, 16);
				tileColor *= 0.72f + 0.28f * sineBump;
				Main.EntitySpriteDraw(texture, drawPosition, frame, tileColor, 0f, new Vector2(8f, 16f), drawScale, SpriteEffects.None, 0f);
			}
		}

	}
}

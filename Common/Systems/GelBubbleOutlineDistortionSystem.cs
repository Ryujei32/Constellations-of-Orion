using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Common.Systems
{
	public class GelBubbleOutlineDistortionSystem : ModSystem
	{
		private const string FilterKey = "GelBubbleOutlineDistortion";
		private const int MaxDistortionOrbs = 3;
		private const int MaxFrameCandidates = 24;

		private static int frameCandidateCount;
		private static readonly float[] frameScores = new float[MaxFrameCandidates];
		private static readonly Vector2[] frameWorldPositions = new Vector2[MaxFrameCandidates];
		private static readonly float[] frameRadii = new float[MaxFrameCandidates];
		private static readonly float[] frameIntensities = new float[MaxFrameCandidates];

		private static readonly Vector2[] slotWorldPositions = new Vector2[MaxDistortionOrbs];
		private static readonly float[] slotRadii = new float[MaxDistortionOrbs];
		private static readonly float[] slotIntensities = new float[MaxDistortionOrbs];
		private static readonly float[] slotOpacities = new float[MaxDistortionOrbs];

		public override void Unload()
		{
			ClearFrameCandidates();
			for (int i = 0; i < MaxDistortionOrbs; i++)
			{
				slotWorldPositions[i] = Vector2.Zero;
				slotRadii[i] = 0f;
				slotIntensities[i] = 0f;
				slotOpacities[i] = 0f;
			}
		}

		public static void RegisterBubble(Vector2 worldPosition, float radiusPixels, float intensity)
		{
			if (Main.dedServ)
				return;

			if (frameCandidateCount >= MaxFrameCandidates)
				return;

			float clampedRadius = MathHelper.Clamp(radiusPixels, 8f, 72f);
			float clampedIntensity = MathHelper.Clamp(intensity, 0f, 1f);
			float score = clampedRadius * clampedIntensity;

			frameScores[frameCandidateCount] = score;
			frameWorldPositions[frameCandidateCount] = worldPosition;
			frameRadii[frameCandidateCount] = clampedRadius;
			frameIntensities[frameCandidateCount] = clampedIntensity;
			frameCandidateCount++;
		}

		public override void PostUpdateEverything()
		{
			if (Main.dedServ)
				return;

			if (!TryGetFilter(out Filter filter))
			{
				ClearFrameCandidates();
				return;
			}

			UpdateSlotsFromFrameCandidates();

			Vector2 screenSize = Main.ScreenSize.ToVector2();
			if (screenSize.X <= 1f || screenSize.Y <= 1f)
			{
				ClearFrameCandidates();
				return;
			}

			float maxOpacity = 0f;
			for (int i = 0; i < MaxDistortionOrbs; i++)
				maxOpacity = Math.Max(maxOpacity, slotOpacities[i]);

			if (maxOpacity <= 0.003f)
			{
				if (filter.IsActive())
					filter.Deactivate();
				ClearFrameCandidates();
				return;
			}

			if (!filter.IsActive())
				Filters.Scene.Activate(FilterKey);

			float minScreenAxis = Math.Min(screenSize.X, screenSize.Y);
			Effect effect = filter.GetShader().Shader;
			for (int i = 0; i < MaxDistortionOrbs; i++)
			{
				float fade = MathHelper.Clamp(slotOpacities[i], 0f, 1f);
				float smoothFade = fade * fade * (3f - 2f * fade);
				Vector2 normalizedSource = (slotWorldPositions[i] - Main.screenPosition) / screenSize;
				float normalizedRadius = (slotRadii[i] / minScreenAxis) * smoothFade;
				float radiusFactor = MathHelper.Clamp(slotRadii[i] / 24f, 0.35f, 1f);
				float intensityValue = MathHelper.Lerp(0.003f, 0.011f, slotIntensities[i] * radiusFactor) * smoothFade;
				float thicknessValue = MathHelper.Lerp(0.008f, 0.018f, slotIntensities[i] * radiusFactor) * MathHelper.Lerp(0.18f, 1f, smoothFade);

				if (effect.Parameters["sourcePosition" + i] != null)
					effect.Parameters["sourcePosition" + i].SetValue(normalizedSource);
				if (effect.Parameters["radius" + i] != null)
					effect.Parameters["radius" + i].SetValue(normalizedRadius);
				if (effect.Parameters["thickness" + i] != null)
					effect.Parameters["thickness" + i].SetValue(thicknessValue);
				if (effect.Parameters["intensity" + i] != null)
					effect.Parameters["intensity" + i].SetValue(intensityValue);
			}

			ClearFrameCandidates();
		}

		private static void UpdateSlotsFromFrameCandidates()
		{
			float[] topScores = new float[MaxDistortionOrbs];
			Vector2[] topPositions = new Vector2[MaxDistortionOrbs];
			float[] topRadii = new float[MaxDistortionOrbs];
			float[] topIntensities = new float[MaxDistortionOrbs];
			int topCount = 0;

			for (int i = 0; i < frameCandidateCount; i++)
			{
				float score = frameScores[i];
				int insertIndex = topCount;
				for (int t = 0; t < topCount; t++)
				{
					if (score > topScores[t])
					{
						insertIndex = t;
						break;
					}
				}

				if (insertIndex >= MaxDistortionOrbs)
					continue;

				int newCount = Math.Min(topCount + 1, MaxDistortionOrbs);
				for (int t = newCount - 1; t > insertIndex; t--)
				{
					topScores[t] = topScores[t - 1];
					topPositions[t] = topPositions[t - 1];
					topRadii[t] = topRadii[t - 1];
					topIntensities[t] = topIntensities[t - 1];
				}

				topScores[insertIndex] = score;
				topPositions[insertIndex] = frameWorldPositions[i];
				topRadii[insertIndex] = frameRadii[i];
				topIntensities[insertIndex] = frameIntensities[i];
				topCount = newCount;
			}

			for (int i = 0; i < MaxDistortionOrbs; i++)
			{
				bool hasTarget = i < topCount;
				if (hasTarget)
				{
					if (slotOpacities[i] <= 0.02f)
					{
						slotWorldPositions[i] = topPositions[i];
						slotRadii[i] = topRadii[i];
						slotIntensities[i] = topIntensities[i];
					}
					else
					{
						slotWorldPositions[i] = Vector2.Lerp(slotWorldPositions[i], topPositions[i], 0.42f);
						slotRadii[i] = MathHelper.Lerp(slotRadii[i], topRadii[i], 0.38f);
						slotIntensities[i] = MathHelper.Lerp(slotIntensities[i], topIntensities[i], 0.35f);
					}

					slotOpacities[i] = MathHelper.Lerp(slotOpacities[i], 1f, 0.28f);
				}
				else
				{
					slotOpacities[i] = MathHelper.Lerp(slotOpacities[i], 0f, 0.14f);
				}
			}
		}

		private static void ClearFrameCandidates()
		{
			frameCandidateCount = 0;
			for (int i = 0; i < MaxFrameCandidates; i++)
			{
				frameScores[i] = 0f;
				frameWorldPositions[i] = Vector2.Zero;
				frameRadii[i] = 0f;
				frameIntensities[i] = 0f;
			}
		}

		private static bool TryGetFilter(out Filter filter)
		{
			filter = null;
			try
			{
				filter = Filters.Scene[FilterKey];
				return filter != null;
			}
			catch
			{
				return false;
			}
		}
	}
}

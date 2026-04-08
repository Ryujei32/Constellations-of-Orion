using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Common.Systems
{
	public class SlimePetImpactShockwaveSystem : ModSystem
	{
		private const bool EnableScreenFilter = false;
		private const string FilterKey = "SlimePetImpactShockwave";
		private const float EffectDuration = 28f;
		private static float timer;
		private static float impactStrength;
		private static Vector2 impactWorldPosition;

		public override void Unload()
		{
			timer = 0f;
			impactStrength = 0f;
			impactWorldPosition = Vector2.Zero;
		}

		public static void Trigger(Vector2 worldPosition, float strength)
		{
			if (Main.dedServ || !EnableScreenFilter)
				return;

			impactWorldPosition = worldPosition;
			impactStrength = MathHelper.Clamp(strength, 0f, 1f);
			timer = EffectDuration;
		}

		public override void PostUpdateEverything()
		{
			if (Main.dedServ || !EnableScreenFilter)
				return;

			Filter filter = Filters.Scene[FilterKey];
			if (timer <= 0f)
			{
				if (filter.IsActive())
					filter.Deactivate();
				return;
			}

			timer--;

			float completion = 1f - timer / EffectDuration;
			float normalizedStrength = (1f - completion) * impactStrength;
			float radius = MathHelper.Lerp(0.02f, 0.42f, completion);
			Vector2 normalizedSource = (impactWorldPosition - Main.screenPosition) / Main.ScreenSize.ToVector2();

			if (!filter.IsActive())
				Filters.Scene.Activate(FilterKey);

			Effect effect = filter.GetShader().Shader;
			if (effect.Parameters["sourcePosition"] != null)
				effect.Parameters["sourcePosition"].SetValue(normalizedSource);
			if (effect.Parameters["progress"] != null)
				effect.Parameters["progress"].SetValue(radius);
			if (effect.Parameters["intensity"] != null)
				effect.Parameters["intensity"].SetValue(normalizedStrength * 0.65f);
		}
	}
}

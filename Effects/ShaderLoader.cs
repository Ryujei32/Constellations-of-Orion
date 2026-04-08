using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace ConstellationsOfOrion.Effects
{
	public class ShaderLoader : ModSystem
	{
		public override void Load()
		{
			if (Main.dedServ)
				return;

			MethodInfo fileGetter = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			TmodFile file = (TmodFile)fileGetter.Invoke(Mod, null);
			var shaders = file.Where(entry => entry.Name.StartsWith("Effects/") && entry.Name.EndsWith(".xnb"));

			foreach (FileEntry entry in shaders)
			{
				string name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
				string path = entry.Name.Replace(".xnb", "");

				if (name.Equals("SlimePetImpactShockwave", System.StringComparison.OrdinalIgnoreCase))
					continue;

				LoadShader(name, path);
			}
		}

		private void LoadShader(string name, string path)
		{
			Ref<Effect> shaderRef = new Ref<Effect>(Mod.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
			Filters.Scene[name] = new Filter(new ScreenShaderData(shaderRef, name + "Pass"), EffectPriority.High);
			Filters.Scene[name].Load();
		}
	}
}

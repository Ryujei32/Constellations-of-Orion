using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;
using ReLogic.Content;

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

				LoadScreenShader(name, path);
			}

			// manual misc shader loading
			LoadMiscShader("Flame");

			LoadMiscShader("BaseVertex", out MiscShaderData baseVertexShader);
			baseVertexShader.UseOpacity(1f); // load defaults
			baseVertexShader.UseSaturation(1f);

			LoadMiscShader("Flame", out MiscShaderData flameShader);
		}

		private void LoadScreenShader(string name, string path)
		{
			var shaderAsset = Mod.Assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
			var screenshaderData = new ScreenShaderData(shaderAsset, name + "Pass");
			Filters.Scene[name] = new Filter(screenshaderData, EffectPriority.High);
			Filters.Scene[name].Load();
		}

		private void LoadMiscShader(string name, out MiscShaderData shaderData, string pass = null)
		{
			var shaderAsset = ModContent.Request<Effect>("ConstellationsOfOrion/Effects/" + name, AssetRequestMode.ImmediateLoad);
			shaderData = new MiscShaderData(shaderAsset, pass ?? name);
			GameShaders.Misc["ConstellationsOfOrion:" + name] = shaderData;
		}
	}
}

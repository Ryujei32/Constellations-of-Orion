using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

public static class GraphicsHelpers
{
    public static void uLightSource(this MiscShaderData shader, Vector3 value)
    {
        shader.Shader.Parameters["uLightSource"].SetValue(value);
    }
}
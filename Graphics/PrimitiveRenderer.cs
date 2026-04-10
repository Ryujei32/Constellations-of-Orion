using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Graphics
{
    [Autoload(Side = ModSide.Client)]
    public sealed class PrimitiveRenderer : ModSystem
    {
        private static DynamicVertexBuffer VertexBuffer;
        private static DynamicIndexBuffer IndexBuffer;
        private static VertexPosition2DColorTexture[] MainVertices;
        private static short[] MainIndices;
        private static int VerticesIndex;
        private static int IndicesIndex;

        private const short MaxVertices = 3072;
        private const short MaxIndices = 8192;

        public override void OnModLoad()
        {
            Main.QueueMainThreadAction(() =>
            {
                MainVertices = new VertexPosition2DColorTexture[MaxVertices];
                MainIndices = new short[MaxIndices];
                VertexBuffer ??= new DynamicVertexBuffer(Main.instance.GraphicsDevice, VertexPosition2DColorTexture.VertexDeclaration2D, MaxVertices, BufferUsage.WriteOnly);
                IndexBuffer ??= new DynamicIndexBuffer(Main.instance.GraphicsDevice, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
            });
        }

        public override void OnModUnload()
        {
            Main.QueueMainThreadAction(() =>
            {
                MainVertices = null;
                MainIndices = null;
                VertexBuffer?.Dispose();
                VertexBuffer = null;
                IndexBuffer?.Dispose();
                IndexBuffer = null;
            });
        }

        /// <summary>
        /// Renders a primitive trail.
        /// </summary>
        public static void RenderTrail(
            Vector2[] positions,
            PrimitiveSettings settings,
            int? pointsToCreate = null,
            bool flipUV = false,
            bool flipUVXY = false)
        {
            if (positions.Length <= 2)
                return;

            VerticesIndex = 0;
            IndicesIndex = 0;

            for (int i = 0; i < positions.Length; i++)
            {
                float completionRatio = i / (float)(positions.Length - 1);
                Vector2 screenPos = positions[i] - Main.screenPosition;
                float width = settings.WidthFunction(completionRatio);
                Color color = settings.ColorFunction(completionRatio);

                Vector2 tangent = i < positions.Length - 1 ?
                    Vector2.Normalize(positions[i + 1] - positions[i]) :
                    Vector2.Normalize(positions[i] - positions[i - 1]);
                Vector2 normal = new Vector2(-tangent.Y, tangent.X);

                Vector2 left = screenPos - normal * width * 0.5f;
                Vector2 right = screenPos + normal * width * 0.5f;
                float topUV = flipUV ? 1f : 0f;
                float bottomUV = flipUV ? 0f : 1f;
                Vector2 leftUV = new Vector2(completionRatio, topUV);
                Vector2 rightUV = new Vector2(completionRatio, bottomUV);

                if (flipUVXY)
                {
                    leftUV = new Vector2(leftUV.Y, leftUV.X);
                    rightUV = new Vector2(rightUV.Y, rightUV.X);
                }

                MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(left, color, leftUV, width);
                MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(right, color, rightUV, width);
            }

            int pointCount = positions.Length;
            for (short i = 0; i < pointCount - 2 && IndicesIndex + 5 < MaxIndices; i++)
            {
                short connectToIndex = (short)(i * 2);
                MainIndices[IndicesIndex++] = connectToIndex;
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 1);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 2);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 2);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 1);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 3);
            }

            Render(settings.UseShader, PrimitiveType.TriangleList, IndicesIndex / 3);
        }

        private static void Render(
            MiscShaderData useShader,
            PrimitiveType primitiveType,
            int primitiveCount,
            bool useScreenTransform = false
        )
        {
            if (VerticesIndex == 0 || IndicesIndex == 0)
                return;

            var device = Main.instance.GraphicsDevice;

            device.RasterizerState = RasterizerState.CullNone;
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearClamp;


            var shader = useShader ?? GameShaders.Misc["ConstellationsOfOrion:BaseVertex"];

            if (useScreenTransform)
            {
                CalculateScreenMatricies(out Matrix view, out Matrix projection);
                shader.Shader.Parameters["uWorldViewProjection"]
                .SetValue(view * projection);
            }
            else
            {
                CalculatePerspectiveMatricies(out Matrix view, out Matrix projection);
                shader.Shader.Parameters["uWorldViewProjection"]
                .SetValue(view * projection);
            }

            shader.Apply();

            VertexBuffer.SetData(MainVertices, 0, VerticesIndex, SetDataOptions.Discard);
            IndexBuffer.SetData(MainIndices, 0, IndicesIndex, SetDataOptions.Discard);

            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;

            device.DrawIndexedPrimitives(
                primitiveType,
                0,
                0,
                VerticesIndex,
                0,
                primitiveCount
            );
        }

        public static void RenderBeam(
            Vector2 start,
            Vector2 end,
            PrimitiveSettings settings,
            int numPoints)
        {
            if (numPoints < 2)
                return;

            var startScreen = start - Main.screenPosition;
            var endScreen = end - Main.screenPosition;

            var beamVec = endScreen - startScreen;
            var beamLength = beamVec.Length();

            if (beamLength <= float.Epsilon)
                return;

            var direction = beamVec / beamLength;
            var normal = new Vector2(-direction.Y, direction.X);

            var verts = new VertexPositionColorTexture[numPoints * 2];

            for (var i = 0; i < numPoints; i++)
            {
                var progress = i / (float)(numPoints - 1);
                var pos = Vector2.Lerp(startScreen, endScreen, progress);
                var width = settings.WidthFunction(progress);
                var halfWidth = width * 0.5f;
                var color = settings.ColorFunction(progress);
                var offset = normal * halfWidth;
                var vi = i * 2;

                verts[vi] = new VertexPositionColorTexture(
                    (pos + offset).WithZ(0f),
                    color,
                    new Vector2(progress, 0f)
                );

                verts[vi + 1] = new VertexPositionColorTexture(
                    (pos - offset).WithZ(0f),
                    color,
                    new Vector2(progress, 1f)
                );
            }

            var shader = settings.UseShader ?? GameShaders.Misc["CoolWeapons:BasicPrimitiveShader"];
            var device = Main.instance.GraphicsDevice;

            device.RasterizerState = RasterizerState.CullNone;
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearClamp;

            CalculatePerspectiveMatricies(out var view, out var projection);
            shader.Shader.Parameters["uWorldViewProjection"]
                .SetValue(view * projection);
            shader.Apply();

            device.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                verts,
                0,
                numPoints * 2 - 2
            );
        }
        public static void RenderRectangle(
            Vector2 screenPos,
            float width,
            float height,
            PrimitiveSettings settings,
            float uvRotation = 0f,
            bool flipUV = false,
            bool useScreenTransform = false
        )
        {
            VerticesIndex = 0;
            IndicesIndex = 0;

            var topLeft = screenPos;
            var topRight = topLeft + new Vector2(width, 0);
            var bottomLeft = topLeft + new Vector2(0, height);
            var bottomRight = topLeft + new Vector2(width, height);

            var color = settings.ColorFunction(0f);
            var w = settings.WidthFunction(0f);

            var topUV = flipUV ? 1f : 0f;
            var bottomUV = flipUV ? 0f : 1f;

            Vector2 RotateUV(Vector2 uv)
            {
                if (uvRotation == 0f) return uv;
                var centered = uv - new Vector2(0.5f);
                var cos = MathF.Cos(uvRotation);
                var sin = MathF.Sin(uvRotation);
                return new Vector2(
                    centered.X * cos - centered.Y * sin,
                    centered.X * sin + centered.Y * cos
                ) + new Vector2(0.5f);
            }

            MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(topLeft, color, RotateUV(new Vector2(0, topUV)), w);
            MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(topRight, color, RotateUV(new Vector2(1, topUV)), w);
            MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(bottomLeft, color, RotateUV(new Vector2(0, bottomUV)), w);
            MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(bottomRight, color, RotateUV(new Vector2(1, bottomUV)), w);

            MainIndices[IndicesIndex++] = 0;
            MainIndices[IndicesIndex++] = 1;
            MainIndices[IndicesIndex++] = 2;
            MainIndices[IndicesIndex++] = 2;
            MainIndices[IndicesIndex++] = 1;
            MainIndices[IndicesIndex++] = 3;

            Render(settings.UseShader, PrimitiveType.TriangleList, 2, useScreenTransform);
        }
        public static void CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix zoomScaleMatrix = Matrix.CreateScale(zoom.X, zoom.Y, 1f);

            int width = Main.instance.GraphicsDevice.Viewport.Width;
            int height = Main.instance.GraphicsDevice.Viewport.Height;

            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
            viewMatrix *= Matrix.CreateTranslation(0f, -height, 0f);
            viewMatrix *= Matrix.CreateRotationZ(MathHelper.Pi);

            if (Main.LocalPlayer.gravDir == -1f)
                viewMatrix *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, height, 0f);

            viewMatrix *= zoomScaleMatrix;

            projectionMatrix = Matrix.CreateOrthographicOffCenter(0f, width * zoom.X, 0f, height * zoom.Y, 0f, 1f) *
                               zoomScaleMatrix;
        }

        public static void CalculateScreenMatricies(out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1, 1
            );
        }
    }
}

public record PrimitiveSettings(MiscShaderData UseShader, Func<float, float> WidthFunction, Func<float, Color> ColorFunction)
{
    public static PrimitiveSettings UsingShader(MiscShaderData shader, Color color = default) => new(shader, _ => 1f, _ => color);
}

public static class VectorHelpers
{
    public static Vector3 WithZ(this Vector2 vector, float z)
    {
        return new Vector3(vector.X, vector.Y, z);
    }
}
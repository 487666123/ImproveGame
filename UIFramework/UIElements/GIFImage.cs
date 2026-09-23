using ImproveGame.Core;

namespace ImproveGame.UIFramework.UIElements
{
    public class GIFImage : UIElement
    {
        /// <summary>
        /// GIF未完成加载前的占位贴图，大小应与GIF保持一致
        /// </summary>
        public Asset<Texture2D> PreviewTexture;
        public TextureGIF Texture;
        public float Scale = 1f;

        public GIFImage(TextureGIF texture)
        {
            Texture = texture;
            Texture.ShouldLoop = true;
            Texture.Play();
            Width = StyleDimension.FromPixels(Texture.Width);
            Height = StyleDimension.FromPixels(Texture.Height);
        }

        public GIFImage(string texPath, int horizontalFrames, int verticalFrames, int totalFrames, int ticksPerFrame)
        {
            Texture = new(MyUtils.GetTexture($"GIFs/{texPath}").Value, horizontalFrames, verticalFrames, totalFrames, ticksPerFrame)
            {
                ShouldLoop = true
            };
            Texture.Play();
            Width = StyleDimension.FromPixels(Texture.Width);
            Height = StyleDimension.FromPixels(Texture.Height);
        }

        public GIFImage(string texPath, int horizontalFrames, int verticalFrames, int totalFrames, int ticksPerFrame, string parentPath)
        {
            Texture = new(MyUtils.GetTexture($"{parentPath}/{texPath}").Value, horizontalFrames, verticalFrames, totalFrames, ticksPerFrame)
            {
                ShouldLoop = true
            };
            Texture.Play();
            Width = StyleDimension.FromPixels(Texture.Width);
            Height = StyleDimension.FromPixels(Texture.Height);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Texture.UpdateGIF();
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dimensions = GetDimensions();
            var origin = new Vector2(Texture.Width, Texture.Height) / 2f;
            Texture.Draw(spriteBatch, dimensions.Center(), Color.White, 0f, origin, Scale, SpriteEffects.None);
        }
    }
}

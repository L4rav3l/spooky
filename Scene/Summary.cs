using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace spooky;

public class Summary : IScene
{
    private GraphicsDevice _graphicsDevice;
    private ContentManager _contentManager;
    private SceneManager _sceneManager;

    private SpriteFont _pixelfont;


    public void DrawText(SpriteBatch spriteBatch, string text, int y)
    {
        Vector2 TextM = _pixelfont.MeasureString(text);
        Vector2 Text = new Vector2(_graphicsDevice.Viewport.Width / 2 - TextM.X / 2, (_graphicsDevice.Viewport.Height / 2) + y);

        spriteBatch.DrawString(_pixelfont, text, Text, Color.White);
    }

    public Summary(GraphicsDevice graphicsDevice, ContentManager contentManager, SceneManager sceneManager)
    {
        _graphicsDevice = graphicsDevice;
        _contentManager = contentManager;
        _sceneManager = sceneManager;
    }

    public void LoadContent()
    {
        _pixelfont = _contentManager.Load<SpriteFont>("pixelfont");
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState state = Keyboard.GetState();

        if(state.IsKeyDown(Keys.E) && !GameData.oldState.IsKeyDown(Keys.E))
        {
            _sceneManager.ChangeScene("Menu");
        }

        GameData.oldState = state;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(Color.Gray);

        DrawText(spriteBatch, "GAME OVER", -50);
        DrawText(spriteBatch, $"Total earnings: {GameData.fullearnings} Total Sold: {GameData.fullSell}", 0);
        DrawText(spriteBatch, "Press E to back to the menu", +50);

    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace spooky;

public class Market : IScene
{
    private GraphicsDevice _graphicsDevice;
    private ContentManager _contentManager;
    private SceneManager _sceneManager;

    private SpriteFont _pixelfont;

    public Market(GraphicsDevice graphicsDevice, ContentManager contentManager, SceneManager sceneManager)
    {
        _graphicsDevice = graphicsDevice;
        _contentManager = contentManager;
        _sceneManager = sceneManager;
    }

    public void LoadContent()
    {

    }

    public void Update(GameTime gameTime)
    {

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        
    }
}
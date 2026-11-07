using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace spooky;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _sceneManager = new SceneManager();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _sceneManager.AddScene(new Menu(GraphicsDevice, Content, _sceneManager), "Menu");
        _sceneManager.ChangeScene("Menu");
    }

    protected override void Update(GameTime gameTime)
    {
        if(GameData.Quit)
        {
            Exit();
        }

        _sceneManager.GetCurrentScene().Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Gray);

        _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, samplerState:SamplerState.PointClamp);

        _sceneManager.GetCurrentScene().Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

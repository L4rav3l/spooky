using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace spooky;

public class Menu : IScene {

    private GraphicsDevice _graphicsDevice;
    private ContentManager _contentManager;
    private SceneManager _sceneManager;

    private SpriteFont _pixelfont;

    private int _selected = 0;

    private float _blinkingTime = 500;

    private bool _blinked = false;

    public Menu(GraphicsDevice graphicsDevice, ContentManager contentManager, SceneManager sceneManager)
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

        float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        _blinkingTime -= elapsed;

        if(_blinkingTime <= 0)
        {
            _blinkingTime = 500;
            _blinked = !_blinked;
        }

        if((state.IsKeyDown(Keys.Down) && !GameData.oldState.IsKeyDown(Keys.Down)) || (state.IsKeyDown(Keys.Up) && !GameData.oldState.IsKeyDown(Keys.Up)))
        {
            if(_selected == 0)
            {
                _selected = 1;
            } else {
                _selected = 0;
            }

            _blinkingTime = 500;
            _blinked = true;
        }

        if(state.IsKeyDown(Keys.Enter) && !GameData.oldState.IsKeyDown(Keys.Enter))
        {
            if(_selected == 0)
            {
                _sceneManager.AddScene(new Market(_graphicsDevice, _contentManager, _sceneManager), "Market");
                _sceneManager.ChangeScene("Market");
            } else {
                GameData.Quit = true;
            }
        }

        GameData.oldState = state;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int width = _graphicsDevice.Viewport.Width;
        int height = _graphicsDevice.Viewport.Height;

        string playString = "Play";
        string quitString = "Quit";

        if(_selected == 0)
        {
            if(_blinked)
            {
                playString = "> Play <";
            }

            quitString = "Quit";
        } else {
            playString = "Play";
            
            if(_blinked)
            {
                quitString = "> Quit <";
            }
        }

        _graphicsDevice.Clear(Color.Gray);

        Vector2 TitleM = _pixelfont.MeasureString("Spooky!");
        Vector2 Title = new Vector2((width / 2) - (TitleM.X / 2), (height / 4) - (TitleM.Y / 2));

        spriteBatch.DrawString(_pixelfont, "Spooky!", Title, Color.White);

        Vector2 PlayM = _pixelfont.MeasureString(playString);
        Vector2 Play = new Vector2((width / 2) - (PlayM.X / 2), (height / 2) - (PlayM.Y / 2));

        Vector2 QuitM = _pixelfont.MeasureString(quitString);
        Vector2 Quit = new Vector2((width / 2) - (QuitM.X / 2), (height / 2) - (QuitM.Y / 2) + 35);

        spriteBatch.DrawString(_pixelfont, playString, Play, Color.White);
        spriteBatch.DrawString(_pixelfont, quitString, Quit, Color.White);
    }
}
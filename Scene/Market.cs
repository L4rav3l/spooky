using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using TiledSharp;

namespace spooky;

public class Market : IScene
{
    private GraphicsDevice _graphicsDevice;
    private ContentManager _contentManager;
    private SceneManager _sceneManager;
    private MapManager _mapManager;

    private TmxMap _map;

    private Player _player;
    private Camera2D _camera;

    private SpriteFont _pixelfont;
    
    private Texture2D _playerTexture;

    private float _pumpkinCountdown = 20000;

    private Dictionary<int, List<Rectangle>> _pumpinkEvents = new();
    private bool[] _pumpkinAvailable = new bool[8];
    private bool[] _pumpkinEnter = new bool[8];

    public Market(GraphicsDevice graphicsDevice, ContentManager contentManager, SceneManager sceneManager)
    {
        _graphicsDevice = graphicsDevice;
        _contentManager = contentManager;
        _sceneManager = sceneManager;
    }

    public List<Rectangle> LoadObject(string mapFilePath, string name)
    {
        var map = new TmxMap(mapFilePath);
        var solidTiles = new List<Rectangle>();

        foreach(var objectGroup in map.ObjectGroups)
        {
            if(objectGroup.Name == name)
            {
                foreach(var obj in objectGroup.Objects)
                {
                    if(obj.Width > 0 && obj.Height > 0)
                    {
                        var rect = new Rectangle(
                            (int)obj.X,
                            (int)obj.Y,
                            (int)obj.Width,
                            (int)obj.Height
                        );

                        solidTiles.Add(rect);
                    }
                }
            }
        }

        return solidTiles;
    }

    public void RandomPumpkinAvailable()
    {
        List<int> unavailablePumpkin = new();

        for(int i = 0; i < 8; i++)
        {
            if(!_pumpkinAvailable[i])
            {
                unavailablePumpkin.Add(i);
            }
        }

        Random random = new Random();

        int index = random.Next(unavailablePumpkin.Count);
        _pumpkinAvailable[unavailablePumpkin[index]] = true;
    }

    public void LoadContent()
    {
        _map = new TmxMap("Content/market.tmx");

        _player = new Player(new Vector2(300, 300));
        _camera = new Camera2D(_graphicsDevice.Viewport);

        _pixelfont = _contentManager.Load<SpriteFont>("pixelfont");

        var dict = new Dictionary<int, Texture2D>();

        foreach (var ts in _map.Tilesets)
        {
            string asset = Path.GetFileNameWithoutExtension(ts.Image.Source);
            dict[ts.FirstGid] = _contentManager.Load<Texture2D>(asset);
        }

        for(int i = 0; i < 8; i++)
        {
            _pumpinkEvents[i] = LoadObject("Content/market.tmx", $"clickable{i+1}zone");
        }

        RandomPumpkinAvailable();
        RandomPumpkinAvailable();
        RandomPumpkinAvailable();

        _mapManager = new MapManager(_map, dict);
    
        _playerTexture = _contentManager.Load<Texture2D>("player");
    }

    public void Update(GameTime gameTime)
    {
        _player.Update(gameTime, new List<Rectangle>(), _camera);
        _camera.Follow(_player.Position, new Vector2(_map.Width * 64, _map.Height * 64));

        float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        _pumpkinCountdown -= elapsed;

        if(_pumpkinCountdown <= 0)
        {
            _pumpkinCountdown = 20000;
            RandomPumpkinAvailable();
        }

        KeyboardState state = Keyboard.GetState();

        for(int i = 0; i < 8; i++)
        {
            _pumpkinEnter[i] = false;

            foreach(Rectangle pumpkinEvent in _pumpinkEvents[i])
            {
                if(_player.Hitbox.Intersects(pumpkinEvent))
                {
                    if(_pumpkinAvailable[i] == true)
                    {
                        _pumpkinEnter[i] = true;
                    }
                }
            }

            if(_pumpkinEnter[i] && state.IsKeyDown(Keys.E) && !GameData.oldState.IsKeyDown(Keys.E))
            {
                _pumpkinAvailable[i] = false;
            }
        }

        GameData.oldState = state;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int width = _graphicsDevice.Viewport.Width;
        int height = _graphicsDevice.Viewport.Height;

        _graphicsDevice.Clear(Color.Black);

        _player.Draw(spriteBatch, _playerTexture, _camera);

        float LayerDeep = 0.01f;

        _mapManager.DrawLayer(_map.Layers["Tile Layer 1"], spriteBatch, LayerDeep, _camera);
        _mapManager.DrawLayer(_map.Layers["Tile Layer 2"], spriteBatch, LayerDeep+0.01f, _camera);

        for(int i = 0; i < 8; i++)
        {
            if(_pumpkinAvailable[i] == true)
            {
                _mapManager.DrawLayer(_map.Layers[$"clickable{i+1}"], spriteBatch, LayerDeep, _camera);
            }
        }

        for(int i = 0; i < 8; i++)
        {
            if(_pumpkinEnter[i])
            {
                Vector2 pumpkinTextM = _pixelfont.MeasureString("Press [E] to pick up the pumpkin.");
                Vector2 pumpkinText = new Vector2((width / 2) - (pumpkinTextM.X / 2), ((height / 10) * 8) - pumpkinTextM.Y / 2);

                spriteBatch.DrawString(_pixelfont, "Press [E] to pick up the pumpkin.", pumpkinText, Color.White);
            }
        }

        /* foreach(var layer in _map.Layers)
        {
            if(layer.Name )
            _mapManager.DrawLayer(layer, spriteBatch, LayerDeep, _camera);
            LayerDeep += 0.01f;
        } */
    }
}
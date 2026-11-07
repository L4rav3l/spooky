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

    public Market(GraphicsDevice graphicsDevice, ContentManager contentManager, SceneManager sceneManager)
    {
        _graphicsDevice = graphicsDevice;
        _contentManager = contentManager;
        _sceneManager = sceneManager;
    }

    public void LoadContent()
    {
        _map = new TmxMap("Content/market.tmx");

        _player = new Player(new Vector2(300, 300));
        _camera = new Camera2D(_graphicsDevice.Viewport);

        var dict = new Dictionary<int, Texture2D>();

        foreach (var ts in _map.Tilesets)
        {
            string asset = Path.GetFileNameWithoutExtension(ts.Image.Source);
            dict[ts.FirstGid] = _contentManager.Load<Texture2D>(asset);
        }

        _mapManager = new MapManager(_map, dict);
    
        _playerTexture = _contentManager.Load<Texture2D>("player");
    }

    public void Update(GameTime gameTime)
    {
        _player.Update(gameTime, new List<Rectangle>(), _camera);
        _camera.Follow(_player.Position, new Vector2(_map.Width * 64, _map.Height * 64));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(Color.Black);
        
        _player.Draw(spriteBatch, _playerTexture, _camera);

        float LayerDeep = 0.01f;

        foreach(var layer in _map.Layers)
        {
            _mapManager.DrawLayer(layer, spriteBatch, LayerDeep, _camera);
            LayerDeep += 0.01f;
        }
    }
}
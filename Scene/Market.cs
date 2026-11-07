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

    public Market(GraphicsDevice graphicsDevice, ContentManager contentManager, SceneManager sceneManager)
    {
        _graphicsDevice = graphicsDevice;
        _contentManager = contentManager;
        _sceneManager = sceneManager;
    }

    public void LoadContent()
    {
        _map = new TmxMap("Content/house.tmx");

        var dict = new Dictionary<int, Texture2D>();

        _mapManager = new MapManager(_map, dict);
    }

    public void Update(GameTime gameTime)
    {

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float LayerDeep = 0.01f;

        foreach(var layer in _map.Layers)
        {
            _mapManager.DrawLayer(layer, spriteBatch, LayerDeep, _camera);
            LayerDeep += 0.01f;
        }
    }
}
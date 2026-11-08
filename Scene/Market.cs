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

    private Texture2D _rectangle;
    private Texture2D _playerTexture;
    private Texture2D _pumpkin;
    private Texture2D _candle;

    private MouseState _previousMouseState;

    private float _pumpkinCountdown = 20000;
    private int _selectedInventory = 1;
    private int _pumpkinStore = 1;
    private int _candleStore = 1;
    private int _money = 0;
    private float _indicatorCountdown = 3000;

    private List<Rectangle> _candleShelf;
    private List<Rectangle> _pumpkinShelf;
    private List<Rectangle> _solidTiles;
    private List<Rectangle> _candleTiles;
    private List<Rectangle> _candleMarket;
    private Rectangle _buyButton;
    private Rectangle _backButton;

    private Dictionary<int, List<Rectangle>> _pumpinkEvents = new();
    private Dictionary<int, int> _inventory = new();
    private bool[] _pumpkinAvailable = new bool[8];
    private bool[] _pumpkinEnter = new bool[8];
    private bool _pumpkinShelfEnter;
    private bool _candleShelfEnter;
    private bool _candleShop;
    private bool _candleShopMenu;
    private bool _nomoney;
    private bool _nostock;

    private string _indicator;

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

        _buyButton = new Rectangle(_graphicsDevice.Viewport.Width / 2 - 50, _graphicsDevice.Viewport.Height / 2 + 175, 100, 50);
        _backButton = new Rectangle(_graphicsDevice.Viewport.Width / 2 + 225, _graphicsDevice.Viewport.Height / 2 - 220, 40, 40);

        _inventory[1] = 0;
        _inventory[2] = 0;
        _inventory[3] = 0;

        _rectangle = new Texture2D(_graphicsDevice, 1, 1);
        _rectangle.SetData(new[] { Color.White });

        _player = new Player(new Vector2(1390, 2130));
        _camera = new Camera2D(_graphicsDevice.Viewport);

        _pixelfont = _contentManager.Load<SpriteFont>("pixelfont");

        _pumpkin = _contentManager.Load<Texture2D>("pumpkin");
        _candle = _contentManager.Load<Texture2D>("candle");

        var dict = new Dictionary<int, Texture2D>();

        foreach (var ts in _map.Tilesets)
        {
            string asset = Path.GetFileNameWithoutExtension(ts.Image.Source);
            dict[ts.FirstGid] = _contentManager.Load<Texture2D>(asset);
        }

        _solidTiles = LoadObject("Content/market.tmx", "SolidTiles");
        _pumpkinShelf = LoadObject("Content/market.tmx", "pumpkinZone");
        _candleShelf = LoadObject("Content/market.tmx", "candleZone");
        _candleMarket = LoadObject("Content/market.tmx", "candlestore");

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
        _player.Update(gameTime, _solidTiles, _camera);
        _camera.Follow(_player.Position, new Vector2(_map.Width * 64, _map.Height * 64));

        float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if(_nomoney)
        {
            _indicator = "There are insufficient money.";
        } else if(_nostock) {
            _indicator = "There is not enough space.";
        }

        if(_indicator != "")
        {
            _indicatorCountdown -= elapsed;

            if(_indicatorCountdown <= 0)
            {
                _nomoney = false;
                _nostock = false;
                _indicator = "";
                _indicatorCountdown = 5000;
            }
        }

        _pumpkinCountdown -= elapsed;

        if(_pumpkinCountdown <= 0)
        {
            _pumpkinCountdown = 20000;
            RandomPumpkinAvailable();
        }

        KeyboardState state = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        if(state.IsKeyDown(Keys.D1) && !GameData.oldState.IsKeyDown(Keys.D1))
        {
            _selectedInventory = 1;
        }

        if(state.IsKeyDown(Keys.D2) && !GameData.oldState.IsKeyDown(Keys.D2))
        {
            _selectedInventory = 2;
        }

        if(state.IsKeyDown(Keys.D3) && !GameData.oldState.IsKeyDown(Keys.D3))
        {
            _selectedInventory = 3;
        }

        foreach(Rectangle pumpkin in _pumpkinShelf)
        {
            _pumpkinShelfEnter = false;

            if(_player.Hitbox.Intersects(pumpkin))
            {
                _pumpkinShelfEnter = true;

                if(state.IsKeyDown(Keys.E) && GameData.oldState.IsKeyUp(Keys.E))
                {
                    if(_inventory[_selectedInventory] == 1 && _pumpkinStore < 3)
                    {
                        _inventory[_selectedInventory] = 0;
                        _pumpkinStore++;
                        break;
                    } else if(_inventory[_selectedInventory] == 0 && _pumpkinStore > 0)
                    {
                        _inventory[_selectedInventory] = 1;
                        _pumpkinStore--;
                        break;
                    }
                }
            }
        }

        foreach(Rectangle candle in _candleMarket)
        {
            _candleShop = false;

            if(_player.Hitbox.Intersects(candle))
            {
                _candleShop = true;

                if(state.IsKeyDown(Keys.E) && !GameData.oldState.IsKeyDown(Keys.E))
                {
                    GameData.Move = false;
                    _candleShopMenu = true;
                }
            }
        }

        foreach(Rectangle candle in _candleShelf)
        {
            _candleShelfEnter = false;

            if(_player.Hitbox.Intersects(candle))
            {
                _candleShelfEnter = true;

                if(state.IsKeyDown(Keys.E) && GameData.oldState.IsKeyUp(Keys.E))
                {
                    if(_inventory[_selectedInventory] == 2 && _candleStore < 3)
                    {
                        _inventory[_selectedInventory] = 0;
                        _candleStore++;
                        break;
                    } else if(_inventory[_selectedInventory] == 0 && _candleStore > 0)
                    {
                        _inventory[_selectedInventory] = 2;
                        _candleStore--;;
                        break;
                    }
                }
            }
        }

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
                if(_inventory[_selectedInventory] == 0)
                {
                    _inventory[_selectedInventory] = 1;
                    _pumpkinAvailable[i] = false;
                }
            }
        }

        if(_buyButton.Contains(mouseState.Position) && _candleShop)
        {
            if(mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed)
            {
                if(_money >= 50)
                {
                    if(_inventory[1] == 0)
                    {
                        _money-=50;
                        _inventory[1] = 2;
                    } else if(_inventory[2] == 0)
                    {
                        _money-=50;
                        _inventory[2] = 2;
                    } else if(_inventory[3] == 0)
                    {
                        _money-=50;
                        _inventory[3] = 2;
                    } else {    
                        _nostock = true;
                    }
                } else {
                    _nomoney = true;
                }
            }
        }

        if(_backButton.Contains(mouseState.Position) && _candleShop)
        {
            if(mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed)
            {
                _candleShopMenu = false;
                GameData.Move = true;
            }
        }

        int scrollDifference = mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;

        if (scrollDifference > 0)
        {
            if(_selectedInventory == 3) {
                _selectedInventory = 2;
            } else if(_selectedInventory == 2) {
                _selectedInventory = 1;
            } else if(_selectedInventory == 1) {
                _selectedInventory = 3;
            }
        }
        else if (scrollDifference < 0)
        {
            if(_selectedInventory == 1) {
                _selectedInventory = 2;
            } else if(_selectedInventory == 2) {
                _selectedInventory = 3;
            } else if(_selectedInventory == 3) {
                _selectedInventory = 1;
            }
        }

        GameData.oldState = state;
        _previousMouseState = mouseState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int width = _graphicsDevice.Viewport.Width;
        int height = _graphicsDevice.Viewport.Height;

        _graphicsDevice.Clear(Color.Black);

        if (!string.IsNullOrEmpty(_indicator) && _pixelfont != null)
        {
            Vector2 indicatorSize = _pixelfont.MeasureString(_indicator);
            Vector2 indicatorPosition = new Vector2(width / 2 - (indicatorSize.X / 2), (height / 4) - indicatorSize.Y / 2);

            spriteBatch.DrawString(_pixelfont, _indicator, indicatorPosition, Color.White);
        }

        _player.Draw(spriteBatch, _playerTexture, _camera);

        float LayerDeep = 0.01f;

        _mapManager.DrawLayer(_map.Layers["Tile Layer 1"], spriteBatch, LayerDeep, _camera);
        _mapManager.DrawLayer(_map.Layers["Tile Layer 2"], spriteBatch, LayerDeep+0.01f, _camera);

        _mapManager.DrawLayer(_map.Layers[$"pumpkin{_pumpkinStore}"], spriteBatch, LayerDeep+0.01f, _camera);
        _mapManager.DrawLayer(_map.Layers[$"candle{_candleStore}"], spriteBatch, LayerDeep+0.01f, _camera);

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
                Vector2 pumpkinText = new Vector2((width / 2) - (pumpkinTextM.X / 2), ((height / 10) * 7) - pumpkinTextM.Y / 2);

                spriteBatch.DrawString(_pixelfont, "Press [E] to pick up the pumpkin.", pumpkinText, Color.White);
            }
        }

        Vector2 MoneyM = _pixelfont.MeasureString($"$ {_money}");
        Vector2 Money = new Vector2(25, 25);

        spriteBatch.DrawString(_pixelfont, $"$ {_money}", Money, Color.White);

        Vector2 CandleM = _pixelfont.MeasureString("Press [E] to interact with candle storage.");
        Vector2 Candle = new Vector2((width / 2) - (CandleM.X / 2), ((height / 10) * 7) - CandleM.Y / 2);

        if(_candleShelfEnter)
        {
            spriteBatch.DrawString(_pixelfont, "Press [E] to interact with candle storage.", Candle, Color.White);
        }

        Vector2 PumpkinM = _pixelfont.MeasureString("Press [E] to interact with pumpkin storage.");
        Vector2 Pumpkin = new Vector2((width / 2) - (CandleM.X / 2), ((height / 10) * 7) - CandleM.Y / 2);

        if(_pumpkinShelfEnter)
        {
            spriteBatch.DrawString(_pixelfont, "Press [E] to interact with pumpkin storage.", Pumpkin, Color.White);
        }

        Vector2 ShopM = _pixelfont.MeasureString("Press [E] to open illegal market.");
        Vector2 Shop = new Vector2((width / 2) - (ShopM.X / 2), ((height / 10) * 7) - ShopM.Y / 2);

        if(_candleShop && !_candleShopMenu)
        {
            spriteBatch.DrawString(_pixelfont, "Press [E] to open illegal market.", Shop, Color.White);
        }

        if(_candleShopMenu)
        {
            spriteBatch.Draw(_rectangle, new Rectangle(width / 2 - 300, height / 2 - 250, 600, 500), null, Color.Brown, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            
            Vector2 BuyCandleM = _pixelfont.MeasureString("BUY CANDLE");
            Vector2 BuyCandle = new Vector2((width / 2) - (BuyCandleM.X / 2), (height / 4) - (BuyCandleM.Y / 2) + 50);

            spriteBatch.DrawString(_pixelfont, "BUY CANDLE", BuyCandle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.91f);

            spriteBatch.Draw(_candle, new Vector2(width / 2 - 160, height / 2 - 200), null, Color.White, 0f, Vector2.Zero, 5f, SpriteEffects.None, 0.91f);

            spriteBatch.Draw(_rectangle, new Rectangle(width / 2 - 50, height / 2 + 175, 100, 50), null, Color.Gray * 0.2f, 0f, Vector2.Zero, SpriteEffects.None, 0.91f);

            Vector2 BuyM = _pixelfont.MeasureString("$50");
            Vector2 Buy = new Vector2(width / 2 - (BuyM.X / 2), (height / 2) + 200 - (BuyM.Y / 2));

            spriteBatch.DrawString(_pixelfont, "$50", Buy, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.92f);

            spriteBatch.Draw(_rectangle, new Rectangle(width / 2 + 225, height / 2 - 220, 40, 40), null, Color.Gray * 0.2f, 0f, Vector2.Zero, SpriteEffects.None, 0.91f);
            spriteBatch.DrawString(_pixelfont, "X", new Vector2(width / 2 + 235, height / 2 - 220), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.92f);
        }

        int w = 100;
        int h = 100;

        if(_selectedInventory == 1)
        {
            spriteBatch.Draw(_rectangle, new Rectangle((width / 2) - 250, (height / 10) * 8, w, h), Color.White * 0.4f);
        } else {
            spriteBatch.Draw(_rectangle, new Rectangle((width / 2) - 250, (height / 10) * 8, w, h), Color.Gray * 0.3f);
        }

        if(_inventory[1] == 1)
        {
            spriteBatch.Draw(_pumpkin, new Vector2((width / 2) - 300 + 66, (height / 10) * 8 + 10), Color.White);
        }

        if(_inventory[1] == 2)
        {
            spriteBatch.Draw(_candle, new Vector2((width / 2) - 300 + 66, (height / 10) * 8 + 10), Color.White);
        }

        if(_selectedInventory == 2)
        {
            spriteBatch.Draw(_rectangle, new Rectangle((width / 2) - 50, (height / 10) * 8, w, h), Color.White * 0.4f);
        } else {
            spriteBatch.Draw(_rectangle, new Rectangle((width / 2) - 50, (height / 10) * 8, w, h), Color.Gray * 0.3f);
        }

        if(_inventory[2] == 1)
        {
            spriteBatch.Draw(_pumpkin, new Vector2((width / 2) - 100 + 66, (height / 10) * 8 + 10), Color.White);
        }

        if(_inventory[2] == 2)
        {
            spriteBatch.Draw(_candle, new Vector2((width / 2) - 100 + 66, (height / 10) * 8 + 10), Color.White);
        }

        if(_selectedInventory == 3)
        {
            spriteBatch.Draw(_rectangle, new Rectangle((width / 2) + 150, (height / 10) * 8, w, h), Color.White * 0.4f);
        } else {
            spriteBatch.Draw(_rectangle, new Rectangle((width / 2) + 150, (height / 10) * 8, w, h), Color.Gray * 0.3f);
        }

        if(_inventory[3] == 1)
        {
            spriteBatch.Draw(_pumpkin, new Vector2((width / 2) + 100 + 66, (height / 10) * 8 + 10), Color.White);
        }

        if(_inventory[3] == 2)
        {
            spriteBatch.Draw(_candle, new Vector2((width / 2) + 100 + 66, (height / 10) * 8 + 10), Color.White);
        }
    }
}
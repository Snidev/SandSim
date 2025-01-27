using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SandSim.Simulation;

namespace SandSim.Monogame;

public class MonogameInstance : Game
{
    private GraphicsDeviceManager _gdm;
    private SpriteBatch _spriteBatch;
    private Color[] _rawTexture = new Color[Width * Height];
    private Texture2D _texture;
    private World _world = new(Width, Height);

    private const int Width = 100;
    private const int Height = 100;
    private const int Magnification = 4;

    private int _pen = 1;
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_texture, new Rectangle(0, 0, Width * Magnification, Height * Magnification), Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        _world.Update();
        
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            int localX = mouse.X / Magnification;
            int localY = mouse.Y / Magnification;

            if (localX >= 0 && localX < _world.Width && localY >= 0 && localY < _world.Height)
            {
                _world.SetDot(localX, localY, _pen);
            }
        }
        
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.D1))
            _pen = 1;
        if (keyboard.IsKeyDown(Keys.D2))
            _pen = 2;
        if (keyboard.IsKeyDown(Keys.D0))
            _pen = 0;
        
        for (int x = 0; x < _world.Width; x++)
        for (int y = 0; y < _world.Height; y++)
        {
            int dot = _world.GetDot(x, y);
            _rawTexture[Width * y + x] = dot switch
            {
                1 => Color.Yellow,
                2 => Color.Aqua,
                _ => Color.Black
            };
        }
        
        _texture.SetData(_rawTexture);
        
        base.Update(gameTime);
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _world.SetDot(50, 50, 1);
        IsMouseVisible = true;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _texture = new Texture2D(GraphicsDevice, Width, Height);
        base.LoadContent();
    }

    public MonogameInstance()
    {
        _gdm = new GraphicsDeviceManager(this);
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SandSim.Simulation;
using SandSim.Simulation.DotTypes;
using SandSim.Simulation.Physics;

namespace SandSim.Monogame;

public class MonogameInstance : Game
{
    private GraphicsDeviceManager _gdm;
    private SpriteBatch _spriteBatch;
    private Color[] _rawTexture = new Color[Width * Height];
    private Texture2D _texture;
    private World _world = new(new Point(Width, Height));

    private int _pen = 0;

    private const int Width = 100;
    private const int Height = 100;
    private const int Magnification = 4;
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
            Point localMouse = new(mouse.X / Magnification, mouse.Y / Magnification);

            if (_world.IsInBounds(localMouse))
            {
                Dot? newDot = _pen switch
                {
                    1 => new SandDot(_world),
                    2 => new WaterDot(_world),
                    _ => null,
                };

                if (newDot is null)
                    _world.DeleteDot(localMouse);
                else if (_world.IsOpen(localMouse))
                    _world.AddDot(newDot, localMouse);
            }
        }
        
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.D0))
            _pen = 0;
        if (keyboard.IsKeyDown(Keys.D1))
            _pen = 1;
        if (keyboard.IsKeyDown(Keys.D2))
            _pen = 2;
        
        for (int x = 0; x < _world.Size.X; x++)
        for (int y = 0; y < _world.Size.Y; y++)
        {
            _rawTexture[_world.Size.X * y + x] = _world.GetDot(new Point(x, y))?.Color ?? Color.Black;
        }
        
        _texture.SetData(_rawTexture);
        
        base.Update(gameTime);
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        IsMouseVisible = true;
        
        // Testing linear traces
        
        TestLinearTrace(new Point(49, 49), new Point(49, 94));
        TestLinearTrace(new Point(49, 49), new Point(94, 49));
        TestLinearTrace(new Point(49, 49), new Point(49, 4));
        TestLinearTrace(new Point(49, 49), new Point(4, 49));
        
        TestLinearTraceTheta(new Point(50, 50), 0, 45);
        TestLinearTraceTheta(new Point(50, 50), Math.PI / 2, 45);
        TestLinearTraceTheta(new Point(50, 50), Math.PI, 45);
        TestLinearTraceTheta(new Point(50, 50), Math.PI * 3 / 2, 45);
        
        base.Initialize();
        return;

        void TestLinearTrace(Point start, Point end)
        {
            LinearTrace trace = new(_world, start, end);

            while (!trace.Finished)
            {
                if (trace.Step() is not null)
                    continue;

                DebugDot d = new(_world);
                d.DotColor = Color.Blue;
                if (trace.Finished)
                    d.DotColor = Color.Red;
                _world.AddDot(d, trace.CurPosition);
            }
        }

        void TestLinearTraceTheta(Point start, double theta, int distance)
        {
            LinearTrace trace = new(_world, start, theta, distance);
            
            while (!trace.Finished)
            {
                if (trace.Step() is not null)
                    continue;

                DebugDot d = new(_world);
                if (trace.Finished)
                    d.DotColor = Color.Red;
                _world.AddDot(d, trace.CurPosition);
            }
        }
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
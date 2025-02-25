using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SandSim.Simulation;
using SandSim.Simulation.ComponentData;
using SandSim.Simulation.System;

namespace SandSim.Monogame;

using MGPoint = Microsoft.Xna.Framework.Point;
using Point = SandSim.Data.Point;

public class MonogameInstance : Game
{
    private GraphicsDeviceManager _gdm;
    private SpriteBatch _spriteBatch;
    private Color[] _rawTexture = new Color[Width * Height];
    private Texture2D _texture;
    private World _world = new(new Point(Width, Height));
    private MonogameRenderer _monogameRenderer;
    private SpriteFont _sf;

    private DotTemplateSystem _templates;

    private int _pen = 0;
    private string fpsCounter = "FPS:      ";
    private string particleCounter = "Particles:               ";
    

    private const int Width = 800;
    private const int Height = 480;
    private const int Magnification = 1;
    protected override void Draw(GameTime gameTime)
    {
        Span<char> fpsCtr = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<char>(fpsCounter.GetPinnableReference()),
            fpsCounter.Length);
        Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds).TryFormat(fpsCtr[5..], out int w);
        Span<char> clear = fpsCtr[(5 + w)..];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = ' ';
        
        Span<char> pCtr = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<char>(particleCounter.GetPinnableReference()),
            particleCounter.Length);
        _world.Particles.TryFormat(pCtr[11..], out w);
        clear = pCtr[(11 + w)..];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = ' ';
        
        GraphicsDevice.Clear(Color.Black);
        
        _monogameRenderer.Draw(_spriteBatch);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        /*_spriteBatch.Draw(_texture, new Rectangle(0, 0, Width * Magnification, Height * Magnification), Color.White);*/
        _spriteBatch.DrawString(_sf, fpsCounter, Vector2.One, Color.White);
        _spriteBatch.DrawString(_sf, particleCounter, new Vector2(1, 30), Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        _world.Update();

        int brushSize = 16;
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Point localMouse = new(mouse.X / Magnification, mouse.Y / Magnification);
            for (int x = -brushSize/2; x < brushSize/2; x++)
            {
                for (int y = -brushSize/2; y < brushSize/2; y++)
                {
                    Point bPoint = new(localMouse.X + x, localMouse.Y + y);
                    if (_world.IsInBounds(bPoint))
                    {
                        string? type = _pen switch
                        {
                            1 => "sand",
                            2 => "water",
                            _ => null
                        };
                        if (type is null)
                            _world.DeleteDot(bPoint);
                        else if (_world.IsEmpty(bPoint))
                            _templates.InstantiateFromTemplate(bPoint, type);
                    }
                }
            }
        }
    
        
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.D0))
            _pen = 0;
        if (keyboard.IsKeyDown(Keys.D1))
            _pen = 1;
        if (keyboard.IsKeyDown(Keys.D2))
            _pen = 2;
        if (keyboard.IsKeyDown(Keys.D3))
            _pen = 3;
        
        _monogameRenderer.Update();
        base.Update(gameTime);
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        IsMouseVisible = true;
        
        _monogameRenderer = new MonogameRenderer(_world, GraphicsDevice);
        _monogameRenderer.Scale = Magnification;
        
        _templates.AddTemplate("sand", (_, ent) =>
        {
            Span<Color> colors = [new Color(255, 200, 80), new Color(255, 220, 100), new Color(255, 210, 90)];

            _world.AllocateComponent(ent, (int)Simulation.Components.ColorData,
                new ColorData { Color = colors[_world.Random.Next(0, colors.Length)] });
            _world.AllocateComponent(ent, (int)Simulation.Components.DynamicSolid);
        });
        
        _templates.AddTemplate("water", (_, ent) =>
        {
            _world.AllocateComponent(ent, (int)Simulation.Components.ColorData, new ColorData {Color = Color.CornflowerBlue});
            _world.AllocateComponent(ent, (int)Simulation.Components.DynamicLiquid);
        });
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _texture = new Texture2D(GraphicsDevice, Width, Height);
        _sf = Content.Load<SpriteFont>("Monogame/Content/ArialFont");
        
        base.LoadContent();
    }

    public MonogameInstance()
    {
        _gdm = new GraphicsDeviceManager(this);

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        InactiveSleepTime = TimeSpan.Zero;
        _gdm.SynchronizeWithVerticalRetrace = false;
        _gdm.ApplyChanges();
        _templates = new DotTemplateSystem(_world);
    }
}
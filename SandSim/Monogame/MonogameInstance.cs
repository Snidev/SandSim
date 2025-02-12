using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    private World _world = new(new Point(Width, Height));
    private MonogameRenderer _monogameRenderer;
    private SpriteFont _sf;

    private int _pen = 0;
    private string fpsCounter = "FPS:      ";
    private string particleCounter = "Particles:               ";
    

    private const int Width = 400;
    private const int Height = 240;
    private const int Magnification = 2;
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
        
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
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

        int brushSize = 8;
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
                        DotType newDot = _pen switch
                        {
                            1 => DotType.Sand,
                            _ => DotType.Empty,
                        };

                        if (newDot == DotType.Empty)
                            _world.DeleteDot(bPoint);
                        else if (_world.IsOpen(bPoint))
                            _world.AddDot(newDot, bPoint);
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
        
        /*for (int x = 0; x < _world.Size.X; x++)
        for (int y = 0; y < _world.Size.Y; y++)
        {
            Point point = new(x, y);
            bool update = !_world.IsPointSleeping(point);
            
            
             /*Color pColor = _world.GetComponentOrDefault<DotType>(new Point(x, y), Simulation.Components.DotType) == DotType.Sand ? Color.Yellow : Color.Black;
            _rawTexture[_world.Size.X * y + x] = pColor;
            
            if (!update)
                _rawTexture[_world.Size.X * y + x] = new Color(pColor.R + 100, pColor.G, pColor.B);
        #1#
        }
        
        int xChunks = _world.Size.X / _world.ChunkSize + 1;
        int yChunks = _world.Size.Y / _world.ChunkSize + 1;
        
        for (int xChunk = 0; xChunk < xChunks; xChunk++)
        for (int yChunk = 0; yChunk < yChunks; yChunk++)
        {
            Point chunk = new(xChunk, yChunk);
            if (_world.IsChunkSleeping(chunk))
                continue;

            int xLim = _world.ChunkSize - Math.Max(0, (xChunk + 1) * _world.ChunkSize - _world.Size.X);
            int yLim = _world.ChunkSize - Math.Max(0, (yChunk + 1) * _world.ChunkSize - _world.Size.Y);
            for (int xRel = 0; xRel < xLim; xRel++)
            for (int yRel = 0; yRel < yLim; yRel++)
            {
                Point position = new Point(xChunk * _world.ChunkSize + xRel, yChunk * _world.ChunkSize + yRel);
                DotType dot = _world.GetComponentOrDefault<DotType>(position, Simulation.Components.DotType);
                _rawTexture[position.Y * _world.Size.X + position.X] = dot switch
                {
                    DotType.Sand => Color.Yellow,
                    _ => Color.Black
                };
            }
        }
        
        _texture.SetData(_rawTexture);
        */
        
        _monogameRenderer.Update();
        
        base.Update(gameTime);
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        IsMouseVisible = true;
        
        _monogameRenderer = new MonogameRenderer(_world, GraphicsDevice);
        _monogameRenderer.Scale = 2;
        
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
    }
}
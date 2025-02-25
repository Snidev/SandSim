#define MCORE

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SandSim.Simulation;
using SandSim.Simulation.ComponentData;

using Point = SandSim.Data.Point;
using Rectangle = SandSim.Data.Rectangle;
using MGPoint = Microsoft.Xna.Framework.Point;
using MGRect = Microsoft.Xna.Framework.Rectangle;

namespace SandSim.Monogame;

public class MonogameRenderer
{
    public int Scale { get; set; } = 1;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly ChunkRenderer[,] _chunkRenderers;

    public MonogameRenderer(World world, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        int xChunks = world.Size.X / world.ChunkSize + (world.Size.X % world.ChunkSize == 0 ? 0 : 1);
        int yChunks = world.Size.Y / world.ChunkSize + (world.Size.Y % world.ChunkSize == 0 ? 0 : 1);
        _chunkRenderers = new ChunkRenderer[xChunks, yChunks];
        
        for (int x = 0; x < xChunks; x++)
        for (int y = 0; y < yChunks; y++)
            _chunkRenderers[x, y] = new ChunkRenderer(this, world,
                new Rectangle(x * world.ChunkSize, 
                    y * world.ChunkSize, 
                    world.ChunkSize - Math.Max(0, (x + 1) * world.ChunkSize - world.Size.X ),  
                    world.ChunkSize - Math.Max(0, (y + 1) * world.ChunkSize - world.Size.Y)));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(Scale));
        for (var index0 = 0; index0 < _chunkRenderers.GetLength(0); index0++)
        for (var index1 = 0; index1 < _chunkRenderers.GetLength(1); index1++)
        {
            ref ChunkRenderer chunkRenderer = ref _chunkRenderers[index0, index1];
            MGRect rect = new MGRect(chunkRenderer.Bounds.X, chunkRenderer.Bounds.Y, chunkRenderer.Bounds.Width, chunkRenderer.Bounds.Height);
            spriteBatch.Draw(chunkRenderer.ChunkTexture, rect, Color.White);
        }

        spriteBatch.End();
    }

    public void Update()
    {
        # if MCORE
        Parallel.For(0, _chunkRenderers.GetLength(0) * _chunkRenderers.GetLength(1), i =>
        {
            int x = i % _chunkRenderers.GetLength(0);
            int y = i / _chunkRenderers.GetLength(0);

            ref ChunkRenderer chunkRenderer = ref _chunkRenderers[x, y];
            chunkRenderer.ProcessChunk();
        });
        # else
        for (var index0 = 0; index0 < _chunkRenderers.GetLength(0); index0++)
        for (var index1 = 0; index1 < _chunkRenderers.GetLength(1); index1++)
        {
            ref ChunkRenderer chunkRenderer = ref _chunkRenderers[index0, index1];
            chunkRenderer.ProcessChunk();
        }
        #endif
    }



    private struct ChunkRenderer
    {
        public readonly Color[] ColorData;
        private bool _sleeping = false;
        private readonly Texture2D _texture;
        private readonly World _world;
        private Rectangle _bounds;
        private bool _updateTexture = true;

        public Rectangle Bounds => _bounds;
        
        public ChunkRenderer(MonogameRenderer parent, World world, Rectangle bounds)
        {
            _world = world;
            _bounds = bounds;
            ColorData = new Color[bounds.Width * bounds.Height];
            _texture = new Texture2D(parent._graphicsDevice, bounds.Width, bounds.Height);
        }

        private int PointToIndex(int x, int y) => y * Bounds.Width + x;
        private int PointToIndex(Point point) => PointToIndex(point.X, point.Y);
        
        public void ProcessChunk()
        {
            Point chunk = new(_bounds.Location.X / _world.ChunkSize, _bounds.Location.Y / _world.ChunkSize);
            bool isProcessSleeping = _world.IsChunkSleeping(chunk);
            if (_sleeping && isProcessSleeping)
                return;

            _sleeping = isProcessSleeping;
            _updateTexture = true;
            
            
            for (int x = 0; x < _bounds.Width; x++)
            for (int y = 0; y < _bounds.Height; y++)
            {
                Point global = _bounds.Location + new Point(x, y);

                _world.GetComponentOrDefault(global, Components.ColorData, out ColorData col);
                Color color = col.Color;
                
                if (x == 0 || y == 0 || x == _bounds.Width - 1 || y == _bounds.Height - 1)
                    color = _sleeping ? Color.Red : Color.Green;
                
                ColorData[PointToIndex(x, y)] = color;
            }
        }
        
        public Texture2D ChunkTexture
        {
            get
            {
                if (_updateTexture)
                {
                    _updateTexture = false;
                    _texture.SetData(ColorData);
                }
                return _texture;
            }
        }
    }
}
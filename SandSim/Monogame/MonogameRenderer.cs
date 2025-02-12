using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SandSim.Simulation;

namespace SandSim.Monogame;

public class MonogameRenderer
{
    public int Scale { get; set; } = 1;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly ChunkRenderer[,] _chunkRenderers;

    public MonogameRenderer(World world, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        int xChunks = world.Size.X / world.ChunkSize + 1;
        int yChunks = world.Size.Y / world.ChunkSize + 1;
        _chunkRenderers = new ChunkRenderer[xChunks, yChunks];
        
        for (int x = 0; x < xChunks; x++)
        for (int y = 0; y < yChunks; y++)
            _chunkRenderers[x, y] = new ChunkRenderer(this, world,
                new Rectangle(x * world.ChunkSize, 
                    y * world.ChunkSize, 
                    world.ChunkSize - Math.Max(0, (x + 1) * world.ChunkSize - world.Size.X), 
                    world.ChunkSize - Math.Max(0, (y + 1) * world.ChunkSize - world.Size.Y)));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(Scale));
        foreach (ChunkRenderer chunkRenderer in _chunkRenderers)
        {
            spriteBatch.Draw(chunkRenderer.ChunkTexture, chunkRenderer.Bounds, Color.White);
        }
        spriteBatch.End();
    }

    public void Update()
    {
        foreach (ChunkRenderer chunkRenderer in _chunkRenderers)
            chunkRenderer.ProcessChunk();
    }



    private struct ChunkRenderer
    {
        public readonly Color[] ColorData;
        public bool Sleeping = false;
        private readonly Texture2D _texture;
        private readonly World _world;
        private Rectangle _bounds;

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
            if (Sleeping && isProcessSleeping)
                return;

            Sleeping = isProcessSleeping;
            
            
            for (int x = 0; x < _bounds.Width; x++)
            for (int y = 0; y < _bounds.Height; y++)
            {
                Point global = _bounds.Location + new Point(x, y);

                Color color = _world.GetComponentOrDefault<DotType>(global, Components.DotType) switch
                {
                    DotType.Sand => Color.Yellow,
                    _ => Color.Black,
                };
                
                if (x == 0 || y == 0 || x == _bounds.Width - 1 || y == _bounds.Height - 1)
                    color = Color.Green;
                
                ColorData[PointToIndex(x, y)] = color;
            }
        }
        
        public Texture2D ChunkTexture
        {
            get
            {
                _texture.SetData(ColorData);
                return _texture;
            }
        }
    }
}
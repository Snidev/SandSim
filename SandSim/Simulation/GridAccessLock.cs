using System.Numerics;
using Microsoft.Xna.Framework;

namespace SandSim.Simulation;

public class GridAccessLock(Point size)
{
    private readonly Vector<int> _zero = new(0);
    private int[] _bitArray = new int[(size.X * size.Y + 31) / 32];

    private int GetLockState(Point point)
    {
        int idx = point.Y * size.X + point.X;
        int word = idx / 32;
        int mask = 1 << (idx % 32);

        return _bitArray[word] & mask;
    }
    
    public bool IsLocked(Point point) => GetLockState(point) != 0;

    public bool TryLock(Point point)
    {
        int idx = point.Y * size.X + point.X;
        int word = idx / 32;
        int mask = 1 << (idx % 32);
        
        int original = _bitArray[word];
        
        if ((original & mask) != 0)
            return false;
        
        int newVal = Interlocked.CompareExchange(ref _bitArray[word], original | mask, original);
        
        return newVal == original;
    }

    public void Unlock(Point point)
    {
        int idx = point.Y * size.X + point.X;
        int word = idx / 32;
        int mask = 1 << (idx % 32);
        
        Interlocked.And(ref _bitArray[word], ~mask);
    }

    public void Clear()
    {
        int length = _bitArray.Length;
        int i = 0;

        for (; i + Vector<int>.Count <= length; i += Vector<int>.Count)
        {
            _zero.CopyTo(_bitArray, i);
        }

        for (; i < length; i++)
        {
            _bitArray[i] = 0;
        }
    }
}
namespace SandSim.Data;

public interface IComponentStore
{
    public void Add(int entity);
    public void Remove(int entity);
    public bool Contains(int entity);
}

public class ComponentStore<T>(int initialMax, T? @default = default) : IComponentStore
{
    private int[] _sparse = new int[initialMax];
    private int[] _dense = new int[initialMax];
    private T?[] _components = new T?[initialMax];
    private int _count = 0;
    
    public int Count => _count;
    public T? Default => @default;

    public bool Contains(int value)
    {
        if (value >= _sparse.Length || _sparse[value] == -1 || _sparse[value] >= _count)
            return false;

        return _dense[_sparse[value]] == value;
    }

    public T? GetComponentOrDefault(int entity)
    {
        if (!Contains(entity))
            return @default;
        
        return _components[_sparse[entity]];
    }

    public void SetComponent(int entity, T? component)
    {
        if (!Contains(entity))
            return;
        
        _components[_sparse[entity]] = component;
    }

    public void Add(int value)
    {
        if (Contains(value))
            return;

        ArrayHelper.Expand(ref _sparse, value);
        ArrayHelper.Expand(ref _dense, _count + 1);
        ArrayHelper.Expand(ref _components, _count + 1);
        
        _sparse[value] = _count;
        _dense[_count] = value;
        _components[_count] = default;
        _count++;
    }

    public void Remove(int value)
    {
        if (!Contains(value))
            return;

        int idx = _sparse[value];
        int last = _dense[_count - 1];
        
        _dense[idx] = last;
        _components[idx] = _components[_count - 1];
        _sparse[last] = idx;
        _sparse[value] = -1;

        _count--;
    }
}

// Sparse set without component storage
public struct SparseSet(int initialMax)
{
    private int[] _sparse = new int[initialMax];
    private int[] _dense = new int[initialMax];
    private int _count = 0;
    
    public int Count => _count;

    public bool Contains(int value)
    {
        if (value < 0 || value >= _sparse.Length || _sparse[value] == -1 || _sparse[value] >= _count)
            return false;

        return _dense[_sparse[value]] == value;
    }
    
    public void Add(int value)
    {
        if (Contains(value))
            return;

        ArrayHelper.Expand(ref _sparse, value);
        ArrayHelper.Expand(ref _dense, _count + 1);
        
        _sparse[value] = _count;
        _dense[_count] = value;
        _count++;
    }

    public void Remove(int value)
    {
        if (!Contains(value))
            return;

        int idx = _sparse[value];
        int last = _dense[_count - 1];
        
        _dense[idx] = last;
        _sparse[last] = idx;
        _sparse[value] = -1;

        _count--;
    }
}
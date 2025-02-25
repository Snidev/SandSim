namespace SandSim.Data;

public abstract class EntityManager
{
    private const int BaseCount = 512;
    private SparseSet _entities = new(BaseCount);
    private int[] _generations = new int[BaseCount];
    private readonly Stack<int> _freeIndices = new();

    protected abstract IComponentStore[] ComponentStore { get; }

    public Entity AllocateEntity()
    {
        int nextFreeIndex = _freeIndices.Count > 0 ? _freeIndices.Pop() : _entities.Count;
        int generation = GetCurrentGeneration(nextFreeIndex);

        _entities.Add(nextFreeIndex);
        _generations[nextFreeIndex] = ++generation;

        return new Entity(nextFreeIndex, generation);
    }

    public Entity GetEntity(int entityId) =>
        _entities.Contains(entityId) ? new Entity(entityId, _generations[entityId]) : new Entity(-1, -1);

    public bool FreeEntity(Entity entity)
    {
        // If the provided entity does not match the generation of the current instance the two objects are not equal
        // If an id that is not allocated is provided GetEntity will return Entity.Null
        Entity validInstance = GetEntity(entity.Id);
        if (validInstance == new Entity(-1, -1) || entity != validInstance)
            return false;

        _entities.Remove(entity.Id);
        _freeIndices.Push(entity.Id);

        foreach (IComponentStore componentStore in ComponentStore)
        {
            componentStore.Remove(entity.Id);
        }
        return true;
    }

    protected bool GetComponentOrDefault<T>(Entity ent, int componentIndex, out T? component)
    {
        ComponentStore<T> componentStore = (ComponentStore[componentIndex] as ComponentStore<T>)!;
        Entity validInstance = GetEntity(ent.Id);

        if (validInstance == new Entity(-1, -1) || ent != validInstance)
        {
            component = componentStore.Default;
            return false;
        }
        
        return componentStore.GetComponentOrDefault(ent.Id, out component);
    }

    protected void SetComponent<T>(Entity ent, int componentIndex, T? value)
    {
        ComponentStore<T> componentStore = (ComponentStore[componentIndex] as ComponentStore<T>)!;
        Entity validInstance = GetEntity(ent.Id);

        if (validInstance == new Entity(-1, -1) || ent != validInstance)
            return;

        componentStore.SetComponent(ent.Id, value);
    }

    public bool HasComponent(Entity ent, int componentIndex)
    {

        IComponentStore componentStore = ComponentStore[componentIndex];
        Entity validInstance = GetEntity(ent.Id);

        if (validInstance == new Entity(-1, -1) || ent != validInstance)
            return false;

        return componentStore.Contains(ent.Id);
    }

    public void AllocateComponent<T>(Entity ent, int componentIndex, T? value)
    {
        IComponentStore componentStore = ComponentStore[componentIndex];
        AllocateComponent(ent, componentIndex);
        (componentStore as ComponentStore<T>).SetComponent(ent.Id, value);
    }

    public void AllocateComponent(Entity ent, int componentIndex)
    {
        IComponentStore componentStore = ComponentStore[componentIndex];

        Entity validInstance = GetEntity(ent.Id);
        if (validInstance == new Entity(-1, -1) || ent != validInstance)
            return;

        componentStore.Add(ent.Id);
    }

    public void FreeComponent<T>(Entity ent, int componentIndex, T value)
    {
        IComponentStore componentStore = ComponentStore[componentIndex];

        Entity validInstance = GetEntity(ent.Id);
        if (validInstance == new Entity(-1, -1) || ent != validInstance)
            return;

        componentStore.Remove(ent.Id);
    }

    private int GetCurrentGeneration(int index)
    {
        ArrayHelper.Expand(ref _generations, index);
        return _generations[index];
    }
}
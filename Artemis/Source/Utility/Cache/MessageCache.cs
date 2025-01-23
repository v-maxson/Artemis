using LazyCache;
using NetCord.Gateway;

namespace Cache;

public class MessageCache()
{
    private static readonly TimeSpan Expiration = new(1, 0, 0);

    public readonly IAppCache Cache = new CachingService();

    public void Add(Message value)
    {
        Cache.Add($"{typeof(MessageCache)}-{value.Id}", value, Expiration);
    }

    public void Update(Message value)
    {
        Cache.Remove($"{typeof(MessageCache)}-{value.Id}");
        Cache.Add($"{typeof(MessageCache)}-{value.Id}", value, Expiration);
    }

    public bool TryGetValue(ulong id, out Message value)
    {
        return Cache.TryGetValue($"{typeof(MessageCache)}-{id}", out value);
    }

    public void Remove(ulong id)
    {
        Cache.Remove($"{typeof(MessageCache)}-{id}");
    }
}

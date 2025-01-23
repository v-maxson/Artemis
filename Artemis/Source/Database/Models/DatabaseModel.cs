using System.Linq.Expressions;
using LiteDB;

namespace Database.Models;

internal abstract class DatabaseModel<T>
    where T : DatabaseModel<T>
{
    [BsonId]
    public required ulong Id { get; set; }
    public ulong? SecondaryId { get; set; }

    public static T? Get(Expression<Func<T, bool>> predicate)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);

        return GetInCollection(predicate, collection);
    }

    public static T? Get(ulong id, ulong? secondaryId = null)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);

        return GetInCollection(id, collection, secondaryId);
    }

    public static T GetOrCreate(Expression<Func<T, bool>> predicate)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);

        return GetOrCreateInCollection(predicate, collection);
    }

    public static T GetOrCreate(ulong id, ulong? secondaryId = null)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);

        return GetOrCreateInCollection(id, collection, secondaryId);
    }

    public static BsonValue Create(T value)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);

        return collection.Insert(value);
    }

    public static T Update(ulong id, Action <T> action)
    {
        return Update(id, null, action);
    }

    public static T Update(ulong id, ulong? secondaryId, Action <T> action)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);
        var entity = GetOrCreateInCollection(id, collection, secondaryId);
        action(entity);
        collection.Update(entity);
        return entity;
    }

    public static T Update(Expression<Func<T, bool>> predicate, Action<T> action)
    {
        using var db = Database.Connect();
        var collection = GetCollection(db);
        var entity = GetOrCreateInCollection(predicate, collection);
        action(entity);
        collection.Update(entity);
        return entity;
    }

    public static ILiteCollection<T> GetCollection(LiteDatabase db)
    {
        return db.GetCollection<T>(typeof(T).Name);
    }

    public static T? GetInCollection(Expression<Func<T, bool>> predicate, ILiteCollection<T> collection)
    {
        return collection.FindOne(predicate);
    }

    public static T? GetInCollection(ulong id, ILiteCollection<T> collection, ulong? secondaryId = null)
    {
        if (secondaryId.HasValue)
        {
            return collection.FindOne(x => x.Id == id && x.SecondaryId == secondaryId);
        }
        else
        {
            return collection.FindOne(x => x.Id == id);
        }
    }

    public static T GetOrCreateInCollection(Expression<Func<T, bool>> predicate, ILiteCollection<T> collection)
    {
        var entity = GetInCollection(predicate, collection);
        if (entity == null)
        {
            entity = Activator.CreateInstance<T>();
            collection.Insert(entity);
            return entity;
        }
        else return entity;
    }

    public static T GetOrCreateInCollection(ulong id, ILiteCollection<T> collection, ulong? secondaryId = null)
    {
        var entity = GetInCollection(id, collection, secondaryId);

        if (entity == null)
        {
            entity = Activator.CreateInstance<T>();
            entity.Id = id;
            entity.SecondaryId = secondaryId;
            collection.Insert(entity);
            return entity;
        }
        else return entity;
    }
}

using System.Linq.Expressions;
using LiteDB;

namespace DB.Models;

internal abstract class DatabaseModel<T>
    where T : DatabaseModel<T>
{
    private static ILiteCollection<T> Collection => Database.Connection.GetCollection<T>(typeof(T).Name);

    [BsonId]
    public required ulong Id { get; set; }
    public ulong? SecondaryId { get; set; }

    public static BsonValue Create(T value) {
        var id = Collection.Insert(value);
        Database.Commit();
        return id;
    }

    public static T Upsert(ulong id, Action<T> action) {
        return Upsert(id, null, action);
    }

    public static T Upsert(ulong id, ulong? secondaryId, Action<T> action) {
        var entity = GetOrCreate(id, secondaryId);
        action(entity);
        Collection.Update(entity);
        Database.Commit();
        return entity;
    }

    public static T Upsert(T value) {
        Collection.Upsert(value);
        Database.Commit();
        return value;
    }

    public static bool TryGet(Expression<Func<T, bool>> predicate, out T value) {
        var search = Collection.FindOne(predicate);
        if (search != null) {
            value = search;
            return true;
        }
        else {
            value = null!;
            return false;
        }
    }

    public static bool TryGet(ulong id, out T value) {
        T? search = Collection.FindOne(x => x.Id == id);
        if (search != null) {
            value = search;
            return true;
        }
        else {
            value = null!;
            return false;
        }
    }

    public static bool TryGet(ulong id, ulong? secondaryId, out T value) {
        T? search = Collection.FindOne(x => x.Id == id && x.SecondaryId == secondaryId);

        if (search != null) {
            value = search;
            return true;
        }
        else {
            value = null!;
            return false;
        }
    }

    public static T GetOrCreate(ulong id, ulong? secondaryId = null) {
        TryGet(id, secondaryId, out var entity);

        if (entity == null) {
            entity = Activator.CreateInstance<T>();
            entity.Id = id;
            entity.SecondaryId = secondaryId;
            Collection.Insert(entity);
            Database.Commit();
            return entity;
        }
        else return entity;
    }
}

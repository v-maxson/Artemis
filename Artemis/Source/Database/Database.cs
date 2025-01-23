using LiteDB;

namespace Database;

internal static class Database
{
    public const string DB_CONNECTION_STRING = "Filename=database.db;Connection=shared;";

    public static LiteDatabase Connect()
    {
        return new LiteDatabase(DB_CONNECTION_STRING);
    }
}

using LiteDB;

namespace Database;

internal static class Database
{
    public const string DB_FILE_NAME = "database";
    public const string DB_FILE_EXT = ".db";
    public const string DB_FILE = $"{DB_FILE_NAME}{DB_FILE_EXT}";
    public const string DB_CONNECTION_STRING = $"Filename={DB_FILE};Connection=shared;";

    public static LiteDatabase Connect()
    {
        return new LiteDatabase(DB_CONNECTION_STRING);
    }

    public static void CreateBackup()
    {
        // If "backups" folder doesn't exist, create it.
        if (!Directory.Exists("backups"))
            Directory.CreateDirectory("backups");

        var backupFilePath = $"./backups/{DB_FILE_NAME}.{DateTime.Now:yyyy-MM-dd.h-mm-ss}{DB_FILE_EXT}";
        File.Copy(DB_FILE, backupFilePath);
    }
}

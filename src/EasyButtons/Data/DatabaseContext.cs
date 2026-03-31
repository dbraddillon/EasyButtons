using SQLite;
using EasyButtons.Models;

namespace EasyButtons.Data;

public class DatabaseContext
{
    private const string DbFileName = "easybuttons.db3";
    private SQLiteAsyncConnection? _connection;

    public string DbPath => Path.Combine(FileSystem.AppDataDirectory, DbFileName);

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null) return _connection;
        _connection = new SQLiteAsyncConnection(DbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _connection.CreateTableAsync<EasyButton>();
        // Clear stale sound sentinel values left over from the old click/silent sound mode system.
        // These are not valid file paths — real sound buttons have an absolute path that File.Exists() confirms.
        await _connection.ExecuteAsync(
            "UPDATE Buttons SET SoundPath = NULL WHERE SoundPath IN ('click', 'silent')");
        return _connection;
    }
}

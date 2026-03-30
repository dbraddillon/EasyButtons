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
        return _connection;
    }
}

using EasyButtons.Data;
using EasyButtons.Models;

namespace EasyButtons.Repositories;

public class EasyButtonRepository(DatabaseContext db)
{
    public async Task<List<EasyButton>> GetAllAsync()
    {
        var conn = await db.GetConnectionAsync();
        return await conn.Table<EasyButton>().OrderBy(b => b.SortOrder).ToListAsync();
    }

    public async Task<EasyButton?> GetByIdAsync(Guid id)
    {
        var conn = await db.GetConnectionAsync();
        return await conn.FindAsync<EasyButton>(id);
    }

    public async Task SaveAsync(EasyButton button)
    {
        var conn = await db.GetConnectionAsync();
        var existing = await conn.FindAsync<EasyButton>(button.Id);
        if (existing is null) await conn.InsertAsync(button);
        else await conn.UpdateAsync(button);
    }

    public async Task DeleteAsync(Guid id)
    {
        var conn = await db.GetConnectionAsync();
        await conn.DeleteAsync<EasyButton>(id);
    }
}

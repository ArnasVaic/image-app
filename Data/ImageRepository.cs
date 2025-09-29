using Dapper;
using image_app.Models;
using Npgsql;

namespace image_app.Data;

public class ImageRepository(IConfiguration config)
{
    private NpgsqlConnection GetConnection()
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        return new NpgsqlConnection(connectionString);
    }

    public async Task<IEnumerable<Image>> GetMetadataAsync()
    {
        using var connection = GetConnection();
        return await connection.QueryAsync<Image>(
            "SELECT \"id\", \"name\", \"createdAt\" FROM public.\"Images\""
        );
    }

    public async Task<Image> GetByIdAsync(Guid id)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstAsync<Image>(
            $"SELECT \"id\", \"name\", \"createdAt\" FROM public.\"Images\" WHERE \"id\"=@Id",
            new { id }
        );
    }

    public async Task<int> UpdateAsync(Image image)
    {
        using var connection = GetConnection();
        int rowsAffected = await connection.ExecuteAsync(
            $"UPDATE public.\"Images\" SET \"name\"=@Name, \"data\"=@Data WHERE id=@Id",
            image
        );
        return rowsAffected;
    }

    public async Task<Guid> AddAsync(string name, byte[] data)
    {
        using var connection = GetConnection();

        // Insert and return the generated ID
        var id = await connection.ExecuteScalarAsync<Guid>(
            @"INSERT INTO public.""Images"" (""name"", ""data"", ""createdAt"")
          VALUES (@Name, @Data, @CreatedAt)
          RETURNING ""id"";",
            new { name, data, CreatedAt = DateTime.Now }
        );

        return id;
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        using var connection = GetConnection();
        int rowsAffected = await connection.ExecuteAsync(
            $"DELETE FROM public.\"Images\" WHERE \"id\"=@Id",
            new { id }
        );
        return rowsAffected;
    }
}

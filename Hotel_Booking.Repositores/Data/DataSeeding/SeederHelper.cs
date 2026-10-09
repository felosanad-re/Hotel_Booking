using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hotel_Booking.Repositories.Data.DataSeeding
{
    public static class SeederHelper
    {
        public static async Task SeederFromJSONAsync<T>(DbContext dbContext, string fileName) where T : class
        {
            if (await dbContext.Set<T>().AnyAsync())
                return;

            var path = Path.Combine(AppContext.BaseDirectory, "Helper", "DataSeeding", fileName);
            if (!File.Exists(path))
                return;

            var data = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var items = JsonSerializer.Deserialize<List<T>>(data, options);
            if (items?.Any() == true)
            {
                await dbContext.Set<T>().AddRangeAsync(items);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}

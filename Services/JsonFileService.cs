using System.Text.Json;

namespace LibraryApp.Services
{
    public class JsonFileService<T>
    {
        private readonly string _filePath;
        public JsonFileService(string filePath)
        {
            _filePath = filePath;
        }

        public List<T> ReadAll()
        {
            if (!File.Exists(_filePath))
                return new List<T>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        public void WriteAll(List<T> items)
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}

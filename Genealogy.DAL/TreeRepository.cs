using System.Text.Json;

namespace Genealogy.DAL
{
    // класс для работы с хранилищем
    public class TreeRepository
    {
        private readonly string _filePath = "genealogy_tree.json";
        private List<Person> _tree = new List<Person>();

        // загрузить древо из JSON-файла в список
        public List<Person> LoadTree()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    _tree = JsonSerializer.Deserialize<List<Person>>(json) ?? new List<Person>();
                }
                catch (JsonException)
                {
                    Console.WriteLine("Ошибка обработки файла. Работа начнется с пустого древа.");
                    _tree = new List<Person>();
                }
            }
            return _tree;
        }

        // сохранить древо из списка в JSON-файла
        public void SaveTree(List<Person> tree)
        {
            var json = JsonSerializer.Serialize(tree, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}

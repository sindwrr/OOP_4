using Genealogy.BLL;
using System.Text.RegularExpressions;

Console.WriteLine("=== ИМИТАТОР СЕМЕЙНОГО ДРЕВА (вер. 0.1) ===\n");

var treeManager = new TreeManager();
while (true)
{
    Console.WriteLine(
        "1. Внести человека\n" +
        "2. Создать брак\n" +
        "3. Создать отношение родитель-ребенок\n" +
        "4. Вывести родственников человека\n" +
        "5. Вычислить возраст предка при рождении потомка\n" +
        "6. Вывести общих предков двух людей\n" +
        "7. Показать древо\n" +
        "8. Очистить древо\n" +
        "9. Выход");

    var choice = Console.ReadLine();
    int personId;
    switch (choice)
    {
        case "1":
            Console.Write("ФИО: ");
            string name;
            while (true)
            {
                name = Console.ReadLine();
                var regex = new Regex(@"^[А-Я][а-я]+ [А-Я][а-я]+ [А-Я][а-я]+$");
                if (regex.IsMatch(name)) break;
                else Console.WriteLine("Неверный формат имени. Попробуйте еще раз.");
            }

            Console.Write("Дата рождения (ГГГГ-ММ-ДД): ");
            DateTime dob;
            while (true)
            {
                try
                {
                    dob = DateTime.Parse(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("Неверный формат даты. Попробуйте еще раз.");
                }
            }

            Console.Write("Пол (м/ж или m/f): ");
            bool gender;
            while (true)
            {
                var genderStr = Console.ReadLine();
                if (genderStr?.Equals("м", StringComparison.OrdinalIgnoreCase) == true ||
                    genderStr?.Equals("m", StringComparison.OrdinalIgnoreCase) == true)
                {
                    gender = true;
                    break;
                }
                else if (genderStr?.Equals("ж", StringComparison.OrdinalIgnoreCase) == true ||
                         genderStr?.Equals("f", StringComparison.OrdinalIgnoreCase) == true)
                {
                    gender = false;
                    break;
                }
                else Console.WriteLine("Неверный формат пола. Попробуйте еще раз.");
            }
            var person = treeManager.CreatePerson(name, dob, gender);
            Console.WriteLine($"Человек успешно добавлен! id = {person.Id}");
            break;
        case "2":
            Console.Write("ID первого человека: ");
            personId = int.Parse(Console.ReadLine());
            Console.Write("ID второго человека: ");
            var spouseId = int.Parse(Console.ReadLine());
            if (treeManager.AddSpouse(personId, spouseId))
                Console.WriteLine("Брак успешно создан!");
            break;
        case "3":
            Console.Write("ID родителя: ");
            var parentId = int.Parse(Console.ReadLine());
            Console.Write("ID ребенка: ");
            var childId = int.Parse(Console.ReadLine());
            if (treeManager.AddParentChildRelationship(parentId, childId))
                Console.WriteLine("Отношение родитель-ребенок успешно создано!");
            break;
        case "4":
            Console.Write("ID человека: ");
            personId = int.Parse(Console.ReadLine());
            treeManager.DisplayRelatives(personId);
            break;
        case "5":
            Console.Write("ID предка: ");
            var ancestorId = int.Parse(Console.ReadLine());
            Console.Write("ID потомка: ");
            var descendantId = int.Parse(Console.ReadLine());
            var age = treeManager.FindAncestorAgeAtDescendantBirth(ancestorId, descendantId);
            if (age > -1) Console.WriteLine($"Возраст предка (в годах): {age}");
            break;
        case "6":
            Console.Write("ID первого человека: ");
            var firstId = int.Parse(Console.ReadLine());
            Console.Write("ID второго человека: ");
            var secondId = int.Parse(Console.ReadLine());
            var commonAncestors = treeManager.GetCommonAncestors(firstId, secondId);

            if (commonAncestors.Any())
            {
                Console.WriteLine("Общие предки:");
                foreach (var ancestor in commonAncestors)
                {
                    Console.WriteLine($"\t{ancestor.Name} (id = {ancestor.Id})");
                }
            }
            else Console.WriteLine("Общих предков не найдено.");
            break;
        case "7":
            Console.WriteLine();
            treeManager.DisplayTree();
            Console.WriteLine();
            break;
        case "8":
            treeManager.ClearTree();
            break;
        case "9":
            return;
        default:
            Console.WriteLine("Неверный выбор, попробуйте еще раз.");
            break;
    }
}

using Genealogy.DAL;
using Spectre.Console;

namespace Genealogy.BLL
{
    // класс для управления древом
    public class TreeManager
    {
        private List<Person> _tree;
        private readonly TreeRepository _repository;
        private int _nextId;

        public TreeManager()
        {
            _repository = new TreeRepository();
            _tree = _repository.LoadTree();
            _nextId = _tree.Any() ? _tree.Max(p => p.Id) + 1 : 1;
        }

        // добавить человека в древо
        public Person CreatePerson(string fullName, DateTime dateOfBirth, bool gender)
        {
            var person = new Person { Id = _nextId++, Name = fullName, DateOfBirth = dateOfBirth, Gender = gender };
            _tree.Add(person);
            _repository.SaveTree(_tree);
            return person;
        }

        // создать брак
        public bool AddSpouse(int personId, int spouseId)
        {
            var person = _tree.FirstOrDefault(p => p.Id == personId);
            var spouse = _tree.FirstOrDefault(p => p.Id == spouseId);
            if (person != null && spouse != null)
            {
                if (spouseId == personId)
                {
                    Console.WriteLine("Человек не может быть в браке с собой.");
                    return false;
                }
                if (person.Spouse == spouseId || spouse.Spouse == personId)
                {
                    Console.WriteLine("Эти люди уже являются супругами.");
                    return false;
                }
                if (person.Spouse.HasValue && person.Spouse != spouseId || spouse.Spouse.HasValue && spouse.Spouse != personId)
                {
                    Console.WriteLine("Один из этих людей уже состоит в браке с другим.");
                    return false;
                }
                if (person.Gender == spouse.Gender)
                {
                    Console.WriteLine("Люди одного пола не могут быть супругами.");
                    return false;
                }
                if (person.Spouse != spouseId) person.Spouse = spouseId;
                if (spouse.Spouse != personId) spouse.Spouse = personId;
                if (person.Children.Any())
                {
                    foreach (var childId in person.Children)
                    {
                        if (!AddParentChildRelationship(spouseId, childId)) return false;
                    }
                }
                else if (spouse.Children.Any())
                {
                    foreach (var childId in spouse.Children)
                    {
                        if (!AddParentChildRelationship(personId, childId)) return false;
                    }
                }

                _repository.SaveTree(_tree);
                return true;
            }
            Console.WriteLine("По крайней мере одного из этих людей нет в древе.");
            return false;
        }

        // добавить отношение родитель-ребенок
        public bool AddParentChildRelationship(int parentId, int childId)
        {
            var parent = _tree.FirstOrDefault(p => p.Id == parentId);
            var child = _tree.FirstOrDefault(p => p.Id == childId);

            if (child.Parents.Count == 2)
            {
                Console.WriteLine($"У человека с id = {childId} уже есть два родителя.");
                return false;
            }
            if (parent != null && child != null)
            {
                if (parentId == childId)
                {
                    Console.WriteLine("Человек не может быть ребенком самого себя.");
                    return false;
                }
                if (parent.DateOfBirth >= child.DateOfBirth)
                {
                    Console.WriteLine("Дата рождения родителя не может быть позже даты рождения ребенка.");
                    return false;
                }

                if (!parent.Children.Contains(childId)) parent.Children.Add(childId);
                if (!child.Parents.Contains(parentId)) child.Parents.Add(parentId);
                if (parent.Spouse.HasValue)
                {
                    var spouse = _tree.FirstOrDefault(p => p.Id == parent.Spouse);
                    if (!child.Parents.Contains((int)parent.Spouse)) child.Parents.Add((int)parent.Spouse);
                    if (!spouse.Children.Contains(childId)) spouse.Children.Add(childId);
                }
                _repository.SaveTree(_tree);
                return true;
            }
            Console.WriteLine("По крайней мере одного из этих людей нет в древе.");
            return false;
        }

        // вывести ближайших родственников человека (родителей, супруга и детей)
        public void DisplayRelatives(int personId)
        {
            var person = _tree.FirstOrDefault(p => p.Id == personId);
            if (person == null)
            {
                Console.WriteLine("Человека нет в древе.");
                return;
            }
            Console.WriteLine("Родители: ");
            if (person.Parents.Any())
            {
                foreach (var parentId in person.Parents)
                {
                    var parent = _tree.FirstOrDefault(p => p.Id == parentId);
                    Console.WriteLine($"\t{parent.Name} (id = {parent.Id})");
                }
            }
            else Console.WriteLine("\tНе определены.");

            if (person.Gender == true) Console.WriteLine("Жена: ");
            else Console.WriteLine("Муж: ");
            if (person.Spouse.HasValue)
            {
                var spouse = _tree.FirstOrDefault(p => p.Id == person.Spouse);
                    Console.WriteLine($"\t{spouse.Name} (id = {spouse.Id})");
            }
            else
            {
                if (person.Gender == true) Console.WriteLine("\tНе определена.");
                else Console.WriteLine("\tНе определен.");
            }

            Console.WriteLine("Дети: ");
            if (person.Children.Any())
            {
                foreach (var childId in person.Children)
                {
                    var child = _tree.FirstOrDefault(p => p.Id == childId);
                    Console.WriteLine($"\t{child.Name} (id = {child.Id})");
                }
            }
            else Console.WriteLine("\tНе определены.");
        }

        // вычислить возраст предка в день рождения потомка
        public int FindAncestorAgeAtDescendantBirth(int ancestorId, int descendantId)
        {
            var ancestor = _tree.FirstOrDefault(p => p.Id == ancestorId);
            var descendant = _tree.FirstOrDefault(p => p.Id == descendantId);

            if (ancestor == null || descendant == null)
            {
                Console.WriteLine("Предка или потомка нет в древе.");
                return -1;
            }

            if (ancestor.DateOfBirth >= descendant.DateOfBirth)
            {
                Console.WriteLine("Дата рождения предка не может быть позже даты рождения потомка.");
                return -1;
            }

            var ageAtBirth = descendant.DateOfBirth.Year - ancestor.DateOfBirth.Year;

            if (descendant.DateOfBirth < ancestor.DateOfBirth.AddYears(ageAtBirth))
            {
                ageAtBirth--;
            }

            return ageAtBirth;
        }

        // найти общих предков двух людей
        public List<Person> GetCommonAncestors(int personId1, int personId2)
        {
            var person1 = _tree.FirstOrDefault(p => p.Id == personId1);
            var person2 = _tree.FirstOrDefault(p => p.Id == personId2);

            if (person1 == null || person2 == null)
            {
                Console.WriteLine("По крайней мере одного из этих людей нет в древе.");
                return new List<Person>();
            }

            var ancestors1 = GetAllAncestors(person1);
            var ancestors2 = GetAllAncestors(person2);

            var commonAncestors = ancestors1.Intersect(ancestors2).ToList();
            return commonAncestors;
        }

        // вспомогательный метод для получения всех предков человека
        private HashSet<Person> GetAllAncestors(Person person)
        {
            var ancestors = new HashSet<Person>();
            var queue = new Queue<Person>();

            foreach (var parentId in person.Parents)
            {
                var parent = _tree.FirstOrDefault(p => p.Id == parentId);
                if (parent != null) queue.Enqueue(parent);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (ancestors.Add(current))
                {
                    foreach (var parentId in current.Parents)
                    {
                        var parent = _tree.FirstOrDefault(p => p.Id == parentId);
                        if (parent != null) queue.Enqueue(parent);
                    }
                }
            }

            return ancestors;
        }

        // очистить древо
        public void ClearTree()
        {
            _tree.Clear();
            _nextId = 1;
            _repository.SaveTree(_tree);
            Console.WriteLine("Древо успешно очищено!");
        }

        // показать древо
        public void DisplayTree()
        {
            var rootPersons = _tree.Where(p => !p.Parents.Any()).ToList();

            if (!rootPersons.Any())
            {
                Console.WriteLine("Древо пусто.");
                return;
            }

            foreach (var root in rootPersons)
            {
                var rootTree = new Tree($"{root.Name} (id = {root.Id})");
                DisplaySubTree(root, rootTree);
                AnsiConsole.Write(rootTree);
            }
        }

        // вспомогательный метод для вывода поддеревьев
        private void DisplaySubTree(Person person, IHasTreeNodes parentNode)
        {
            if (person.Spouse.HasValue)
            {
                var spouse = _tree.FirstOrDefault(p => p.Id == person.Spouse)?.Name;
                if (!string.IsNullOrEmpty(spouse))
                {
                    if (person.Gender == true) parentNode.AddNode($"Жена: {spouse}");
                    else parentNode.AddNode($"Муж: {spouse}");
                }
            }

            foreach (var childId in person.Children)
            {
                var child = _tree.FirstOrDefault(p => p.Id == childId);
                if (child != null)
                {
                    var childNode = parentNode.AddNode($"{child.Name} (id = {child.Id})");
                    DisplaySubTree(child, childNode);
                }
            }
        }
    }
}

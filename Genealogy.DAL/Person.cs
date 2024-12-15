namespace Genealogy.DAL
{
    // класс для человека
    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public int? Spouse { get; set; }
        public List<int> Parents { get; set; } = new List<int>();
        public List<int> Children { get; set; } = new List<int>();
    }
}

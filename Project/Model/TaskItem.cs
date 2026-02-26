namespace Project.Model
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public bool Completed { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {(Completed ? "[X]" : "[ ]")} {Description}";
        }
    }
}

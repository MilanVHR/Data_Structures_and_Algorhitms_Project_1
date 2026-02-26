// This file defines the TaskItem model, which represents a single to-do task
// in the application. It is part of the Model layer in the Clean Architecture
// structure. The Model layer contains only data structures and no business logic.
//
// A TaskItem stores:
// - A unique ID
// - A description of the task
// - Whether the task is completed
//
// This model is used by:
// - The Repository layer (for saving/loading tasks)
// - The Service layer (for business logic)
// - The View layer (for displaying tasks to the user)

namespace Project.Model
{
    public class TaskItem
    {
        // Unique identifier for the task.
        // Assigned automatically by the TaskService.
        public int Id { get; set; }

        // A short text describing what the task is about.
        public string Description { get; set; } = "";

        // Indicates whether the task is completed.
        // True = done, False = still open.
        public bool Completed { get; set; }

        // Converts the task into a readable string for display in the console UI.
        public override string ToString()
        {
            return $"[{Id}] {(Completed ? "[X]" : "[ ]")} {Description}";
        }
    }
}

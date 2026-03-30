// This file defines the TaskItem model, which represents a single to-do task
// in the application.
//
// A TaskItem stores:
// - A unique ID
// - A description of the task
// - A Kanban status

namespace Project.Model
{
    using System.Text.Json.Serialization;

    public enum TaskStage
    {
        ToDo,
        Doing,
        ToReview,
        Done
    }

    public class TaskItem
    {
        // Unique identifier for the task.
        // Assigned automatically by the TaskService.
        public int Id { get; set; }

        // A short text describing what the task is about.
        public string Description { get; set; } = "";

        // Current task status in the Kanban flow.
        public TaskStage Status { get; set; } = TaskStage.ToDo;

        // Legacy completion flag used only for migration from older JSON data.
        public bool? Completed { get; set; }

        // UTC creation timestamp used for sorting and history views.
        public DateTime CreatedAt { get; set; }

        // Converts the task into a readable string for display in the console UI.
        public override string ToString()
        {
            return $"[{Id}] [{Status}] {Description}";
        }
    }
}

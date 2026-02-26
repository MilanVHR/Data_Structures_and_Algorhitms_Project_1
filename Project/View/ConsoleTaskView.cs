// Responsibilities:
// - Display tasks in a formatted table
// - Show a menu using a selection prompt
// - Ask the user for input
// - Call the TaskService to perform actions

// It is purely presentation logic.

using Spectre.Console;
using Project.Services;
using Project.Model;
using Project.Collections;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private readonly ITaskService _service; // Reference to the business logic layer

        public ConsoleTaskView(ITaskService service)
        {
            _service = service;
        }

        // Displays all tasks in a nice Spectre.Console table.
        private void DisplayTasks()
        {
            Console.Clear();

            // Big ASCII title
            AnsiConsole.Write(
                new FigletText("To-Do List")
                    .Centered()
                    .Color(Color.Cyan1));

            // Create a table with rounded borders
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn("[yellow]ID[/]")
                .AddColumn("[green]Description[/]")
                .AddColumn("[blue]Completed[/]");

            // Fill the table with tasks
            var it = _service.GetAllTasks().GetIterator();
            while (it.HasNext())
            {
                var t = it.Next();
                table.AddRow(
                    t.Id.ToString(),
                    t.Description,
                    t.Completed ? "[green]Yes[/]" : "[red]No[/]");
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        // Main UI loop
        public void Run()
        {
            while (true)
            {
                DisplayTasks();

                // Menu using Spectre.Console selection prompt
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Kies een optie[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices(new[]
                        {
                            "Taak toevoegen",
                            "Taak verwijderen",
                            "Taak togglen (voltooid / niet voltooid)",
                            "Afsluiten"
                        }));

                switch (option)
                {
                    case "Taak toevoegen":
                        AddTask();
                        break;

                    case "Taak verwijderen":
                        RemoveTask();
                        break;

                    case "Taak togglen (voltooid / niet voltooid)":
                        ToggleTask();
                        break;

                    case "Afsluiten":
                        return;
                }
            }
        }

        // Adds a new task by asking the user for a description.
        private void AddTask()
        {
            string desc = AnsiConsole.Ask<string>("[green]Beschrijving van de taak:[/]");
            _service.AddTask(desc);

            AnsiConsole.MarkupLine("[bold green]Taak toegevoegd![/]");
            AnsiConsole.MarkupLine("[grey]Druk op een toets om verder te gaan...[/]");
            Console.ReadKey();
        }

        // Removes a task by ID.
        private void RemoveTask()
        {
            int id = AnsiConsole.Ask<int>("[red]ID van de taak om te verwijderen:[/]");
            _service.RemoveTask(id);

            AnsiConsole.MarkupLine("[bold yellow]Taak verwijderd (indien gevonden).[/]");
            AnsiConsole.MarkupLine("[grey]Druk op een toets om verder te gaan...[/]");
            Console.ReadKey();
        }

        // Toggles the completion state of a task.
        private void ToggleTask()
        {
            int id = AnsiConsole.Ask<int>("[blue]ID van de taak om te togglen:[/]");
            _service.ToggleTaskCompletion(id);

            AnsiConsole.MarkupLine("[bold aqua]Taakstatus aangepast![/]");
            AnsiConsole.MarkupLine("[grey]Druk op een toets om verder te gaan...[/]");
            Console.ReadKey();
        }
    }
}

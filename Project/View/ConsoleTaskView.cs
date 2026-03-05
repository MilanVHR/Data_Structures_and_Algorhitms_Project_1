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
                            "Taak aanpassen",
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

                    case "Taak aanpassen":
                        EditTask();
                        break;
                        
                    case "Afsluiten":
                        return;
                }
            }
        }

        // Adds a new task by asking the user for a description.
        private void AddTask()
        {
            while (true)
            {
                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak toevoegen ===[/]");
                string desc = AnsiConsole.Ask<string>("[green]Beschrijving van de taak:[/]");
                _service.AddTask(desc);

                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak toevoegen ===[/]");

                AnsiConsole.MarkupLine("[bold green]Taak toegevoegd![/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices("Nog een taak toevoegen", "Terug naar menu"));

                if (choice == "Terug naar menu")
                    return;
            }
        }

        // Removes a task by ID.
        private void RemoveTask()
        {
            while (true)
            {
                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak verwijderen ===[/]");
                int id = AnsiConsole.Ask<int>("[red]ID van de taak om te verwijderen:[/]");
                _service.RemoveTask(id);

                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak verwijderen ===[/]");

                AnsiConsole.MarkupLine("[bold yellow]Taak verwijderd (indien gevonden).[/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices("Nog een taak verwijderen", "Terug naar menu"));

                if (choice == "Terug naar menu")
                    return;
            }
        }

        // Toggles the completion state of a task.
        private void ToggleTask()
        {
            while (true)
            {
                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak togglen ===[/]");
                int id = AnsiConsole.Ask<int>("[blue]ID van de taak om te togglen:[/]");
                _service.ToggleTaskCompletion(id);

                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak togglen ===[/]");

                AnsiConsole.MarkupLine("[bold aqua]Taakstatus aangepast![/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices("Nog een taak togglen", "Terug naar menu"));

                if (choice == "Terug naar menu")
                    return;
            }
        }

        // Edits a task by ID.
        private void EditTask()
        {
            while (true)
            {
                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak aanpassen ===[/]");
                int id = AnsiConsole.Ask<int>("[yellow]ID van de taak om aan te passen:[/]");
                string newDesc = AnsiConsole.Ask<string>("[green]Nieuwe beschrijving:[/]");

                _service.UpdateTaskDescription(id, newDesc);

                DisplayTasks();
                AnsiConsole.MarkupLine("[bold cyan]=== Taak aanpassen ===[/]");

                AnsiConsole.MarkupLine("[bold green]Taak aangepast.[/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices("Nog een taak aanpassen", "Terug naar menu"));

                if (choice == "Terug naar menu")
                    return;
            }
        }
    }
}

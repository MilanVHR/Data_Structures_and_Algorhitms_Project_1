// Responsibilities:
// - Display tasks in a formatted table
// - Show a menu using a selection prompt
// - Ask the user for input
// - Call the TaskService to perform actions

using Spectre.Console;
using Project.Services;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private readonly ITaskService _service;

        public ConsoleTaskView(ITaskService service)
        {
            _service = service;
        }

        // Renders the task table at the top of the console.
        // Optionally shows the current section title and highlights one selected task.
        private void DisplayTasks(string? sectionTitle = null, int? selectedTaskId = null)
        {
            Console.Clear();

            AnsiConsole.Write(
                new FigletText("To-Do List")
                    .Centered()
                    .Color(Color.Cyan1));

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn("[yellow]ID[/]")
                .AddColumn("[green]Description[/]")
                .AddColumn("[blue]Completed[/]");

            var it = _service.GetAllTasks().GetIterator();
            bool hasTasks = false;
            while (it.HasNext())
            {
                hasTasks = true;
                var task = it.Next();
                bool isSelected = selectedTaskId.HasValue && task.Id == selectedTaskId.Value;

                if (isSelected)
                {
                    table.AddRow(
                        $"[black on yellow]{task.Id}[/]",
                        $"[black on yellow]{task.Description}[/]",
                        task.Completed ? "[black on yellow]Yes[/]" : "[black on yellow]No[/]");
                }
                else
                {
                    table.AddRow(
                        task.Id.ToString(),
                        task.Description,
                        task.Completed ? "[green]Yes[/]" : "[red]No[/]");
                }
            }

            if (!hasTasks)
            {
                table.AddRow("-", "[grey]Nog geen taken[/]", "-");
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            if (!string.IsNullOrWhiteSpace(sectionTitle))
                AnsiConsole.MarkupLine($"[bold cyan]=== {sectionTitle} ===[/]");
        }

        public void Run()
        {
            while (true)
            {
                DisplayTasks();

                // Main action menu.
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

        // Add flow: show list, ask for description, create task, show confirmation.
        private void AddTask()
        {
            while (true)
            {
                DisplayTasks("Taak toevoegen");
                string desc = AnsiConsole.Ask<string>("[green]Beschrijving van de taak:[/]");
                _service.AddTask(desc);

                DisplayTasks("Taak toevoegen");
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

        // Remove flow: ask for ID, highlight selected task, ask confirmation,
        // then remove and show explicit found/not-found feedback.
        private void RemoveTask()
        {
            while (true)
            {
                DisplayTasks("Taak verwijderen");
                int id = AnsiConsole.Ask<int>("[red]ID van de taak om te verwijderen:[/]");

                var selectedTask = _service.GetTaskById(id);
                if (selectedTask == null)
                {
                    DisplayTasks("Taak verwijderen");
                    AnsiConsole.MarkupLine($"[bold red]Taak met ID {id} bestaat niet.[/]");
                }
                else
                {
                    DisplayTasks("Taak verwijderen", id);
                    AnsiConsole.MarkupLine("[bold yellow]Geselecteerde taak is gemarkeerd.[/]");

                    // Explicit confirmation before deleting the selected task.
                    var confirm = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]Wil je deze taak verwijderen?[/]")
                            .HighlightStyle(new Style(Color.Cyan1))
                            .AddChoices("Ja", "Nee"));

                    if (confirm == "Ja")
                    {
                        bool removed = _service.RemoveTask(id);
                        DisplayTasks("Taak verwijderen");
                        AnsiConsole.MarkupLine(
                            removed
                                ? "[bold green]Taak succesvol verwijderd.[/]"
                                : $"[bold red]Taak met ID {id} bestaat niet.[/]");
                    }
                    else
                    {
                        DisplayTasks("Taak verwijderen", id);
                        AnsiConsole.MarkupLine("[bold yellow]Verwijderen geannuleerd.[/]");
                    }
                }

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices("Nog een taak verwijderen", "Terug naar menu"));

                if (choice == "Terug naar menu")
                    return;
            }
        }

        // Toggle flow: ask for ID, highlight selected task, ask confirmation,
        // then toggle completed state and show explicit found/not-found feedback.
        private void ToggleTask()
        {
            while (true)
            {
                DisplayTasks("Taak togglen");
                int id = AnsiConsole.Ask<int>("[blue]ID van de taak om te togglen:[/]");

                var selectedTask = _service.GetTaskById(id);
                if (selectedTask == null)
                {
                    DisplayTasks("Taak togglen");
                    AnsiConsole.MarkupLine($"[bold red]Taak met ID {id} bestaat niet.[/]");
                }
                else
                {
                    DisplayTasks("Taak togglen", id);
                    AnsiConsole.MarkupLine("[bold yellow]Geselecteerde taak is gemarkeerd.[/]");

                    // Explicit confirmation before changing status.
                    var confirm = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]Wil je deze taakstatus wijzigen?[/]")
                            .HighlightStyle(new Style(Color.Cyan1))
                            .AddChoices("Ja", "Nee"));

                    if (confirm == "Ja")
                    {
                        bool toggled = _service.ToggleTaskCompletion(id);
                        DisplayTasks("Taak togglen");
                        AnsiConsole.MarkupLine(
                            toggled
                                ? "[bold green]Taakstatus aangepast![/]"
                                : $"[bold red]Taak met ID {id} bestaat niet.[/]");
                    }
                    else
                    {
                        DisplayTasks("Taak togglen", id);
                        AnsiConsole.MarkupLine("[bold yellow]Wijzigen geannuleerd.[/]");
                    }
                }

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices("Nog een taak togglen", "Terug naar menu"));

                if (choice == "Terug naar menu")
                    return;
            }
        }

        // Edit flow: ask for ID, highlight selected task, ask new description,
        // confirm update, then save and show explicit found/not-found feedback.
        private void EditTask()
        {
            while (true)
            {
                DisplayTasks("Taak aanpassen");
                int id = AnsiConsole.Ask<int>("[yellow]ID van de taak om aan te passen:[/]");

                var selectedTask = _service.GetTaskById(id);
                if (selectedTask == null)
                {
                    DisplayTasks("Taak aanpassen");
                    AnsiConsole.MarkupLine($"[bold red]Taak met ID {id} bestaat niet.[/]");
                }
                else
                {
                    DisplayTasks("Taak aanpassen", id);
                    AnsiConsole.MarkupLine("[bold yellow]Geselecteerde taak is gemarkeerd.[/]");

                    string newDesc = AnsiConsole.Ask<string>("[green]Nieuwe beschrijving:[/]");

                    // Explicit confirmation before saving changes.
                    var confirm = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]Wil je deze wijziging opslaan?[/]")
                            .HighlightStyle(new Style(Color.Cyan1))
                            .AddChoices("Ja", "Nee"));

                    if (confirm == "Ja")
                    {
                        bool updated = _service.UpdateTaskDescription(id, newDesc);
                        DisplayTasks("Taak aanpassen");
                        AnsiConsole.MarkupLine(
                            updated
                                ? "[bold green]Taak aangepast.[/]"
                                : $"[bold red]Taak met ID {id} bestaat niet.[/]");
                    }
                    else
                    {
                        DisplayTasks("Taak aanpassen", id);
                        AnsiConsole.MarkupLine("[bold yellow]Aanpassen geannuleerd.[/]");
                    }
                }

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
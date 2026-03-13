using Spectre.Console;
using Project.Services;
using Project.Model;
using Project.Collections;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private readonly ITaskService _service;
        private TaskSortField _activeSortField = TaskSortField.Id;
        private bool _activeSortAscending = true;

        public ConsoleTaskView(ITaskService service)
        {
            _service = service;
        }

        public void Run()
        {
            while (true)
            {
                DisplayTasks();

                var option = PromptMainMenu();

                switch (option)
                {
                    case "Taak toevoegen":
                        RepeatActionUntilMenu(AddTask, "Nog een taak toevoegen");
                        break;

                    case "Taak verwijderen":
                        RepeatActionUntilMenu(RemoveTask, "Nog een taak verwijderen");
                        break;

                    case "Taak togglen (voltooid / niet voltooid)":
                        RepeatActionUntilMenu(ToggleTask, "Nog een taak togglen");
                        break;

                    case "Taak aanpassen":
                        RepeatActionUntilMenu(EditTask, "Nog een taak aanpassen");
                        break;

                    case "Taken sorteren":
                        RepeatActionUntilMenu(SortTasks, "Sortering opnieuw instellen");
                        break;

                    case "Afsluiten":
                        return;
                }
            }
        }

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
                .AddColumn("[blue]Completed[/]")
                .AddColumn("[magenta]Created (UTC)[/]");

            var it = _service.GetSortedTasks(_activeSortField, _activeSortAscending).GetIterator();
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
                        task.Completed
                            ? "[black on yellow]Yes[/]"
                            : "[black on yellow]No[/]",
                        $"[black on yellow]{FormatCreatedAt(task.CreatedAt)}[/]");
                }
                else
                {
                    table.AddRow(
                        task.Id.ToString(),
                        task.Description,
                        task.Completed ? "[green]Yes[/]" : "[red]No[/]",
                        FormatCreatedAt(task.CreatedAt));
                }
            }

            if (!hasTasks)
            {
                table.AddRow("-", "[grey]Nog geen taken[/]", "-", "-");
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Sortering: {GetSortLabel()}[/]");
            AnsiConsole.WriteLine();

            if (!string.IsNullOrWhiteSpace(sectionTitle))
            {
                AnsiConsole.MarkupLine($"[bold cyan]=== {sectionTitle} ===[/]");
            }
        }

        private string PromptMainMenu()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Kies een optie[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(
                        "Taak toevoegen",
                        "Taak verwijderen",
                        "Taak togglen (voltooid / niet voltooid)",
                        "Taak aanpassen",
                        "Taken sorteren",
                        "Afsluiten"));
        }

        private void SortTasks()
        {
            DisplayTasks("Taken sorteren");

            string fieldChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Sorteer op:[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices("ID", "Beschrijving", "Status", "Creatiedatum"));

            _activeSortField = fieldChoice switch
            {
                "Beschrijving" => TaskSortField.Description,
                "Status" => TaskSortField.Status,
                "Creatiedatum" => TaskSortField.CreatedAt,
                _ => TaskSortField.Id
            };

            string directionChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Volgorde:[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices("Oplopend", "Aflopend"));

            _activeSortAscending = directionChoice == "Oplopend";

            DisplayTasks("Taken sorteren");
            AnsiConsole.MarkupLine($"[bold green]Sortering toegepast: {GetSortLabel()}[/]");
        }

        private void RepeatActionUntilMenu(Action action, string repeatText)
        {
            while (true)
            {
                action();

                var shouldRepeat = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Wat wil je doen?[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices(repeatText, "Terug naar menu"));

                if (shouldRepeat == "Terug naar menu")
                    return;
            }
        }

        private int AskTaskId(string prompt)
        {
            return AnsiConsole.Ask<int>(prompt);
        }

        private bool Confirm(string prompt)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{prompt}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices("Ja", "Nee"));

            return choice == "Ja";
        }

        private string FormatCreatedAt(DateTime createdAt)
        {
            return createdAt == default
                ? "-"
                : createdAt.ToString("dd-MM-yyyy");
        }

        private string GetSortLabel()
        {
            string field = _activeSortField switch
            {
                TaskSortField.Description => "Beschrijving",
                TaskSortField.Status => "Status",
                TaskSortField.CreatedAt => "Creatiedatum",
                _ => "ID"
            };

            string direction = _activeSortAscending ? "oplopend" : "aflopend";
            return $"{field} ({direction})";
        }

        private void AddTask()
        {
            DisplayTasks("Taak toevoegen");

            string desc = AnsiConsole.Ask<string>("[green]Beschrijving van de taak:[/]");
            _service.AddTask(desc);

            DisplayTasks("Taak toevoegen");
            AnsiConsole.MarkupLine("[bold green]Taak toegevoegd![/]");
        }

        private void RemoveTask()
        {
            DisplayTasks("Taak verwijderen");

            int id = AskTaskId("[red]ID van de taak om te verwijderen:[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks("Taak verwijderen");
                AnsiConsole.MarkupLine($"[bold red]Taak met ID {id} bestaat niet.[/]");
                return;
            }

            DisplayTasks("Taak verwijderen", id);
            AnsiConsole.MarkupLine("[bold yellow]Geselecteerde taak is gemarkeerd.[/]");

            if (!Confirm("Wil je deze taak verwijderen?"))
            {
                DisplayTasks("Taak verwijderen", id);
                AnsiConsole.MarkupLine("[bold yellow]Verwijderen geannuleerd.[/]");
                return;
            }

            bool removed = _service.RemoveTask(id);

            DisplayTasks("Taak verwijderen");
            AnsiConsole.MarkupLine(
                removed
                    ? "[bold green]Taak succesvol verwijderd.[/]"
                    : $"[bold red]Taak met ID {id} bestaat niet.[/]");
        }

        private void ToggleTask()
        {
            DisplayTasks("Taak togglen");

            int id = AskTaskId("[blue]ID van de taak om te togglen:[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks("Taak togglen");
                AnsiConsole.MarkupLine($"[bold red]Taak met ID {id} bestaat niet.[/]");
                return;
            }

            DisplayTasks("Taak togglen", id);
            AnsiConsole.MarkupLine("[bold yellow]Geselecteerde taak is gemarkeerd.[/]");

            if (!Confirm("Wil je deze taakstatus wijzigen?"))
            {
                DisplayTasks("Taak togglen", id);
                AnsiConsole.MarkupLine("[bold yellow]Wijzigen geannuleerd.[/]");
                return;
            }

            bool toggled = _service.ToggleTaskCompletion(id);

            DisplayTasks("Taak togglen");
            AnsiConsole.MarkupLine(
                toggled
                    ? "[bold green]Taakstatus aangepast![/]"
                    : $"[bold red]Taak met ID {id} bestaat niet.[/]");
        }

        private void EditTask()
        {
            DisplayTasks("Taak aanpassen");

            int id = AskTaskId("[yellow]ID van de taak om aan te passen:[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks("Taak aanpassen");
                AnsiConsole.MarkupLine($"[bold red]Taak met ID {id} bestaat niet.[/]");
                return;
            }

            DisplayTasks("Taak aanpassen", id);
            AnsiConsole.MarkupLine("[bold yellow]Geselecteerde taak is gemarkeerd.[/]");

            string newDesc = AnsiConsole.Ask<string>("[green]Nieuwe beschrijving:[/]");

            if (!Confirm("Wil je deze wijziging opslaan?"))
            {
                DisplayTasks("Taak aanpassen", id);
                AnsiConsole.MarkupLine("[bold yellow]Aanpassen geannuleerd.[/]");
                return;
            }

            bool updated = _service.UpdateTaskDescription(id, newDesc);

            DisplayTasks("Taak aanpassen");
            AnsiConsole.MarkupLine(
                updated
                    ? "[bold green]Taak aangepast.[/]"
                    : $"[bold red]Taak met ID {id} bestaat niet.[/]");
        }
    }
}
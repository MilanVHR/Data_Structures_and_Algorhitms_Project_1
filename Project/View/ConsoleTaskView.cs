using Spectre.Console;
using Project.Services;
using Project.Model;
using Project.Collections;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private readonly ITaskService _service;

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
                        RepeatAction(
                            action: AddTask,
                            repeatText: "Nog een taak toevoegen");
                        break;

                    case "Taak verwijderen":
                        RepeatAction(
                            action: RemoveTask,
                            repeatText: "Nog een taak verwijderen");
                        break;

                    case "Taak togglen (voltooid / niet voltooid)":
                        RepeatAction(
                            action: ToggleTask,
                            repeatText: "Nog een taak togglen");
                        break;

                    case "Taak aanpassen":
                        RepeatAction(
                            action: EditTask,
                            repeatText: "Nog een taak aanpassen");
                        break;

                    case "Afsluiten":
                        return;
                }
            }
        }

        private void DisplayTasks()
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
                        "Afsluiten"));
        }

        private void RepeatAction(Action action, string repeatText)
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

        private void AddTask()
        {
            string desc = AnsiConsole.Ask<string>("[green]Beschrijving van de taak:[/]");
            _service.AddTask(desc);
            AnsiConsole.MarkupLine("[bold green]Taak toegevoegd![/]");
        }

        private void RemoveTask()
        {
            int id = AskTaskId("[red]ID van de taak om te verwijderen:[/]");
            _service.RemoveTask(id);
            AnsiConsole.MarkupLine("[bold yellow]Taak verwijderd (indien gevonden).[/]");
        }

        private void ToggleTask()
        {
            int id = AskTaskId("[blue]ID van de taak om te togglen:[/]");
            _service.ToggleTaskCompletion(id);
            AnsiConsole.MarkupLine("[bold aqua]Taakstatus aangepast![/]");
        }

        private void EditTask()
        {
            int id = AskTaskId("[yellow]ID van de taak om aan te passen:[/]");
            string newDesc = AnsiConsole.Ask<string>("[green]Nieuwe beschrijving:[/]");
            _service.UpdateTaskDescription(id, newDesc);
            AnsiConsole.MarkupLine("[bold green]Taak aangepast.[/]");
        }
    }
}
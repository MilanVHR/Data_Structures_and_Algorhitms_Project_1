using Spectre.Console;
using Project.Services;
using Project.Model;
using Project.Collections;
using System.Globalization;
using System.Threading;
using System;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private const string EnglishCulture = "en-US";
        private const string DutchCulture = "nl-NL";

        private readonly ITaskService _service;
        private TaskSortField _activeSortField = TaskSortField.Id;
        private bool _activeSortAscending = true;

        private TaskFilterField _activeFilterField = TaskFilterField.All;

        public ConsoleTaskView(ITaskService service)
        {
            _service = service;
        }
        private static void ApplyLanguage(string culture)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        private string PromptLanguageCulture()
        {
            var english = Texts.Get("Language_English");
            var dutch = Texts.Get("Language_Dutch");

            var selectedLanguage = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{Texts.Get("Choose_Language")}[/]")
                    .AddChoices(english, dutch)
                    .HighlightStyle(new Style(foreground: Color.Green)));

            return selectedLanguage == dutch ? DutchCulture : EnglishCulture;
        }

        public void Run()
        {
            Console.Clear();
            ApplyLanguage(PromptLanguageCulture());

            while (true)
            {
                DisplayTasks();

                var option = PromptMainMenu();

                switch (option)
                {
                    case var c when c == Texts.Get("Add_Task"):
                        RepeatActionUntilMenu(AddTask, Texts.Get("Add_Another_Task"));
                        break;

                    case var c when c == Texts.Get("Delete_Task"):
                        RepeatActionUntilMenu(RemoveTask, Texts.Get("Delete_Another_Task"));
                        break;

                    case var c when c == Texts.Get("Toggle_Task"):
                        RepeatActionUntilMenu(ToggleTask, Texts.Get("Toggle_Another_Task"));
                        break;

                    case var c when c == Texts.Get("Update_Task"):
                        RepeatActionUntilMenu(EditTask, Texts.Get("Update_Another_Task"));
                        break;

                    case var c when c == Texts.Get("Sort_Task"):
                        RepeatActionUntilMenu(SortTasks, Texts.Get("Another_Sort"));
                        break;

                    case var c when c == Texts.Get("Filter_Task"):
                        RepeatActionUntilMenu(FilterTasks, Texts.Get("Another_Filter"));
                        break;

                    case var c when c == Texts.Get("Change_Language"):
                        ApplyLanguage(PromptLanguageCulture());
                        break;

                    case var c when c == Texts.Get("Quit"):
                        return;
                }
            }
        }

        private void DisplayTasks(string? sectionTitle = null, int? selectedTaskId = null)
        {
            Console.Clear();

            AnsiConsole.Write(
                new FigletText(Texts.Get("App_Title"))
                    .Centered()
                    .Color(Color.Cyan1));

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn($"[cyan]{Texts.Get("ID")}[/]")
                .AddColumn($"[green]{Texts.Get("Description")}[/]")
                .AddColumn($"[blue]{Texts.Get("Completed")}[/]")
                .AddColumn($"[magenta]{Texts.Get("Created_Time")}[/]");

            IMyCollection<TaskItem> tasks;

            if (_activeFilterField == TaskFilterField.All)
            {
                tasks = _service.GetSortedTasks(_activeSortField, _activeSortAscending);
            }
            else
            {
                tasks = _service.GetFilteredTasks(_activeFilterField);
            }

            var it = tasks.GetIterator();
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
                            ? $"[black on yellow]{Texts.Get("Yes")}[/]"
                            : $"[black on yellow]{Texts.Get("No")}[/]",
                        $"[black on yellow]{FormatCreatedAt(task.CreatedAt)}[/]");
                }
                else
                {
                    table.AddRow(
                        task.Id.ToString(),
                        task.Description,
                        task.Completed ? $"[green]{Texts.Get("Yes")}[/]" : $"[red]{Texts.Get("No")}[/]",
                        FormatCreatedAt(task.CreatedAt));
                }
            }

            if (!hasTasks)
            {
                table.AddRow("-", $"[grey]{Texts.Get("No_Tasks_Yet")}[/]", "-", "-");
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]{Texts.Get("Sort")}: {GetSortLabel()}[/]");
            AnsiConsole.MarkupLine($"[grey]{Texts.Get("Filter")}: {GetFilterLabel()}[/]");
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
                    .Title($"[yellow]{Texts.Get("Choose_Option")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(
                        Texts.Get("Add_Task"),
                        Texts.Get("Delete_Task"),
                        Texts.Get("Toggle_Task"),
                        Texts.Get("Update_Task"),
                        Texts.Get("Sort_Task"),
                        Texts.Get("Filter_Task"),
                        Texts.Get("Change_Language"),
                        Texts.Get("Quit")));
        }

        private void SortTasks()
        {
            DisplayTasks(Texts.Get("Sort_Task"));

            var idChoice = Texts.Get("ID");
            var descriptionChoice = Texts.Get("Description");
            var completedChoice = Texts.Get("Completed");
            var createdTimeChoice = Texts.Get("Created_Time");

            string fieldChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Sort_On")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(idChoice, descriptionChoice, completedChoice, createdTimeChoice));

            _activeSortField = fieldChoice switch
            {
                var c when c == descriptionChoice => TaskSortField.Description,
                var c when c == completedChoice => TaskSortField.Status,
                var c when c == createdTimeChoice => TaskSortField.CreatedAt,
                _ => TaskSortField.Id
            };

            var ascendingChoice = Texts.Get("Sort_Ascending");
            var descendingChoice = Texts.Get("Sort_Descending");

            string directionChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Order")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(ascendingChoice, descendingChoice));

            _activeSortAscending = directionChoice == ascendingChoice;

            DisplayTasks($"{Texts.Get("Sort_Task")}");
            AnsiConsole.MarkupLine($"[bold green]{Texts.Get("Sort_Applied")}: {GetSortLabel()}[/]");
        }

        private void FilterTasks()
        {
            DisplayTasks($"{Texts.Get("Filter_Task")}");

            var allTasksChoice = Texts.Get("All_Tasks");
            var completedTasksChoice = Texts.Get("Completed_Tasks");
            var openTasksChoice = Texts.Get("Open_Tasks");

            string filterChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Filter_On_Status")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(allTasksChoice, completedTasksChoice, openTasksChoice));

            _activeFilterField = filterChoice switch
            {
                var c when c == completedTasksChoice => TaskFilterField.Completed,
                var c when c == openTasksChoice => TaskFilterField.Pending,
                _ => TaskFilterField.All
            };

            DisplayTasks(Texts.Get("Filter_Task"));
            AnsiConsole.MarkupLine($"[bold green]{Texts.Get("Filter_Applied")}: {GetFilterLabel()}[/]");
        }

        private void RepeatActionUntilMenu(Action action, string repeatText)
        {
            while (true)
            {
                action();

                var shouldRepeat = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[yellow]{Texts.Get("What_Do_You_Want_To_Do")}[/]")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices(repeatText, Texts.Get("Back_To_Menu")));

                if (shouldRepeat == Texts.Get("Back_To_Menu"))
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
                    .AddChoices(Texts.Get("Yes"), Texts.Get("No")));

            return choice == Texts.Get("Yes");
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
                TaskSortField.Description => Texts.Get("Description"),
                TaskSortField.Status => Texts.Get("Completed"),
                TaskSortField.CreatedAt => Texts.Get("Created_Time"),
                _ => Texts.Get("ID")
            };

            string direction = _activeSortAscending ? Texts.Get("Sort_Ascending") : Texts.Get("Sort_Descending");
            return $"{field} ({direction})";
        }

        private string GetFilterLabel()
        {
            return _activeFilterField switch
            {
                TaskFilterField.Completed => Texts.Get("Completed_Tasks"),
                TaskFilterField.Pending => Texts.Get("Open_Tasks"),
                _ => Texts.Get("All_Tasks")
            };
        }

        private void AddTask()
        {
            DisplayTasks(Texts.Get("Add_Task"));

            string desc = AnsiConsole.Ask<string>($"[green]{Texts.Get("Task_Description_Prompt")}[/]");
            _service.AddTask(desc);

            DisplayTasks(Texts.Get("Add_Task"));
            AnsiConsole.MarkupLine($"[bold green]{Texts.Get("Task_Added")}[/]");
        }

        private void RemoveTask()
        {
            DisplayTasks(Texts.Get("Delete_Task"));

            int id = AskTaskId($"[red]{Texts.Get("Task_Id_To_Delete_Prompt")}[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks(Texts.Get("Delete_Task"));
                AnsiConsole.MarkupLine($"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
                return;
            }

            DisplayTasks(Texts.Get("Delete_Task"), id);
            AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Selected_Task_Highlighted")}[/]");

            if (!Confirm(Texts.Get("Confirm_Delete_Task")))
            {
                DisplayTasks(Texts.Get("Delete_Task"), id);
                AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Delete_Cancelled")}[/]");
                return;
            }

            bool removed = _service.RemoveTask(id);

            DisplayTasks(Texts.Get("Delete_Task"));
            AnsiConsole.MarkupLine(
                removed
                    ? $"[bold green]{Texts.Get("Task_Removed_Success")}[/]"
                    : $"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
        }

        private void ToggleTask()
        {
            DisplayTasks(Texts.Get("Toggle_Task"));

            int id = AskTaskId($"[blue]{Texts.Get("Task_Id_To_Toggle_Prompt")}[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks(Texts.Get("Toggle_Task"));
                AnsiConsole.MarkupLine($"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
                return;
            }

            DisplayTasks(Texts.Get("Toggle_Task"), id);
            AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Selected_Task_Highlighted")}[/]");

            if (!Confirm(Texts.Get("Confirm_Toggle_Task_Status")))
            {
                DisplayTasks(Texts.Get("Toggle_Task"), id);
                AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Change_Cancelled")}[/]");
                return;
            }

            bool toggled = _service.ToggleTaskCompletion(id);

            DisplayTasks(Texts.Get("Toggle_Task"));
            AnsiConsole.MarkupLine(
                toggled
                    ? $"[bold green]{Texts.Get("Task_Status_Updated")}[/]"
                    : $"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
        }

        private void EditTask()
        {
            DisplayTasks(Texts.Get("Update_Task"));

            int id = AskTaskId($"[yellow]{Texts.Get("Task_Id_To_Update_Prompt")}[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks(Texts.Get("Update_Task"));
                AnsiConsole.MarkupLine($"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
                return;
            }

            DisplayTasks(Texts.Get("Update_Task"), id);
            AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Selected_Task_Highlighted")}[/]");

            string newDesc = AnsiConsole.Ask<string>($"[green]{Texts.Get("New_Description_Prompt")}[/]");

            if (!Confirm(Texts.Get("Confirm_Save_Changes")))
            {
                DisplayTasks(Texts.Get("Update_Task"), id);
                AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Update_Cancelled")}[/]");
                return;
            }

            bool updated = _service.UpdateTaskDescription(id, newDesc);

            DisplayTasks(Texts.Get("Update_Task"));
            AnsiConsole.MarkupLine(
                updated
                    ? $"[bold green]{Texts.Get("Task_Updated")}[/]"
                    : $"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
        }
    }
}
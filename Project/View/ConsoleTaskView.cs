using Spectre.Console;
using Project.Services;
using Project.Model;
using Project.Collections;
using System.Globalization;
using System.Threading;
using System.Text;
using System;

namespace Project.View
{
    public class ConsoleTaskView : ITaskView
    {
        private const string EnglishCulture = "en-US";
        private const string DutchCulture = "nl-NL";

        private readonly ITaskService _service;
        private readonly IUiSoundPlayer _soundPlayer;
        private TaskSortField _activeSortField = TaskSortField.Id;
        private bool _activeSortAscending = true;

        private TaskFilterField _activeFilterField = TaskFilterField.All;

        public ConsoleTaskView(ITaskService service, IUiSoundPlayer soundPlayer)
        {
            _service = service;
            _soundPlayer = soundPlayer;
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

            _soundPlayer.PlayClick();

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

                    case var c when c == Texts.Get("Move_Task"):
                        RepeatActionUntilMenu(MoveTask, Texts.Get("Move_Another_Task"));
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

            IMyCollection<TaskItem> tasks;
            tasks = _service.GetSortedAndFilteredTasks(_activeSortField, _activeSortAscending, _activeFilterField);

            var toDoTasks = new ArrayCollection<TaskItem>();
            var doingTasks = new ArrayCollection<TaskItem>();
            var toReviewTasks = new ArrayCollection<TaskItem>();
            var doneTasks = new ArrayCollection<TaskItem>();

            var it = tasks.GetIterator();
            while (it.HasNext())
            {
                var task = it.Next();

                switch (task.Status)
                {
                    case TaskStage.ToDo:
                        toDoTasks.Add(task);
                        break;

                    case TaskStage.Doing:
                        doingTasks.Add(task);
                        break;

                    case TaskStage.ToReview:
                        toReviewTasks.Add(task);
                        break;

                    case TaskStage.Done:
                        doneTasks.Add(task);
                        break;
                }
            }

            if (_activeFilterField == TaskFilterField.All)
            {
                var boardTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Grey)
                    .AddColumn($"[bold steelblue1]{Texts.Get("To_Do")}[/]")
                    .AddColumn($"[bold orange1]{Texts.Get("Doing")}[/]")
                    .AddColumn($"[bold mediumpurple]{Texts.Get("To_Review")}[/]")
                    .AddColumn($"[bold green]{Texts.Get("Done")}[/]");

                boardTable.AddRow(
                    CreateBoardCell(toDoTasks, selectedTaskId, Color.SteelBlue, "steelblue1", "steelblue3"),
                    CreateBoardCell(doingTasks, selectedTaskId, Color.Orange1, "orange1", "orange3"),
                    CreateBoardCell(toReviewTasks, selectedTaskId, Color.MediumPurple, "mediumpurple", "mediumpurple3"),
                    CreateBoardCell(doneTasks, selectedTaskId, Color.Green, "green", "green3"));

                AnsiConsole.Write(boardTable);
            }
            else
            {
                Panel lanePanel;

                switch (_activeFilterField)
                {
                    case TaskFilterField.ToDo:
                        lanePanel = CreateLanePanel(Texts.Get("To_Do"), Color.SteelBlue, toDoTasks, selectedTaskId, "steelblue1", "steelblue3");
                        break;

                    case TaskFilterField.Doing:
                        lanePanel = CreateLanePanel(Texts.Get("Doing"), Color.Orange1, doingTasks, selectedTaskId, "orange1", "orange3");
                        break;

                    case TaskFilterField.ToReview:
                        lanePanel = CreateLanePanel(Texts.Get("To_Review"), Color.MediumPurple, toReviewTasks, selectedTaskId, "mediumpurple", "mediumpurple3");
                        break;

                    case TaskFilterField.Done:
                        lanePanel = CreateLanePanel(Texts.Get("Done"), Color.Green, doneTasks, selectedTaskId, "green", "green3");
                        break;

                    default:
                        lanePanel = CreateLanePanel(Texts.Get("To_Do"), Color.SteelBlue, toDoTasks, selectedTaskId, "steelblue1", "steelblue3");
                        break;
                }

                AnsiConsole.Write(lanePanel);
            }

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
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Choose_Option")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(
                        Texts.Get("Add_Task"),
                        Texts.Get("Delete_Task"),
                        Texts.Get("Move_Task"),
                        Texts.Get("Update_Task"),
                        Texts.Get("Sort_Task"),
                        Texts.Get("Filter_Task"),
                        Texts.Get("Change_Language"),
                        Texts.Get("Quit")));

            _soundPlayer.PlayClick();
            return option;
        }

        private void SortTasks()
        {
            DisplayTasks(Texts.Get("Sort_Task"));

            var idChoice = Texts.Get("ID");
            var descriptionChoice = Texts.Get("Description");
            var createdTimeChoice = Texts.Get("Created_Time");

            string fieldChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Sort_On")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(idChoice, descriptionChoice, createdTimeChoice));

            _soundPlayer.PlayClick();

            _activeSortField = fieldChoice switch
            {
                var c when c == descriptionChoice => TaskSortField.Description,
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

            _soundPlayer.PlayClick();

            _activeSortAscending = directionChoice == ascendingChoice;

            DisplayTasks($"{Texts.Get("Sort_Task")}");
            AnsiConsole.MarkupLine($"[bold green]{Texts.Get("Sort_Applied")}: {GetSortLabel()}[/]");
        }

        private void FilterTasks()
        {
            DisplayTasks($"{Texts.Get("Filter_Task")}");

            var allTasksChoice = Texts.Get("All_Tasks");
            var toDoTasksChoice = Texts.Get("To_Do_Tasks");
            var doingTasksChoice = Texts.Get("Doing_Tasks");
            var toReviewTasksChoice = Texts.Get("To_Review_Tasks");
            var doneTasksChoice = Texts.Get("Done_Tasks");

            string filterChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Filter_On_Status")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(allTasksChoice, toDoTasksChoice, doingTasksChoice, toReviewTasksChoice, doneTasksChoice));

            _soundPlayer.PlayClick();

            _activeFilterField = filterChoice switch
            {
                var c when c == toDoTasksChoice => TaskFilterField.ToDo,
                var c when c == doingTasksChoice => TaskFilterField.Doing,
                var c when c == toReviewTasksChoice => TaskFilterField.ToReview,
                var c when c == doneTasksChoice => TaskFilterField.Done,
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

                _soundPlayer.PlayClick();

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

            _soundPlayer.PlayClick();

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
                TaskSortField.Status => Texts.Get("Status"),
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
                TaskFilterField.ToDo => Texts.Get("To_Do_Tasks"),
                TaskFilterField.Doing => Texts.Get("Doing_Tasks"),
                TaskFilterField.ToReview => Texts.Get("To_Review_Tasks"),
                TaskFilterField.Done => Texts.Get("Done_Tasks"),
                _ => Texts.Get("All_Tasks")
            };
        }

        // This method creates a panel for a lane in the Kanban board.
        // It takes the lane title, border color, tasks in that lane, the currently selected task ID (if any), and accent colors for styling.
        private Panel CreateLanePanel(
            string title,
            Color borderColor,
            IMyCollection<TaskItem> laneTasks,
            int? selectedTaskId,
            string accentColor,
            string secondaryColor)
        {
            return new Panel(new Markup(BuildLaneContent(laneTasks, selectedTaskId, accentColor, secondaryColor)))
            {
                Header = new PanelHeader($"[bold]{title}[/]", Justify.Center),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(borderColor),
                Padding = new Padding(1, 0, 1, 0)
            };
        }

        // This method creates a panel for a lane in the Kanban board. 
        //It uses the BuildLaneContent method to generate the content string based on the tasks in that lane.
        private Panel CreateBoardCell(
            IMyCollection<TaskItem> laneTasks,
            int? selectedTaskId,
            Color borderColor,
            string accentColor,
            string secondaryColor)
        {
            return new Panel(new Markup(BuildLaneContent(laneTasks, selectedTaskId, accentColor, secondaryColor)))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(borderColor),
                Padding = new Padding(1, 0, 1, 0)
            };
        }

        // This method builds the content string for a lane in the Kanban board. 
        //It iterates through the tasks in the lane and formats each task's description and creation date. 
        //The currently selected task (if any) is highlighted with a different background color. 
        //If there are no tasks, it shows a placeholder message.
        private string BuildLaneContent(
            IMyCollection<TaskItem> laneTasks,
            int? selectedTaskId,
            string accentColor,
            string secondaryColor)
        {
            var content = new StringBuilder();
            var it = laneTasks.GetIterator();

            if (!it.HasNext())
            {
                content.Append($"[{secondaryColor}]{Texts.Get("No_Tasks_Yet")}[/]");
                return content.ToString();
            }

            while (it.HasNext())
            {
                var task = it.Next();
                bool isSelected = selectedTaskId.HasValue && task.Id == selectedTaskId.Value;
                var escapedDescription = Markup.Escape(task.Description);
                var escapedDate = Markup.Escape(FormatCreatedAt(task.CreatedAt));

                if (isSelected)
                {
                    content.AppendLine($"[black on yellow]#{task.Id} {escapedDescription}[/]");
                    content.AppendLine($"[black on yellow]{escapedDate}[/]");
                }
                else
                {
                    content.AppendLine($"[bold {accentColor}]#{task.Id}[/] [{accentColor}]{escapedDescription}[/]");
                    content.AppendLine($"[{secondaryColor}]{escapedDate}[/]");
                }

                if (it.HasNext())
                    content.AppendLine();
            }

            return content.ToString();
        }

        // This method prompts the user to select a new status for a task when moving it. 
        // It displays the available statuses with localized names and returns the corresponding 
        //TaskStage value based on the user's selection.
        private TaskStage PromptTaskStatus(string title)
        {
            var toDoChoice = Texts.Get("To_Do");
            var doingChoice = Texts.Get("Doing");
            var toReviewChoice = Texts.Get("To_Review");
            var doneChoice = Texts.Get("Done");

            var selectedStatus = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{title}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(toDoChoice, doingChoice, toReviewChoice, doneChoice));

            _soundPlayer.PlayClick();

            return selectedStatus switch
            {
                var c when c == doingChoice => TaskStage.Doing,
                var c when c == toReviewChoice => TaskStage.ToReview,
                var c when c == doneChoice => TaskStage.Done,
                _ => TaskStage.ToDo
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
                _soundPlayer.PlayError();
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

            if (!removed)
            {
                _soundPlayer.PlayError();
            }
        }

        // This method prompts the user to select a new status for a task when moving it.
        // It displays the available statuses with localized names and returns the corresponding
        //TaskStage value based on the user's selection.
        private void MoveTask()
        {
            DisplayTasks(Texts.Get("Move_Task"));

            int id = AskTaskId($"[blue]{Texts.Get("Task_Id_To_Move_Prompt")}[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks(Texts.Get("Move_Task"));
                AnsiConsole.MarkupLine($"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
                _soundPlayer.PlayError();
                return;
            }

            DisplayTasks(Texts.Get("Move_Task"), id);
            AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Selected_Task_Highlighted")}[/]");

            var newStatus = PromptTaskStatus(Texts.Get("Move_Task_To_Status"));

            if (selectedTask.Status == newStatus)
            {
                DisplayTasks(Texts.Get("Move_Task"), id);
                AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Task_Already_In_Status")}[/]");
                _soundPlayer.PlayError();
                return;
            }

            if (!Confirm(Texts.Get("Confirm_Move_Task")))
            {
                DisplayTasks(Texts.Get("Move_Task"), id);
                AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Move_Cancelled")}[/]");
                return;
            }

            bool moved = _service.MoveTaskToStatus(id, newStatus);

            DisplayTasks(Texts.Get("Move_Task"));
            AnsiConsole.MarkupLine(
                moved
                    ? $"[bold green]{Texts.Get("Task_Moved_Success")}[/]"
                    : $"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");

            if (!moved)
            {
                _soundPlayer.PlayError();
            }
        }

        // This method prompts the user to select a new status for a task when moving it.
        // It displays the available statuses with localized names and returns the corresponding
        //TaskStage value based on the user's selection.

        private void EditTask()
        {
            DisplayTasks(Texts.Get("Update_Task"));

            int id = AskTaskId($"[yellow]{Texts.Get("Task_Id_To_Update_Prompt")}[/]");
            var selectedTask = _service.GetTaskById(id);

            if (selectedTask == null)
            {
                DisplayTasks(Texts.Get("Update_Task"));
                AnsiConsole.MarkupLine($"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
                _soundPlayer.PlayError();
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

            if (!updated)
            {
                _soundPlayer.PlayError();
            }
        }
    }
}
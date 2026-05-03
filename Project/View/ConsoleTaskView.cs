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
        private TaskSortField _activeSortField = TaskSortField.Id;
        private bool _activeSortAscending = true;

        private TaskFilterField _activeFilterField = TaskFilterField.All;

        private const int PageSize = 10;
        private int _toDoPage;
        private int _doingPage;
        private int _toReviewPage;
        private int _donePage;

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

                    case var c when c == Texts.Get("Page_Task"):
                        NavigatePages();
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

            int toDoPageCount = GetPageCount(toDoTasks.Count);
            int doingPageCount = GetPageCount(doingTasks.Count);
            int toReviewPageCount = GetPageCount(toReviewTasks.Count);
            int donePageCount = GetPageCount(doneTasks.Count);

            _toDoPage = ClampPageIndex(_toDoPage, toDoPageCount);
            _doingPage = ClampPageIndex(_doingPage, doingPageCount);
            _toReviewPage = ClampPageIndex(_toReviewPage, toReviewPageCount);
            _donePage = ClampPageIndex(_donePage, donePageCount);

            var toDoPageTasks = GetPagedTasks(toDoTasks, _toDoPage);
            var doingPageTasks = GetPagedTasks(doingTasks, _doingPage);
            var toReviewPageTasks = GetPagedTasks(toReviewTasks, _toReviewPage);
            var donePageTasks = GetPagedTasks(doneTasks, _donePage);

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
                    CreateBoardCell(toDoPageTasks, selectedTaskId, Color.SteelBlue, "steelblue1", "steelblue3", _toDoPage + 1, toDoPageCount),
                    CreateBoardCell(doingPageTasks, selectedTaskId, Color.Orange1, "orange1", "orange3", _doingPage + 1, doingPageCount),
                    CreateBoardCell(toReviewPageTasks, selectedTaskId, Color.MediumPurple, "mediumpurple", "mediumpurple3", _toReviewPage + 1, toReviewPageCount),
                    CreateBoardCell(donePageTasks, selectedTaskId, Color.Green, "green", "green3", _donePage + 1, donePageCount));

                AnsiConsole.Write(boardTable);
            }
            else
            {
                Panel lanePanel;

                switch (_activeFilterField)
                {
                    case TaskFilterField.ToDo:
                        lanePanel = CreateLanePanel(Texts.Get("To_Do"), Color.SteelBlue, toDoPageTasks, selectedTaskId, "steelblue1", "steelblue3", _toDoPage + 1, toDoPageCount);
                        break;

                    case TaskFilterField.Doing:
                        lanePanel = CreateLanePanel(Texts.Get("Doing"), Color.Orange1, doingPageTasks, selectedTaskId, "orange1", "orange3", _doingPage + 1, doingPageCount);
                        break;

                    case TaskFilterField.ToReview:
                        lanePanel = CreateLanePanel(Texts.Get("To_Review"), Color.MediumPurple, toReviewPageTasks, selectedTaskId, "mediumpurple", "mediumpurple3", _toReviewPage + 1, toReviewPageCount);
                        break;

                    case TaskFilterField.Done:
                        lanePanel = CreateLanePanel(Texts.Get("Done"), Color.Green, donePageTasks, selectedTaskId, "green", "green3", _donePage + 1, donePageCount);
                        break;

                    default:
                        lanePanel = CreateLanePanel(Texts.Get("To_Do"), Color.SteelBlue, toDoPageTasks, selectedTaskId, "steelblue1", "steelblue3", _toDoPage + 1, toDoPageCount);
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
            return AnsiConsole.Prompt(
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
                        Texts.Get("Page_Task"),
                        Texts.Get("Change_Language"),
                        Texts.Get("Quit")));
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
            string secondaryColor,
            int pageIndex,
            int pageCount)
        {
            return new Panel(new Markup(BuildLaneContent(laneTasks, selectedTaskId, accentColor, secondaryColor, pageIndex, pageCount)))
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
            string secondaryColor,
            int pageIndex,
            int pageCount)
        {
            return new Panel(new Markup(BuildLaneContent(laneTasks, selectedTaskId, accentColor, secondaryColor, pageIndex, pageCount)))
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
            string secondaryColor,
            int pageIndex,
            int pageCount)
        {
            var content = new StringBuilder();
            var it = laneTasks.GetIterator();

            if (!it.HasNext())
            {
                content.Append($"[{secondaryColor}]{Texts.Get("No_Tasks_Yet")}[/]");
            }
            else
            {
                while (it.HasNext())
                {
                    var task = it.Next();
                    bool isSelected = selectedTaskId.HasValue && task.Id == selectedTaskId.Value;
                    var escapedDescription = Markup.Escape(task.Description);
                    var escapedDate = Markup.Escape(FormatCreatedAt(task.CreatedAt));
                    var assigneeText = string.IsNullOrEmpty(task.AssignedTo)
                        ? Texts.Get("Unassigned")
                        : task.AssignedTo ?? string.Empty;
                    var escapedAssignee = Markup.Escape(assigneeText);

                    if (isSelected)
                    {
                        content.AppendLine($"[black on yellow]#{task.Id} {escapedDescription}[/]");
                        content.AppendLine($"[black on yellow]{escapedDate}[/]");
                        content.AppendLine($"[black on yellow]{Texts.Get("Assigned_To")}: {escapedAssignee}[/]");
                    }
                    else
                    {
                        content.AppendLine($"[bold {accentColor}]#{task.Id}[/] [{accentColor}]{escapedDescription}[/]");
                        content.AppendLine($"[{secondaryColor}]{escapedDate}[/]");
                        content.AppendLine($"[{secondaryColor}]{Texts.Get("Assigned_To")}: {escapedAssignee}[/]");
                    }

                    if (it.HasNext())
                        content.AppendLine();
                }
            }

            content.AppendLine();
            content.Append($"[{secondaryColor}]{Texts.Get("Page_Label")} {pageIndex}/{pageCount}[/]");
            return content.ToString();
        }

        // This method takes a collection of tasks for a specific lane and returns only the tasks that should be displayed on the current page based on the defined page size.
        private IMyCollection<TaskItem> GetPagedTasks(IMyCollection<TaskItem> laneTasks, int pageIndex)
        {
            var pagedTasks = new ArrayCollection<TaskItem>();
            int startIndex = pageIndex * PageSize;
            int endIndex = startIndex + PageSize;
            int currentIndex = 0;
            // Iterate through the tasks in the lane and add only those that fall within the current page range to the pagedTasks collection.
            var it = laneTasks.GetIterator();
            while (it.HasNext())
            {
                var task = it.Next();

                if (currentIndex >= startIndex && currentIndex < endIndex)
                {
                    pagedTasks.Add(task);
                }
                // Increment the current index to keep track of how many tasks have been processed, and break the loop once we've added enough tasks for the current page. 
                // This ensures that we only process as many tasks as needed for display, improving performance when there are many tasks in a lane.
                currentIndex++;
                if (currentIndex >= endIndex)
                    break;
            }

            return pagedTasks;
        }
        // This method calculates the total number of pages needed to display a given number of items based on the defined page size.
        private int GetPageCount(int totalItems)
        {
            if (totalItems <= 0)
                return 1;

            return (totalItems + PageSize - 1) / PageSize;
        }
        // This method ensures that the page index stays within valid bounds based on the total number of pages available.
        private int ClampPageIndex(int pageIndex, int pageCount)
        {
            if (pageIndex < 0)
                return 0;

            if (pageIndex >= pageCount)
                return pageCount - 1;

            return pageIndex;
        }
        // This method handles the pagination navigation for the Kanban board.
        private void NavigatePages()
        {
            var tasks = _service.GetSortedAndFilteredTasks(_activeSortField, _activeSortAscending, _activeFilterField);

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

            int toDoPageCount = GetPageCount(toDoTasks.Count);
            int doingPageCount = GetPageCount(doingTasks.Count);
            int toReviewPageCount = GetPageCount(toReviewTasks.Count);
            int donePageCount = GetPageCount(doneTasks.Count);

            var toDoPrev = $"{Texts.Get("To_Do")} {Texts.Get("Previous_Page")}";
            var toDoNext = $"{Texts.Get("To_Do")} {Texts.Get("Next_Page")}";
            var doingPrev = $"{Texts.Get("Doing")} {Texts.Get("Previous_Page")}";
            var doingNext = $"{Texts.Get("Doing")} {Texts.Get("Next_Page")}";
            var toReviewPrev = $"{Texts.Get("To_Review")} {Texts.Get("Previous_Page")}";
            var toReviewNext = $"{Texts.Get("To_Review")} {Texts.Get("Next_Page")}";
            var donePrev = $"{Texts.Get("Done")} {Texts.Get("Previous_Page")}";
            var doneNext = $"{Texts.Get("Done")} {Texts.Get("Next_Page")}";

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Page_Task")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(
                        toDoPrev,
                        toDoNext,
                        doingPrev,
                        doingNext,
                        toReviewPrev,
                        toReviewNext,
                        donePrev,
                        doneNext,
                        Texts.Get("Back_To_Menu")));

            switch (choice)
            {
                case var c when c == toDoPrev:
                    _toDoPage = ClampPageIndex(_toDoPage - 1, toDoPageCount);
                    break;
                case var c when c == toDoNext:
                    _toDoPage = ClampPageIndex(_toDoPage + 1, toDoPageCount);
                    break;
                case var c when c == doingPrev:
                    _doingPage = ClampPageIndex(_doingPage - 1, doingPageCount);
                    break;
                case var c when c == doingNext:
                    _doingPage = ClampPageIndex(_doingPage + 1, doingPageCount);
                    break;
                case var c when c == toReviewPrev:
                    _toReviewPage = ClampPageIndex(_toReviewPage - 1, toReviewPageCount);
                    break;
                case var c when c == toReviewNext:
                    _toReviewPage = ClampPageIndex(_toReviewPage + 1, toReviewPageCount);
                    break;
                case var c when c == donePrev:
                    _donePage = ClampPageIndex(_donePage - 1, donePageCount);
                    break;
                case var c when c == doneNext:
                    _donePage = ClampPageIndex(_donePage + 1, donePageCount);
                    break;
                default:
                    return;
            }
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

            string? assignee = PromptAssignee();

            _service.AddTask(desc, assignee);

            DisplayTasks(Texts.Get("Add_Task"));
            AnsiConsole.MarkupLine($"[bold green]{Texts.Get("Task_Added")}[/]");
        }

        private string? PromptAssignee()
        {
            var assignees = _service.GetAssignees();
            var options = new List<string>();
            foreach (var assignee in assignees)
            {
                options.Add(assignee);
            }
            options.Add(Texts.Get("Other_Add_New_Name"));

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[green]{Texts.Get("Assign_Task_Prompt")}[/]")
                    .AddChoices(options)
                    .HighlightStyle(new Style(foreground: Color.Green)));

            if (selected == Texts.Get("Other_Add_New_Name"))
            {
                string newName = AnsiConsole.Ask<string>($"[green]{Texts.Get("Enter_New_Name")}[/]");
                return newName;
            }

            return selected;
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
                return;
            }

            DisplayTasks(Texts.Get("Move_Task"), id);
            AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Selected_Task_Highlighted")}[/]");

            var newStatus = PromptTaskStatus(Texts.Get("Move_Task_To_Status"));

            if (selectedTask.Status == newStatus)
            {
                DisplayTasks(Texts.Get("Move_Task"), id);
                AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Task_Already_In_Status")}[/]");
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
                return;
            }

            DisplayTasks(Texts.Get("Update_Task"), id);
            AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Selected_Task_Highlighted")}[/]");

            var updateDescriptionChoice = Texts.Get("Update_Description");
            var updateAssigneeChoice = Texts.Get("Update_Assignee");

            string updateChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Texts.Get("Update_Field_Prompt")}[/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(updateDescriptionChoice, updateAssigneeChoice));

            bool updated;

            if (updateChoice == updateAssigneeChoice)
            {
                string? newAssignee = PromptAssignee();

                if (!Confirm(Texts.Get("Confirm_Save_Changes")))
                {
                    DisplayTasks(Texts.Get("Update_Task"), id);
                    AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Update_Cancelled")}[/]");
                    return;
                }

                updated = _service.UpdateTaskAssignee(id, newAssignee ?? string.Empty);
            }
            else
            {
                string newDesc = AnsiConsole.Ask<string>($"[green]{Texts.Get("New_Description_Prompt")}[/]");

                if (!Confirm(Texts.Get("Confirm_Save_Changes")))
                {
                    DisplayTasks(Texts.Get("Update_Task"), id);
                    AnsiConsole.MarkupLine($"[bold yellow]{Texts.Get("Update_Cancelled")}[/]");
                    return;
                }

                updated = _service.UpdateTaskDescription(id, newDesc);
            }

            DisplayTasks(Texts.Get("Update_Task"));
            AnsiConsole.MarkupLine(
                updated
                    ? $"[bold green]{Texts.Get("Task_Updated")}[/]"
                    : $"[bold red]{string.Format(Texts.Get("Task_With_Id_Not_Found"), id)}[/]");
        }
    }
}
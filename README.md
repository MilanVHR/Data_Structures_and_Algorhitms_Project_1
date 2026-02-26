# Het Project

Dit project is een console‑gebaseerde To‑Do applicatie gebouwd in C#.
Het doel van de opdracht is om eigen datastructuren te implementeren en deze toe te passen in een werkende applicatie volgens Clean Architecture.

De applicatie ondersteunt:

Taken toevoegen

Taken verwijderen

Taken togglen (voltooid / niet voltooid)

Opslag in JSON

Een mooie console‑interface met Spectre.Console

Een volledig zelfgeschreven ArrayCollection + Iterator







# ARCHITECTUUR

Het project volgt een eenvoudige variant van Clean Architecture, waarbij elke laag een duidelijke verantwoordelijkheid heeft.

1. Model Layer
Bevat alleen data‑objecten.

TaskItem

ID

Description

Completed

2. Collections Layer
Bevat de zelfgemaakte datastructuren die verplicht zijn voor de opdracht.

IMyCollection<T> — interface voor eigen collecties

IMyIterator<T> — interface voor iterators

ArrayCollection<T> — dynamische array (vergelijkbaar met List<T>)

Automatische resizing

Eigen iterator

Eigen FindBy‑methode

3. Repository Layer
Verantwoordelijk voor opslag en laden van data.

ITaskRepository — abstractie

JsonTaskRepository — implementatie met JSON‑bestand

Leest en schrijft tasks.json

Gebruikt ArrayCollection voor opslag

4. Service Layer
Bevat alle business logic.

ITaskService

TaskService

Taken toevoegen

Taken verwijderen

Taken togglen

Automatisch ID genereren

Werkt uitsluitend via IMyCollection

5. View Layer
De gebruikersinterface.

ITaskView

ConsoleTaskView

Gebouwd met Spectre.Console

Mooie tabellen, prompts en kleuren

Roept alleen de Service‑laag aan

6. Program.cs
De “composition root”:

Maakt repository → service → view

Start de applicatie


# Data Structuren

De kern van dit project is de ArrayCollection, een zelfgeschreven dynamische array.

Functionaliteit:
Automatisch vergroten van capaciteit

Elementen toevoegen

Elementen verwijderen

Zoeken met FindBy

Itereren met een eigen iterator

Converteren naar array voor JSON

Waarom geen List<T>?
De opdracht vereist dat alle datastructuren zelf worden geïmplementeerd.
Daarom wordt nergens gebruikgemaakt van:

List<T>

Dictionary<T>

LinkedList<T>

LINQ


# Console UI (Spectere.Console)

De applicatie gebruikt Spectre.Console voor een moderne en overzichtelijke interface.

Voorbeelden van UI‑elementen:

ASCII‑titel (FigletText)

Tabellen (Table)

Menu’s (SelectionPrompt)

Kleurrijke prompts (AnsiConsole.Ask)

Dit maakt de applicatie veel gebruiksvriendelijker en professioneler.


# Data

Alles wordt opgeslagen in Project/Data?tasks.json

# Het programma runnen

Run het programma door in de map waar Project.csproj file zit "dotnet run" te doen


# Project structuur

Project/
│   Program.cs
│   Project.csproj
│   tasks.json
│
├── Collections/
│     ArrayCollection.cs
│     IMyCollection.cs
│     IMyIterator.cs
│
├── Model/
│     TaskItem.cs
│
├── Repository/
│     ITaskRepository.cs
│     JsonTaskRepository.cs
│
├── Services/
│     ITaskService.cs
│     TaskService.cs
│
└── View/
      ITaskView.cs
      ConsoleTaskView.cs
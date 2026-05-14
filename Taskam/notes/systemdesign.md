Below is a **clear, complete, university‑grade system design** for your WPF application **Taskam**, including architecture diagrams, module boundaries, data flow, and recommended technologies.  
It is designed specifically for:

- .NET 8  
- WPF (MVVM)  
- Clean Architecture  
- Local persistent storage (SQLite)  
- Modules: Calendar, Todo, Pomodoro, Projects, Timeline, Kanban  

Everything is written so you can paste it into your documentation.

---

# 🎯 1. System Overview

**Taskam** is a desktop productivity dashboard built using **WPF + MVVM + Clean Architecture**.  
Its purpose is to unify student productivity tools:

- Weekly class calendar  
- Daily tasks  
- Project timeline  
- Project Kanban board  
- Pomodoro with time analytics  
- Course, class, event management  
- Time tracking across activities  

The system is modular, extensible, and follows a clear separation of concerns.

---

# 🏛 2. High-Level Architecture (Clean Architecture)

```
+-------------------------------------------------------+
|                     UI (WPF)                          |
|  - Views (XAML)                                       |
|  - ViewModels (MVVM Toolkit)                          |
+--------------------------+----------------------------+
                           |
                           |
+-------------------------------------------------------+
|                   Application Layer                   |
|  - Services (Orchestrate modules)                     |
|  - Navigation                                         |
|  - Commands / Validators                              |
+--------------------------+----------------------------+
                           |
                           |
+-------------------------------------------------------+
|                   Domain Layer (Core)                 |
|  Entities:                                             |
|    - TaskItem                                          |
|    - TodoItem                                          |
|    - Project                                           |
|    - ProjectTask (Kanban)                              |
|    - PomodoroSession                                   |
|    - Course, Class                                     |
|    - CalendarEvent                                     |
|  Enums:                                                |
|    - KanbanStatus                                      |
|    - PomodoroType                                      |
+--------------------------+----------------------------+
                           |
                           |
+-------------------------------------------------------+
|                   Infrastructure                      |
|  - SQLite Database (EF Core)                           |
|  - Repositories                                        |
|  - Data Mappers                                        |
+-------------------------------------------------------+
```

---

# 🔧 3. Project Folder Structure

```
Taskam
│
├─ Taskam.Core
│   ├─ Entities
│   ├─ Enums
│   └─ Interfaces
│
├─ Taskam.Data
│   ├─ TaskamDbContext.cs
│   ├─ Repositories
│   └─ Migrations
│
├─ Taskam.Services
│   ├─ TodoService.cs
│   ├─ ProjectService.cs
│   ├─ CalendarService.cs
│   ├─ PomodoroService.cs
│   └─ ReportingService.cs
│
├─ Taskam.UI
│   ├─ Views
│   ├─ ViewModels
│   ├─ Controls
│   └─ App.xaml / MainWindow.xaml
```

---

# 🧬 4. Domain Model (Entities)

## 4.1 Project + Kanban

```
Project {
    int Id
    string Name
    DateTime StartDate
    DateTime EndDate
    ICollection<ProjectTask> Tasks
}
```

```
ProjectTask {
    int Id
    int ProjectId
    string Title
    string Description
    KanbanStatus Status
    int EstimatedMinutes
}
```

```
enum KanbanStatus {
    NotStarted,
    InProgress,
    Suspended,
    Done
}
```

---

## 4.2 Todo Module

```
TodoItem {
    int Id
    string Title
    string? Notes
    DateTime? DueDate
    bool IsToday
    bool IsCompleted
}
```

---

## 4.3 Calendar Module (Weekly Classes)

```
ClassEvent {
    int Id
    string CourseName
    DayOfWeek Day
    TimeSpan Start
    TimeSpan End
    string Location
}
```

```
CalendarEvent {
    int Id
    string Title
    DateTime Start
    DateTime End
}
```

---

## 4.4 Pomodoro Module

```
PomodoroSession {
    int Id
    PomodoroType Type     // Project, Course, Todo
    int? LinkedProjectTaskId
    int? LinkedCourseId
    int? LinkedTodoId
    DateTime Start
    DateTime End
    int DurationMinutes
}
```

---

# 🧱 5. Services (Application Layer)

Each module has a dedicated service.

### 5.1 TodoService
- Add/Edit/Delete
- Toggle "today"
- Auto‑move overdue tasks
- Provide data to UI

### 5.2 ProjectService
- Create project
- Add tasks to project
- Update Kanban status
- Provide tasks by status

### 5.3 TimelineService
- Provides list of projects with date ranges
- Prepares data for horizontal timeline visualization

### 5.4 CalendarService
- List weekly classes
- List events
- Merge for weekly view

### 5.5 PomodoroService
- Start session
- Stop session
- Store session in DB
- Generate time analytics:  
  - Time spent per project  
  - Time spent per course  
  - Time spent on todos  

---

# 🗄 6. Infrastructure Layer

## Database: SQLite via EF Core

### Tables:
- Projects  
- ProjectTasks  
- TodoItems  
- ClassEvents  
- CalendarEvents  
- PomodoroSessions  

### Repositories:
- GenericRepository<T>
- ProjectRepository
- TodoRepository
- CalendarRepository
- PomodoroRepository

---

# 🖥 7. UI Layer (WPF MVVM)

Your UI is the **Dashboard OS**.

## Main Layout (as you designed)

```
+---------------------------------------------------------------+
| Sidebar (Navigation)      |    Google Calendar (Week View)   |
| Pomodoro                  |                                   |
| Projects                  |-----------------------------------|
| Courses                   |     Daily Tasks                   |
| Classes                   |                                   |
| Tasks                     |                                   |
| Events                    |                                   |
+---------------------------------------------------------------+
|                   Project Timeline (Horizontal)               |
+---------------------------------------------------------------+
```

---

# 🧩 8. Module UI + ViewModels

### 8.1 CalendarWeekView + CalendarWeekViewModel
- Displays recurring weekly classes
- Displays events
- Maps time → grid rows

### 8.2 TodoView + TodoViewModel
- Tasks for today
- Quick add
- Mark complete

### 8.3 KanbanView + KanbanViewModel
- 4 columns (NotStarted / InProgress / Suspended / Done)

### 8.4 TimelineView + TimelineViewModel
- Shows projects as bars across a timeline

### 8.5 PomodoroView + PomodoroViewModel
- Activity selector (Project/Course/Todo)
- Timer
- Logs session → PomodoroService

### 8.6 NavigationService
- Switches between modules
- Uses ContentControl + DataTemplate

---

# 🔗 9. Data Flow Diagram

```
User Action → View → ViewModel → Service → Repository → SQLite
                                                       ↑
                                       ViewModel ← Service
```

Example: user moves task to "In Progress":

```
KanbanView
→ KanbanVM.MoveToInProgress(task)
→ ProjectService.UpdateStatus(id, InProgress)
→ ProjectRepository.Update(task)
→ SQLite
→ ProjectService.GetTasks()
→ KanbanVM updates UI
```

---

# 🔒 10. Non‑Functional Requirements

- **Performance**: Fast load, local cache  
- **Scalability**: Easily add modules  
- **Reliability**: Autosave, stable persistence  
- **Usability**: Clear layout, minimal clicks  
- **Maintainability**: Clean architecture layers  

---

# 🎓 11. Justification for Architecture (For University Report)

- MVVM ensures clear separation between UI and logic.  
- Clean Architecture improves modularity and testability.  
- SQLite is lightweight and perfect for a student productivity tool.  
- Using services decouples logic from UI, enabling future features.  
- WPF Material Design provides modern, clean UX.

This is **exactly what instructors look for** in a system design submission.

---

If you want, I can now generate:

✅ UML Class Diagram  
✅ Use Case Diagram  
✅ Sequence Diagrams (for Pomodoro, Calendar, Tasks, etc.)  
✅ A full SRS (Software Requirements Specification)  
✅ A full architecture PDF using the canvas tool  

Just tell me what you need.
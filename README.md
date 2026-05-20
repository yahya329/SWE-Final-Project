# 📔 Personal Diary System
> A data-driven desktop application built with C# (.NET Framework), Oracle Database, and ODP.Net — designed from a formal Software Requirements Specification (SRS) and implemented with a clean layered architecture.

---

## 🧠 Overview

This project was not just about building a diary app.
My goal was **doing it right** _ applying what we learn on SWE and SWLC (Software lifecycle)
starting from a formal SRS document, designing the database schema from requirements, and implementing every layer with purpose.

Every line of code traces back to a requirement.  
Every design decision has a reason.

<img width="1021" height="664" alt="Main_page" src="https://github.com/user-attachments/assets/8c8c4c93-2414-4de5-a547-c04ca342dba5" />

---

## 🏗️ System Design
---

### ⚙️ Core Features

- User Authentication (BCrypt-secured)
- Create / Read / Update / Delete diary entries
- Search by keyword or date
- Monthly mood analytics
- Connected & Disconnected database modes

---
## Programm flow
```
┌─────────────────────────────────────────┐
│              Presentation Layer          │
│   Windows Forms (LoginPage, MainPage,   │
│   Form1 Connected, Form2 Disconnected)  │
├─────────────────────────────────────────┤
│               Service Layer             │
│      AuthService  │  DiaryService       │
│   (business logic, validation, rules)   │
├─────────────────────────────────────────┤
│             Data Access Layer           │
│  UserRepository  │  DiaryRepository    │
│  DiaryRepositoryDisconnected            │
│         DbHelper (connection)           │
├─────────────────────────────────────────┤
│            Oracle Database              │
│   Users table  │  DiaryEntries table   │
│   Stored Procedures  │  Sequences       │
└─────────────────────────────────────────┘
```

---

## 🗄️ Database Design (Oracle 11g)

Schema designed from ERD → implemented in Oracle SQL Developer.

```sql
Users (UserID PK, Username UNIQUE, Password, Email, CreatedAt)
  │
  │ 1 ──── ∞
  │
DiaryEntries (EntryID PK, UserID FK, Title, Body CLOB,
              Mood CHECK, EntryDate, CreatedAt)
```

### Stored Procedures

| Procedure | Type | Purpose |
|-----------|------|---------|
| `GetEntriesByUser` | SYS_REFCURSOR | Returns multiple rows for a user |
| `GetEntryByID` | OUT Parameters (NUMBER) | Returns single row without cursor |

---

## 🛠️ Tech Stack

- C# (.NET Framework 4.7.2)
- Windows Forms
- Oracle 11g
- ODP.Net
- BCrypt.Net-Next
- Crystal Reports

---

## 🚀 Running the Project

1. Clone repo
```bash
git clone https://github.com/yourusername/personal-diary-system.git

```
**Prerequisites:**
- Visual Studio 2022 with .NET desktop development workload
- Oracle 11g with scott/tiger user unlocked
- Oracle.ManagedDataAccess NuGet package
- BCrypt.Net-Next NuGet package

2. Run the SQL scripts in Oracle SQL Developer (in order):
```
scripts/01_create_tables.sql
scripts/02_sequences_triggers.sql
scripts/03_stored_procedures.sql
scripts/04_sample_data.sql
```

3. Update the connection string in `Data/DbHelper.cs`:
```csharp
"User Id=scott;Password=tiger;Data Source=localhost:1521/orcl;"
```

4. Build and run in Visual Studio 


## 👨‍💻 Author

Built as part of a Software Engineering course — Phase 2: Implementation.  
Designed from SRS → ERD → Database → Logic → GUI.

> *"Every feature traces back to a requirement. Every query uses bind variables. Every password is hashed."*

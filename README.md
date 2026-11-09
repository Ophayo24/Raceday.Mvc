# RaceDay — Event Management System

Full-stack event management platform for the South African road running, walking, and cycling community.


## Roles
* **Organiser** — create and manage events, categories, enrolments, and results.
* **Participant** — browse events, enter a category, track personal results.

- GitHub Actions CI

## Setup & Run Instructions (Local)

### 1. Database
Ensure LocalDB is running and apply migrations:
```bash
cd Raceday.Api
dotnet ef database update

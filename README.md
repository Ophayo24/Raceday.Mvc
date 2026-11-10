# RaceDay — Part 3

This is the ASP.NET Core MVC front-end application for the RaceDay system. It consumes the Part 2 REST API to provide a clean, role-aware interface for Organisers and Participants.

**Link to Part 2 API Repository:** https://github.com/Ophayo24/RaceDay-Part2

## Features
- **Role-Based Navigation:** Distinct dashboards for Organisers and Participants.
- **Organiser Interface:** Create events (with image uploads), manage categories, view enrolments, and capture results.
- **Participant Interface:** Browse and filter upcoming events, enrol in categories, view personal results, and update profiles (with profile picture uploads).
- **Authentication:** Session management via cookie forwarding to the API.
- **Azure Blob Storage:** Images are uploaded via the API and rendered from Azure.

## Tech Stack
- .NET 8 MVC
- Bootstrap 5 (RaceDay custom styling)
- Typed HttpClient for API communication

## Setup and Run Instructions

### 1. Prerequisites
- Ensure the **Part 2 API** is running locally on `https://localhost:7236`

### 2. Run the MVC App
```bash
cd Raceday.Mvc
dotnet run

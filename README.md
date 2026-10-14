
# FlexGCC Work Request Tracker

A full-stack application to create, track, search, filter, and manage client work requests.

## Technologies Used

### Backend
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- Swagger / OpenAPI

### Frontend
- React
- JavaScript
- Vite
- CSS

## Features

- Create, view, update, and delete work requests
- Search by title, client name, or description
- Filter requests by client, status, priority, and creation date
- Sort results and paginate records
- Add and view notes for individual work requests
- Validate request data
- Handle unexpected API errors

## Project Structure

- `WorkRequest.Api/` — ASP.NET Core Web API
- `work-request-ui/` — React frontend
- `FlexGCCWorkTracker.sln` — Visual Studio solution

## Prerequisites

- .NET 8 SDK
- Node.js and npm
- SQL Server
- Visual Studio 2022 or Visual Studio Code

## Database Configuration

The API uses SQL Server and Entity Framework Core.

1. Create or select a SQL Server database named `FlexGCCWorkTrackerDb`.
2. Configure the `ConnectionStrings:DefaultConnection` value in your local API configuration.
3. Ensure the SQL Server instance name and authentication settings match your environment.
4. Apply the existing Entity Framework Core migrations if the database has not been initialized.

Do not commit passwords, API keys, or other secrets to the repository.

## Run the Backend

Open a terminal in the solution root:

```bash
cd WorkRequest.Api
dotnet restore
dotnet run
```

Open the Swagger URL shown in the terminal to explore and test the API.

For the current local development setup, Swagger has been accessed at:

`https://localhost:7099/swagger/index.html`

The actual URL may differ depending on the launch profile.

## Run the Frontend

Open another terminal in the solution root:

```bash
cd work-request-ui
npm install
npm run dev
```

Open the local frontend URL printed by Vite, commonly:

`http://localhost:5173`

## API Endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/WorkRequests` | List, search, filter, sort, and paginate requests |
| GET | `/api/WorkRequests/{id}` | Get a request by ID |
| POST | `/api/WorkRequests` | Create a request |
| PUT | `/api/WorkRequests/{id}` | Update a request |
| DELETE | `/api/WorkRequests/{id}` | Delete a request |
| GET | `/api/WorkRequests/{workRequestId}/Notes` | Get notes for a request |
| POST | `/api/WorkRequests/{workRequestId}/Notes` | Add a note to a request |

## Testing

- Test API endpoints using Swagger.
- Test the frontend by creating and viewing work requests.
- Verify search, filtering, sorting, and pagination.
- Verify status updates, notes, and error scenarios.
- Confirm that changes are persisted in SQL Server.

## Known Limitations

Document any remaining issues, unimplemented features, or environment-specific configuration requirements before submission.

## AI Assistance

Disclose any AI tools used during development and describe the assistance provided, in accordance with the assignment instructions.

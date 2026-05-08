# Task Management API Design (For Approval)

## 1) Scope and Objective

Build a RESTful Task Management API in `.NET 10` using Clean Architecture and PostgreSQL.

The API must:
- Manage `Task` resources with CRUD operations.
- Secure all task endpoints with JWT authentication.
- Associate each task with a `User`.
- Default task ownership to the authenticated user.
- Allow creating tasks on behalf of another user only when authorization rules permit it.

## 2) Functional Requirements

### Task model
Required fields:
- `id` (primary key)
- `title`
- `description`
- `status` (`Open` or `Closed`)
- `due_date`
- `created_dt`
- `updated_dt`
- `assigned_to` (owner user reference)

### User model
Required user shape for authentication and authorization:
- `id`
- `email`
- `name`
- `password_hash` (stored password hash; plain text password is never stored)
- `role` (`Admin` or `User`)

### API behavior
- `GET` tasks (list and by id)
- `POST` create task
- `PUT/PATCH` update task
- `DELETE` task
- JWT-protected endpoints
- On create:
  - default owner = authenticated user
  - optional `user_id` for on-behalf creation
  - verify permission before creating on behalf

## 3) Proposed Architecture (Clean Architecture)

Projects/layers:
- `HelpDesk.Domain`
  - Entities and domain rules (`TaskItem`, `User`)
  - Repository contracts (`ITaskRepository`, `IUserRepository`)
- `HelpDesk.Application`
  - Use-cases/services (`TaskService`)
  - DTOs + validation + authorization checks
- `HelpDesk.Infrastructure`
  - PostgreSQL persistence (repositories, SQL, mappers)
  - JWT token generation/validation adapters
- `HelpDesk.API`
  - Controllers, middleware, DI, auth policies

Dependency direction:
- API -> Infrastructure -> Application -> Domain

## 4) Domain Model

### Entity: TaskItem
- `Guid Id`
- `string Title`
- `string Description`
- `TaskStatus Status` (`Open`, `Closed`)
- `DateTimeOffset DueDate`
- `DateTimeOffset CreatedDt`
- `DateTimeOffset UpdatedDt`
- `Guid AssignedToUserId`

### Entity: User
- `Guid Id`
- `string Email`
- `string Name`
- `string PasswordHash`
- `UserRole Role` (`Admin`, `User`)

### Domain validations
- `Title` required, max length constraint (proposed: 200)
- `Description` optional with max length (proposed: 2000)
- `DueDate` must be a valid date-time (timezone normalized to UTC)
- `DueDate` must be in the future when creating a task
- `AssignedToUserId` must reference an existing user
- `Email` required and normalized for uniqueness
- Password must satisfy configured complexity policy before hashing
- `PasswordHash` is required for all authenticatable users
- `Role` must be a valid enum value (`Admin` or `User`)

## 5) Authorization and Security Design

JWT claims (minimum):
- `sub` (user id)
- `email`
- `role`

Password management requirements:
- Passwords are never stored or logged in plain text.
- Store only salted and strong one-way password hashes (recommended: Argon2id or bcrypt).
- Password verification occurs during login before issuing JWT.
- Password change/reset operations must replace the hash and update `updated_dt`.

Approved authorization rule for "create on behalf":
- Allowed when requester is an `Admin`.
- Non-admin users may only create tasks for themselves.

Approved access rules:
- Task visibility:
  - `Admin`: can view all tasks.
  - Non-admin: can view only tasks assigned to themselves.
- Update/Delete permissions:
  - `Admin` or task owner may update/delete.
- Reassignment:
  - Only `Admin` can change `assigned_to`.
- Status changes:
  - Only the task owner can change status.

Enforcement location:
- Primary check in Application layer (`TaskService.CreateAsync`)
- Endpoint-level `[Authorize]` plus role-aware policies in API layer

## 6) API Contract (v1)

Base route: `/api/tasks`

### POST `/api/tasks`
Create task.

Request body:
- `title`
- `description`
- `dueDate`
- optional `status` (default `Open`)
- optional `userId` (on-behalf owner)

Behavior:
- If `userId` missing -> set to auth user id
- If `userId` present and differs from auth user:
  - require permission (proposed: Admin)
  - reject with `403` if unauthorized

Responses:
- `201 Created` + created task payload
- `400` validation error
- `401` unauthorized
- `403` forbidden
- `404` if target user does not exist

### GET `/api/tasks`
List tasks (auth required).

Proposed query filters:
- `status`
- `assignedTo`
- `dueBefore`
- `dueAfter`
- pagination: `page`, `pageSize` (default `20`)

Response:
- `200 OK` + list payload

### GET `/api/tasks/{id}`
Get one task by id.

Responses:
- `200 OK`
- `404 Not Found`

### PUT `/api/tasks/{id}`
Replace/update task.

Updatable fields:
- `title`
- `description`
- `dueDate`
- optional reassignment (`assignedToUserId`) for `Admin` only

Authorization:
- `Admin` or owner can execute `PUT`.
- If `assignedToUserId` is provided, requester must be `Admin`.
- If `status` is included in `PUT`, only owner can change it.

### PATCH `/api/tasks/{id}`
Partial update task.

Allowed patchable fields:
- `title`
- `description`
- `dueDate`
- `status` (owner only)
- `assignedToUserId` (Admin only)

Responses:
- `200 OK`
- `400`, `401`, `403`, `404`

### DELETE `/api/tasks/{id}`
Soft-delete task.

Behavior:
- Set `deleted_dt` to current UTC timestamp.
- Exclude soft-deleted tasks from standard list/get responses.
- Keep row for auditability.

Responses:
- `204 No Content`
- `401`, `403`, `404`

## 7) Data Model (PostgreSQL)

### Table: users
- `id uuid primary key`
- `email varchar(320) not null unique`
- `name varchar(200) not null`
- `password_hash varchar(255) not null`
- `role varchar(32) not null` check in (`Admin`, `User`)
- `created_dt timestamptz not null default now()`
- `updated_dt timestamptz not null default now()`

### Table: tasks
- `id uuid primary key`
- `title varchar(200) not null`
- `description varchar(2000) null`
- `status varchar(16) not null` check in (`Open`, `Closed`)
- `due_date timestamptz not null`
- `assigned_to uuid not null references users(id)`
- `created_dt timestamptz not null default now()`
- `updated_dt timestamptz not null default now()`
- `deleted_dt timestamptz null`

Indexes (proposed):
- `idx_tasks_assigned_to`
- `idx_tasks_status`
- `idx_tasks_due_date`
- `idx_tasks_deleted_dt`

## 8) Application Layer Use Cases

`TaskService` responsibilities:
- `CreateTaskAsync(request, actor)`
- `GetTaskByIdAsync(id, actor)`
- `ListTasksAsync(filters, actor)`
- `UpdateTaskAsync(id, request, actor)`
- `DeleteTaskAsync(id, actor)`

Key checks:
- Validate payload
- Validate ownership/role rules
- Validate referenced users on assignment
- Maintain `created_dt` and `updated_dt` lifecycle
- Enforce `due_date` future-only rule on create
- Apply soft-delete behavior and filtering

## 9) Error Handling and Response Standards

Use RFC7807-style problem details:
- Validation errors -> `400`
- Authentication failures -> `401`
- Authorization failures -> `403`
- Missing resources -> `404`
- Unexpected errors -> `500`

## 10) Docker Development and Reload

Per project rule, include Docker Compose for development with reload support.

Proposed services:
- `db` (`postgres`)
- `api` (`dotnet watch run` for hot reload)

Compose artifacts:
- `docker-compose.yml` (base services)
- `docker-compose.dev.yml` (bind mounts + dev overrides)

Key environment values:
- `ConnectionStrings__Default`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`

## 11) Testing Strategy

- Unit tests:
  - Domain validations
  - Authorization rules for on-behalf creation
  - Service-level CRUD logic
- Integration tests:
  - Repository SQL against PostgreSQL container
  - API endpoints with JWT auth
- Security tests:
  - unauthorized and forbidden paths


create table if not exists users (
    id uuid primary key,
    email varchar(320) not null unique,
    name varchar(200) not null,
    password_hash varchar(255) not null,
    role varchar(32) not null check (role in ('Admin', 'User')),
    created_dt timestamptz not null default now(),
    updated_dt timestamptz not null default now()
);

create table if not exists tasks (
    id uuid primary key,
    title varchar(200) not null,
    description varchar(2000) null,
    status varchar(16) not null check (status in ('Open', 'Closed')),
    due_date timestamptz not null,
    assigned_to uuid not null references users(id),
    created_dt timestamptz not null default now(),
    updated_dt timestamptz not null default now(),
    deleted_dt timestamptz null
);

create index if not exists idx_tasks_assigned_to on tasks(assigned_to);
create index if not exists idx_tasks_status on tasks(status);
create index if not exists idx_tasks_due_date on tasks(due_date);
create index if not exists idx_tasks_deleted_dt on tasks(deleted_dt);

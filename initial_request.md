### Goal
Need to create a RESTful api for task management.

### Requirements
A Task should include title, description, status[Open/Closed], and due_date. Additionally add id as primary key, and created_dt, updated_dt, assigned_to (Links to owner user).
A Task belongs to a User.
Assume a simple user model. id, email, name.
Api needs to support Get, Create, Update, Delete.
Api must be secured with a JWT.
Task will normally created as belonging to the auth user, but allows to create task on behalf by providing the user_id, verify permission to create on behalf.

### Tech staff.
Target .net core 10.
Use CleanArchitecture with separated project for layer
	Domain (Entities)
	Application (Use Cases)
	Api (Interface Adapters)
	Infraestructure 
Use TDD, so creating testing scenarios will be part of the process, after design, but before implementation.

Target postgresql database engine.
Use docker for development / testing


### Tasks
1. Create a design document for my revision and approval before implementing any code.
2. Ask me any question required to complete the design document.






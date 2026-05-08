When I start development a new feature for existing project or a new project, the first that i do is to write a file with the ideas / requirements / technical staff, a list of tasks, among other things. This is used as my initial feed to the AI agent to start building a design document. See [initial_request.md](./initial_request.md).

Once I have put everything that I need (or that I can think of at the moment) on the document I ask the agent to read the document and complete the tasks at the end of the document. See [`prompts.md`](./prompts.md) PROMPT #1.

The design phase usually will require some feedback to the agent, to fill gaps, I review the design document top to end, and ask to refine / modify the things I consider so. This is done based on what I need, experience, personal coding style, etc.

When I am ok with the Design document I ask the agent to implement the design and to generate a cursor context file to make it easy for new agents, collaborators to understand and make changes to the project. See [`prompts.md`](./prompts.md) PROMPT #2.

Testing goes on to make sure the things are working as expected.


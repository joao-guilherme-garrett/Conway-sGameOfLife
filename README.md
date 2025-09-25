**Conway's Game of Life API**

This project is a C#/.NET 7 implementation of an API for Conway's Game of Life, built as a response to the InfoStorm code challenge.

The primary focus of this project is to demonstrate best practices in modern software development, including Clean Architecture, Test-Driven Development (TDD), SOLID principles, and creating a "production-ready" service.

**DISCLAIMER**

This challenge was very enjoyable. Due to an urgent call-in for work requiring me to work until the early hours, I had a short timeline to complete the project.

I focused on delivering a straightforward implementation, strictly adhering to the game logic and core requirements while applying my daily coding principles.

I followed good principles and built as I normally build, so you can have a good idea of how I usually think when coding.

Although functional and ready for review, I know there is certainly ample room for improvement. Specifically, I could enhance the current implementation by:

 - Improving modularity and further splitting responsibilities.

 - Optimizing the algorithm (e.g., ignoring dead cells for faster execution in larger flows).

 - Implementing robust safeguards (e.g., resource exhaustion protection and generation limits).

 - Adding more robust input validation for enhanced safety.

 - Including Docker support for easier deployment.

 - And more!

Given the scope of a test challenge, I believe this submission is sufficient for review, but I acknowledge that these improvements would make the solution significantly more robust. Thanks for the opportunity, it was certainly fun!

**END OF DISCLAIMER**

**Features**

Create Boards: Upload an initial board state and receive a unique ID.

Evolve Boards: Advance a board by a single generation.

Advance Multiple Generations: Advance a board by a specified number of generations in a single call.

Find Final State: Automatically simulate the game until the board becomes stable, enters a loop, or times out.

Persistence: Board states are saved in a SQLite database, ensuring they survive application restarts.

**Technology & Architecture**

Framework: ASP.NET Core 7.0

Language: C#

Testing: xUnit

Database: SQLite with Entity Framework Core

_Architecture:_

Clean Architecture: The solution is structured to separate concerns. The core game logic (GameOfLifeService) has no knowledge of the API or the database.

Dependency Injection: Services and the DbContext are managed by the ASP.NET Core DI container, promoting loose coupling.

Repository Pattern (via DbContext): EF Core's DbContext and DbSet act as a generic repository, abstracting data access logic.

DTOs (Data Transfer Objects): We use BoardStateDto to separate our internal domain model (Board) from the public API contract, which is a key security and scalability practice.

**Getting Started**

_Prerequisites_

.NET 7 SDK

Visual Studio 2022 or a compatible IDE

**Running the Application**

Clone the repository.

Open the solution file (GameOfLife.sln) in Visual Studio.

Restore NuGet Packages: Visual Studio should do this automatically. If not, right-click the solution and select "Restore NuGet Packages".

_Set up the Database:_

Open the Package Manager Console (Tools > NuGet Package Manager > Package Manager Console).

Ensure the "Default project" is set to GameOfLife.Api.

Run the command: Update-Database. This will create the gameoflife.db file.

Run the project: Press the run button in Visual Studio. A browser window will open with the Swagger UI, where you can interact with all the API endpoints.

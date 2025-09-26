**Conway's Game of Life API**

This project is a C#/.NET 7 implementation of an API for Conway's Game of Life, built as a response to the InfoStorm code challenge.

The primary focus of this project is to demonstrate best practices in modern software development, including Clean Architecture, Test-Driven Development (TDD), SOLID principles, and creating a "production-ready" service.

**DISCLAIMER**

This challenge was very enjoyable. Due to an urgent call-in for work requiring me to work until the early hours, I had a short timeline to complete the project. Later on I had the time to review it and make small improvements.

I focused on delivering a straightforward implementation, strictly adhering to the game logic and core requirements while applying my daily coding principles.

I followed good principles and built as I normally build, so you can have a good idea of how I usually think when coding.

Upon a second pass of the project, I implemented several key improvements, essentially treating this as a refactoring phase in a broader view (similar to the TDD approach).

Specifically, I improved focused on:

 - Controller Thinning: Extracted business logic from the controller to reduce its responsibilities (following the SRP).

 - Test Enhancements: Improved the existing unit tests and added integration tests for the controller layer.

 - Algorithmic Optimization: Refactored the core game logic to significantly improve performance on larger, sparser boards. The previous approach looped across the entire board (O(height×width)), while the new algorithm now only iterates over living cells and their neighbors.

 - Included Docker support for easier deployment.

 - Added more robust input validation for enhanced safety, preventing cyberattacks like DoS.

Although functional and ready for review, I know there is certainly ample room for improvement. Specifically, I could enhance the current implementation by:

 - Improving modularity and further splitting responsibilities.

 - Optimizing the algorithm even further (e.g., adding a parallel prossessing).

 - Implementing robust safeguards (e.g., resource exhaustion protection and generation limits).

 - Adding more robust input validation for enhanced safety.

 - Also adding Caching, rate limitting, health check endpoint, structured logging, api versioning, use a dedicated validation library like FluentValidation, and more! 

Given the scope of a test challenge, I believe this submission is sufficient for review, but I acknowledge that these improvements would make the solution significantly more robust. Thanks for the opportunity, it was certainly fun!

**END OF DISCLAIMER**

**Features**

_Create Boards:_ Upload an initial board state to the API.

_Calculate Next Generation:_ Get the next state for a given board.

_Advance Multiple Generations:_ Get the state of a board after x number of generations.

_Find Final State:_ Automatically find the final, stable state of a board. The API detects both static and oscillating (looping) patterns.

_Persistence:_ Board states are saved in a SQLite database, so they persist across application restarts.

_Interactive API Documentation:_ A Swagger UI is integrated for easy testing and exploration of the API endpoints.

**Getting Started**

You can run this project in one of two ways: using the .NET SDK directly or using Docker.

**Option 1: Running with .NET (Recommended for Development)**

Prerequisites

_.NET 7 SDK_

_An IDE like Visual Studio 2022 or VS Code_

**Setup Instructions**

Clone the repository:

		git clone [https://github.com/your-username/Conway-sGameOfLife.git](https://github.com/your-username/Conway-sGameOfLife.git)
		cd Conway-sGameOfLife

Build the solution:

		dotnet build

Set up the database:

This command will apply the Entity Framework migrations and create the gameoflife.db SQLite database file.

		dotnet ef database update --project src/GameOfLife.Api

Run the application:

		dotnet run --project src/GameOfLife.Api

Access the API:

Navigate to https://localhost:7123/swagger in your web browser to view and interact with the API documentation.

**Option 2: Running with Docker**

Prerequisites

_Docker Desktop_

**Setup Instructions**

Clone the repository:

		git clone [https://github.com/your-username/Conway-sGameOfLife.git](https://github.com/your-username/Conway-sGameOfLife.git)
		cd Conway-sGameOfLife

Build the Docker image:

From the root directory (where the Dockerfile is located), run the following command. This will build the application inside a container.

		docker build -t gameoflife-api .

Run the Docker container:

This command will start the container and map port 8080 on your local machine to port 80 inside the container.

		docker run -p 8080:80 gameoflife-api

Access the API:

Navigate to http://localhost:8080/swagger in your web browser to view and interact with the API documentation.

**Architectural Choices**

This project was built with a focus on creating a clean, maintainable, and scalable architecture.

_Clean Architecture:_ The project is structured to separate concerns into distinct layers: API (Controllers), Business Logic (Services), and Data Access (Repositories).

_Dependency Injection (DI):_ Services and repositories are registered in the ASP.NET Core service container and injected where needed, promoting loose coupling.

_Repository Pattern:_ A repository layer (IBoardRepository) is used to abstract the data persistence logic from the rest of the application.

_Data Transfer Objects (DTOs):_ DTOs are used to separate the internal domain model (Board) from the public-facing API contract.

_Unit & Integration Testing:_ The solution includes both unit tests for the core service logic and integration tests for the API endpoints to ensure correctness and reliability.
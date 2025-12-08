# Text-Based Adventure Game & Security API

This project contains a classic text-based adventure game and a secure ASP.NET Core Web API for user authentication. The solution is divided into multiple projects, including the game, the API, and testing utilities.

---

## 1. Text-Based Adventure Game

The text-based adventure game is a console application where players explore a world, interact with objects, and fight monsters. The game requires authentication against the Web API to play.

### 1.1 How to Run the Game

Before running the game, ensure the **Web API is already running**, as the game needs to connect to it for authentication.

To start the game, navigate to the project's root directory in your terminal and run the following command:

```bash
dotnet run --project security-testing-project/security-testing-project.csproj
```

### 1.2 In-Game Registration and Login

When the game starts, you must first **register** a new user or **login** with an existing one.

1.  **Register**: Choose the "Register" option. You will be prompted to enter a new username, password, and an optional role (`Player` or `Admin`). Since the API uses an in-memory store, you will need to register a new user each time the API restarts.
2.  **Login**: After registering, or if you already have a user from the current API session, choose the "Login" option and enter your credentials.

Once authenticated, the adventure begins.

### 1.3 Game Mechanics

The game is played by typing commands into the console. The core game loop reads player input, executes the corresponding action, and updates the game state in real-time.

#### Player Commands

| Command             | Description                                                   |
| ------------------- | ------------------------------------------------------------- |
| `go <direction>`    | Moves the player to the room in the specified direction.      |
| `take <item>`       | Adds an item from the current room to the player's inventory. |
| `look`              | Displays the description of the current room and inventory.   |
| `inventory`         | Shows only the items currently in the player's inventory.     |
| `fight`             | Initiates combat with a monster in the current room.          |
| `help`              | Lists all available commands.                                 |
| `quit`              | Exits the game.                                               |

#### Winning and Losing

- **Win Condition**: The game is won by entering the **"Treasure Room,"** which requires a key for access.
- **Game Over Conditions**:
  1.  Entering a room where `IsDeadly` is `true`.
  2.  Engaging in combat with a monster without a `Weapon`.
  3.  Attempting to move out of a room while a monster is present.

---

## 2. Web API for User Authentication

The `API` project is a secure ASP.NET Core Web API that provides user registration and login functionality using JWT (JSON Web Tokens).

### 2.1 Setup and Configuration

Follow these steps to get the API running on your local machine.

#### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
- A code editor like Visual Studio, VS Code, or JetBrains Rider.

#### Step 1: Configure the JWT Secret

The application requires a secret key to sign and validate JWTs. This key must be configured before running the API.

1.  Navigate to the `API` directory:
    ```bash
    cd API
    ```
2.  Create a new file named `appsettings.Development.json`. This file is listed in `.gitignore`, so your secret key will not be committed to the repository.

3.  Add the following JSON to the file, replacing `"YOUR_SUPER_SECRET_KEY_GOES_HERE"` with a strong, unique secret:

    ```json
    {
      "JWT_SECRET_KEY": "YOUR_SUPER_SECRET_KEY_GOES_HERE"
    }
    ```

### 2.2 How to Run the API

1.  Open your terminal and navigate to the root of the project.
2.  Run the following command:

    ```bash
    dotnet run --project API/API.csproj
    ```
3.  The API will start, and the console will display the URL where it is running (e.g., `https://localhost:7058`).

### 2.3 API Tutorial: Authentication and Usage

The API includes interactive documentation (Swagger) to help you test the endpoints. Once the API is running, open your browser and navigate to the Swagger UI at the URL displayed in the console (e.g., `https://localhost:7058/swagger`).

#### Step 1: Register a New User

The API uses an in-memory user store, so you will need to register a new user each time you restart the application.

1.  In the Swagger UI, find the `POST /api/Auth/register` endpoint and expand it.
2.  Click **"Try it out"**.
3.  In the request body, enter a `username`, `password`, and `role`. The password must be at least 6 characters long.

    ```json
    {
      "username": "testuser",
      "password": "password123",
      "role": "Player"
    }
    ```
4.  Click **"Execute"**. You should receive a `200 OK` response confirming the user was registered successfully.

#### Step 2: Log In and Get a JWT

Next, log in with the newly created user to obtain a JWT.

1.  Find the `POST /api/Auth/login` endpoint and expand it.
2.  Click **"Try it out"**.
3.  Enter the `username` and `password` you just registered.
4.  Click **"Execute"**. The response body will contain a JWT.

#### Step 3: Access a Protected Endpoint

Now you can use the JWT to access endpoints that require authentication.

1.  **Copy the entire `token`** from the login response.
2.  At the top right of the Swagger page, click the **"Authorize"** button.
3.  In the dialog that appears, type **`Bearer`** followed by a space, and then paste your token.
4.  Click **"Authorize"** and then **"Close"**.
5.  Now, find the `GET /api/Auth/me` endpoint, expand it, and click **"Execute"**. You should receive a `200 OK` response with the current user's details.

---

## 3. Testing

The project includes a dedicated test project, `SecurityProject.MSTests`, for ensuring code quality and correctness through unit, integration, and behavior-driven tests.

---

## 4. Encryption Tool

The `EncryptionTool` project is a utility for encrypting and decrypting room files using a hybrid encryption scheme with X.509 certificates.

### How to Use

#### Generate a Certificate
```bash
dotnet run --project EncryptionTool/EncryptionTool.csproj -- generate-cert <output-path> <password>
```

#### Encrypt a File
```bash
dotnet run --project EncryptionTool/EncryptionTool.csproj -- encrypt <inputFile> <outputFile> <cert-path>
```

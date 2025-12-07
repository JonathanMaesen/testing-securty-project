# Text-Based Adventure Game Documentation

Made by Sander De Moor, Nils Mertens, & Jonathan Maesen

GitHub: https://github.com/JonathanMaesen/testing-securty-project
---

## 1. General Structure & Basic Program

### 1.1 World Structure

The game world is a dynamic environment constructed from a network of interconnected **`Room`** objects. This design allows for complex and expandable maps that players can explore.

#### 1.1.1 Rooms

A `Room` is the fundamental building block of the game world. Each room has the following characteristics:

- **Name and Description**: A title and descriptive text that informs the player about their surroundings.
- **Exits**: Connections to other rooms, defined by a `Direction` (*Up*, *Down*, *Left*, *Right*).
- **Items**: A room can contain one or more `Item` objects that the player can pick up.
- **Monster**: A room may be inhabited by a `Monster`.
- **Special Properties**:
  - `IsDeadly`: If `true`, entering the room results in immediate death.
  - `RequiresKey`: If `true`, the room is initially locked and requires a `Key` type item to enter.
  - `IsEncrypted`: If `true`, the room's content is encrypted and requires decryption via the API and a passphrase to reveal its secrets (see Security section 3.6 for details).
- **Dynamic Descriptions**: Rooms can have alternate descriptions that are displayed under specific conditions:
  - `DescriptionWhenEmpty`: Shown after an item has been taken from the room.
  - `DescriptionWhenMonsterDefeated`: Shown after a monster in the room has been defeated.

#### 1.1.2 World Composition

The entire game world is instantiated in the `Program.cs` file within the `CreateTestWorld` method. This centralizes the world-building process and is responsible for:
1.  Instantiating all `Room` objects.
2.  Populating rooms with `Item` and `Monster` objects.
3.  Connecting the rooms by defining their `Exits`.
4.  Setting the player's starting room.

### 1.2 Game Mechanics

The game is played by typing commands into the console. The core game loop reads player input, executes the corresponding action, and updates the game state in real-time.

#### 1.2.1 Player Commands

| Command             | Description                                                   |
| ------------------- | ------------------------------------------------------------- |
| `go <direction>`    | Moves the player to the room in the specified direction.      |
| `take <item>`       | Adds an item from the current room to the player's inventory. |
| `look`              | Displays the description of the current room and inventory.   |
| `inventory`         | Shows only the items currently in the player's inventory.     |
| `fight`             | Initiates combat with a monster in the current room.          |
| `help`              | Lists all available commands.                                 |
| `quit`              | Exits the game.                                               |

#### 1.2.2 Combat

- Combat is initiated in one of two ways:
  1.  The player explicitly uses the `fight` command.
  2.  The player enters a room that contains a `Monster`.
- **Winning Combat**: If the player has a `Weapon` type item in their inventory, they will defeat the monster.
- **Losing Combat**: If the player does not have a weapon, they will die, and the game will be over.
- **Fleeing**: Attempting to leave a room while an undefeated monster is present also results in a game over.

#### 1.2.3 Winning and Losing

- **Win Condition**: The game is won by entering the **"Treasure Room,"** which requires a key for access.
- **Game Over Conditions**:
  1.  Entering a room where `IsDeadly` is `true`.
  2.  Engaging in combat with a monster without a `Weapon`.
  3.  Attempting to move out of a room while a monster is present.
- **Restart Mechanism**: Upon a "Game Over," the game automatically restarts after a short delay, creating a fresh world instance.

#### 1.2.4 Console Experience

- **Clear Interface**: The console clears after every command to provide a clean and focused view of the current game state.
- **Error Handling**: If an unknown command is entered, the game displays an error message and then redisplays the current room's description, so the player never loses context.

### 1.3 Class Overview

The project follows an object-oriented design, with each class having a distinct and well-defined responsibility.

- **`Program.cs`**: The main entry point of the application. It contains the primary game loop, handles command-line input, and initiates the game world.

- **`World.cs`**: The core of the game engine. It manages the game state, including the collection of all rooms and the player's current location. It implements the logic for all player actions.

- **`Room.cs`**: Defines a single location in the game. It holds its own description, exits, items, and an optional monster, and is responsible for generating its own descriptive text.

- **`Player.cs`**: Represents the player character, holding their `Inventory`.

- **`Monster.cs`**: Represents a hostile creature. It has a name and a status indicating whether it `IsAlive`.

- **`Item.cs`**: Defines an object that can be found and picked up. Items have a `Name`, `Description`, and `ItemType` (e.g., `Weapon`, `Key`).

- **`Inventory.cs`**: The `PlayerInventory` class manages the items the player is carrying and provides helper methods to check for specific item types.

- **`Direction.cs`**: Defines the possible directions of movement (*Up*, *Down*, *Left*, *Right*), ensuring type safety for room connections.

- **`console-interfacer.cs`**: Contains the `CommandManager`, a generic utility for registering and processing text-based commands. It handles parsing user input and dispatching it to the correct action.

### 1.4 How to Run the Game

This is a .NET console application. To start the game, navigate to the project's root directory in your terminal and run the following command:

```bash
dotnet run --project security-testing-project/security-testing-project.csproj
```

---

## 2. Testing

The project's primary focus is on robust testing to ensure code quality and correctness. Our testing strategy is divided into three layers:

### 2.1 Unit Testing

Unit tests are used to validate individual components in isolation. We focused on testing the core logic of classes like `PlayerInventory`, `Room`, and `Item` to ensure their methods behave as expected. For example, we verified that `PlayerInventory.HasItemType()` correctly identifies items and that a `Room` correctly updates its description after a monster is defeated.

### 2.2 Integration Testing

Integration tests check the collaboration between different classes. We created tests to simulate player actions and verify the entire game state updates correctly. A key test case was ensuring that moving between rooms (`Player` and `World` interaction) and picking up an item (`Player`, `Room`, and `Item` interaction) works seamlessly.

### 2.3 Behavior-Driven Testing (BDD)

We used a BDD approach with Gherkin syntax to test end-to-end player scenarios. These tests describe a user's journey from a feature-level perspective, making them easy to understand for everyone. Our BDD tests cover the main win/loss conditions:
- **Winning the game**: A scenario where the player finds the key, defeats the monster, and reaches the treasure room.
- **Losing the game**: Scenarios for dying in a deadly room, fighting a monster without a weapon, or trying to flee from a monster.

---

## 3. Security

This project includes a web API for user authentication and key sharing, which is secured using JWT (JSON Web Tokens).

### 3.1 API Project

The `API` project is an ASP.NET Core Web API that provides the following functionalities:
- User registration and login.
- Secure sharing of keys between users.

### 3.2 JWT Authentication

The API uses JWT-based authentication to secure its endpoints. When a user logs in, the API generates a JWT containing claims about the user. This token must be included in the `Authorization` header of subsequent requests to access protected resources.

### 3.3 Configuration

The JWT signing key is a critical secret and must be configured correctly for the API to run.

#### 3.3.1 JWT Secret Key

The application expects a `JWT_SECRET_KEY` in its configuration. Instead of hardcoding this key, the application reads it from the configuration provider.

For development, the secret key is stored in `API/appsettings.Development.json`. This file is included in the `.gitignore` to prevent committing secrets to the repository.

To run the API in a development environment, you must have an `appsettings.Development.json` file in the `API` directory with the following structure:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "JWT_SECRET_KEY": "YOUR_SUPER_SECRET_KEY_GOES_HERE"
}
```

### 3.4 How to Run the API

To run the API, navigate to the project's root directory and execute the following command:

```bash
dotnet run --project API/API.csproj
```

The API will start, and you can access its endpoints, which are documented using Swagger. The Swagger UI is available at the URL displayed in the console upon startup (e.g., `https://localhost:7058/swagger`).

### 3.5 Creating a User and Logging In

Since the application uses an in-memory user store, you will need to register a new user each time you restart the API.

1.  **Run the API** using the command above.
2.  **Open the Swagger UI** in your browser.
3.  **Register a new user**:
    *   Navigate to the `POST /api/Auth/register` endpoint.
    *   Click "Try it out".
    *   Enter a username, password, and role (`Player` or `Admin`) in the request body. Note that the role comparison is case-insensitive, so "admin" will also work.
    *   Click "Execute". You should receive a success response.
4.  **Log in**:
    *   Navigate to the `POST /api/Auth/login` endpoint.
    *   Click "Try it out".
    *   Enter the username and password you just registered.
    *   Click "Execute". You will receive a JWT token in the response body. This token can be used to authenticate to secured endpoints.

#### 3.5.1 Authorizing Requests in Swagger UI

To access protected endpoints (like `/api/Auth/me`) after logging in, you need to provide your JWT token to Swagger UI so it can include it in the request headers.

1.  **Copy the JWT token** (the long string) from the response body of a successful login.
2.  Click the **"Authorize"** button at the **top right of the Swagger page**.
3.  A dialog will pop up. In the "Value" text box, you **must type the word `Bearer` followed by a space**, and then paste your copied token. It should look like this (do not add any extra quotes):

    ```
    Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    ```

4.  Click "Authorize" in the dialog, then "Close".
5.  Now, you can execute protected endpoints like `/api/Auth/me`, and they should return a `200 OK` response with the user's details.

### 3.6 Encrypted Rooms

The game features encrypted rooms whose content is not directly visible until successfully decrypted. This uses a symmetric AES-256 encryption scheme, where the decryption key is derived dynamically from a combination of a "keyshare" provided by the API and a user-supplied "passphrase".

#### 3.6.1 Encryption Tool Usage

The `EncryptionTool` project is used to encrypt plaintext room files into `.enc` files.

To encrypt a room file, navigate to the project's root directory and run the following command:

```bash
dotnet run --project EncryptionTool/EncryptionTool.csproj -- <inputFile> <outputFile> <keyshare> <passphrase>
```

-   `<inputFile>`: The path to the plaintext file (e.g., `room_secret.txt`).
-   `<outputFile>`: The desired path for the encrypted output file (e.g., `room_secret.enc`).
-   `<keyshare>`: A secret string that will be combined with the passphrase to form the encryption key. This should correspond to a keyshare served by the API (e.g., `SecretKeyShare123ForRoom1`).
-   `<passphrase>`: A secret passphrase that will be combined with the keyshare.

**Example:**
```bash
dotnet run --project EncryptionTool/EncryptionTool.csproj -- EncryptionTool/room_secret.txt EncryptionTool/room_secret.enc SecretKeyShare123ForRoom1 open_sesame
```

The tool uses **AES-256 in CBC mode with PKCS7 padding**. The output `.enc` file contains the **16-byte Initialization Vector (IV) prepended to the ciphertext**.

#### 3.6.2 Client-Side Decryption

When the player attempts to enter an encrypted room:

1.  The game prompts the player to **enter a passphrase**.
2.  The client makes an authenticated API call to `GET /api/keys/keyshare/{roomId}` to retrieve the specific `keyshare` for that room. This API endpoint validates the player's JWT and role.
3.  The client then dynamically generates the **decryption key** by computing `SHA256(keyshare + ":" + passphrase)`.
4.  Using this derived key, along with the IV extracted from the `.enc` file, the client decrypts the room's content.
5.  If decryption is successful, the room's hidden content is revealed; otherwise, an appropriate error message is displayed.

---
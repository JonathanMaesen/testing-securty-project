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

#### Encrypted Rooms

Some rooms in the game are encrypted. To enter them, you need to complete a security challenge:
1.  When you try to enter an encrypted room, the game will automatically contact the API to fetch a **Keyshare**.
2.  You will then be prompted to enter a **Passphrase**.
3.  The game combines the Keyshare and your Passphrase to derive a decryption key.
4.  Finally, you must provide the path to your personal certificate file (`.pfx`), which is protected by this derived key.

If the derived key correctly unlocks your certificate, the room will be decrypted, and you can enter.

---

## 2. Web API for User Authentication

The `API` project is a secure ASP.NET Core Web API that provides user registration, login, and keyshare distribution functionality using JWT.

### 2.1 Setup and Configuration

#### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

#### Step 1: Configure the JWT Secret

1.  Navigate to the `API` directory.
2.  Create a file named `appsettings.Development.json`.
3.  Add the following JSON, replacing the placeholder with a strong secret:
    ```json
    {
      "JWT_SECRET_KEY": "YOUR_SUPER_SECRET_KEY_GOES_HERE"
    }
    ```

### 2.2 How to Run the API

1.  Open your terminal and navigate to the root of the project.
2.  Run the command:
    ```bash
    dotnet run --project API/API.csproj
    ```

---

## 3. Encryption Tool (For Developers)

The `EncryptionTool` is a command-line utility for developers to create the encrypted room files (`.enc`) and their corresponding certificates (`.pfx`).

### 3.1 The Decryption Key Explained

The password for the certificate (`.pfx` file) is not a simple string. It is **derived** from two pieces of information:
- A **Keyshare** (provided by the API, e.g., `"Share_For_Players_123"`)
- A **Passphrase** (a secret known by the player, e.g., `"apple"`)

The final password is the `SHA256` hash of these two values combined with a colon: `SHA256(Keyshare + ":" + Passphrase)`.

### 3.2 How to Create Encrypted Rooms

Here is the developer workflow for creating a new encrypted room.

#### Step 1: Define Your Secrets

Decide on the `roomId`, `keyshare`, and `passphrase` for your new room.
- **Room ID**: `room_secret`
- **Keyshare**: `Share_For_Players_123`
- **Passphrase**: `apple`

#### Step 2: Calculate the Certificate Password

You need to calculate the SHA256 hash of `"Share_For_Players_123:apple"`. You can use any online or local tool to do this. The result will be a long hexadecimal string (e.g., `3A7BD3E2...`).

#### Step 3: Create the Certificate and Encrypted File

Use the `encrypt` command in the `EncryptionTool`. It will automatically generate the certificate if it doesn't exist.

- `<inputFile>`: Your plaintext room content (e.g., `room_secret.txt`).
- `<outputFile>`: The destination for the encrypted content (e.g., `room_secret.enc`).
- `<cert-path>`: The path for the certificate to be created (e.g., `certs/room_secret.cer`).
- `[cert-password]`: The **derived SHA256 hash** you calculated in Step 2.

**Example Command:**
```bash
dotnet run --project EncryptionTool/EncryptionTool.csproj -- encrypt room_secret.txt room_secret.enc certs/room_secret.cer 3A7BD3E2...
```

This command will:
1.  See that `certs/room_secret.cer` does not exist.
2.  Generate `certs/room_secret.cer` (public key) and `certs/room_secret.pfx` (private key), using the SHA256 hash as the password for the `.pfx` file.
3.  Encrypt `room_secret.txt` into `room_secret.enc` using the new certificate.

You can then distribute the `.pfx` file to the player and place the `.enc` file in the game's output directory.

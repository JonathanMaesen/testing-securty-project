# Text Adventure Game Tutorial

This tutorial will guide you through setting up and playing the text adventure game.

## Introduction

The project consists of three parts:
1.  **A Text Adventure Game:** A C# console application where you can explore a world, solve puzzles, and fight monsters.
2.  **An API:** A C# web API that provides user authentication and "key shares" for the game.
3.  **An Encryption Tool:** A C# console application that encrypts the content of certain rooms in the game.

The goal of the game is to find the treasure. To do this, you will need to decrypt the content of two special rooms: the "Secret Vault" and the "Admin Sanctum".

## Setup & Installation

You will need the .NET SDK installed to run this project.

### 1. Run the API

The API is responsible for user authentication and providing key shares.

1.  Open a terminal and navigate to the `API` directory.
2.  Run the command `dotnet run`.

The API will start listening on a specific address (e.g., `http://localhost:5263`). Make a note of this address.

### 2. Run the Encryption Tool

The Encryption Tool creates the encrypted files that the game will use. **This step is crucial.**

1.  Open a new terminal and navigate to the `EncryptionTool` directory.
2.  Run the command `dotnet run`.
3.  This will create several files, including:
    *   `room_secret.enc`
    *   `room_admin.enc`
    *   `CMS_Encryption_Key.pfx` (the certificate with the private key)
4.  Copy the `.enc` files (`room_secret.enc` and `room_admin.enc`) to the `security-testing-project/bin/Debug/net8.0` directory.
5.  Keep the `CMS_Encryption_Key.pfx` file somewhere safe. You will need it to play the game.

### 3. Run the Game Client

This is the main game.

1.  Open a new terminal and navigate to the `security-testing-project` directory.
2.  Run the command `dotnet run`.

The game will start, and you will be prompted to log in or register.

## How to Play

### Basic Commands

*   `look`: Shows the description of the current room, items in the room, and the exits.
*   `inventory`: Shows your inventory.
*   `go <direction>`: Moves you to a different room (e.g., `go up`).
*   `back`: Go back to the previous room.
*   `take <item>`: Picks up an item (e.g., `take key`).
*   `fight`: Fights a monster in the current room.
*   `quit`: Exits the game.

### The Encrypted Rooms and the Decryption Process

There are two encrypted rooms in the game: the **Secret Vault** and the **Admin Sanctum**. When you try to enter these rooms, the decryption process will be initiated.

Here's how it works:

1.  **Login/Register:** You must first log in to the game. You can also register a new user. To access the "Admin Sanctum", you need to be an "Admin". You can specify the role during registration.

2.  **Get the Key Share:** When you try to enter an encrypted room, the game will automatically contact the API to get a "key share". You need the correct role ("Player" for the Secret Vault, "Admin" for the Admin Sanctum) to get the key share.

3.  **Enter the Passphrase:** The game will then ask you for your "personal Passphrase". This is a secret that you need to know. For this project, the passphrase is: `TheQuickBrownFox`

4.  **Derive the Certificate Password:** The game will use the key share and the passphrase to derive the password for the certificate. The formula is: `SHA256(keyshare + ":" + passphrase)`.

5.  **Enter the Certificate Path:** The game will then ask you for the path to your certificate file (`.pfx`). This is the `CMS_Encryption_Key.pfx` file that was created by the `EncryptionTool`. You need to provide the full path to this file.

6.  **Decryption:** If the certificate password is correct and the certificate is valid, the game will decrypt the content of the room and you will be able to enter.

### The Goal

The goal of the game is to find the treasure. The decrypted message in the "Secret Vault" will give you a hint about where to find the treasure. Good luck!

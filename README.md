# Text-Based Adventure Game Documentation

Made by Sander De Moor, Nils Mertens, & Jonathan Maesen

---

## 1. Project Overview

This project is a text-based adventure game with a focus on security features. It consists of three main components:

*   **Text Adventure Game:** A C# console application that serves as the game client.
*   **API:** A C# web API that manages user authentication and provides "key shares" for in-game puzzles.
*   **Encryption Tool:** A C# console application that encrypts game content using a certificate.

---

## 2. Security Features

The core of this project is a security-themed puzzle that requires the player to decrypt the content of two special rooms: the **Secret Vault** and the **Admin Sanctum**.

### 2.1 The API: Authentication and Key Shares

The API handles user registration and login, using JWT for authentication. It also provides "key shares" which are necessary for the decryption process. Access to certain key shares is restricted by user roles ("Player" and "Admin").

### 2.2 The Encryption Tool: Content Encryption

The `EncryptionTool` encrypts room descriptions using a combination of AES and RSA encryption. It generates a `.pfx` certificate file containing a private key, and `.enc` files containing the encrypted content.

### 2.3 The Decryption Process

To decrypt the rooms, the player must:
1.  Authenticate with the API to get a key share.
2.  Provide a secret passphrase.
3.  Use the key share and passphrase to derive the password for the certificate.
4.  Provide the path to the certificate file.

This multi-step process demonstrates a more secure approach to handling secrets in an application.

---

## 3. How to Run the Game

This project consists of a text adventure game, an API, and an encryption tool. For detailed instructions on how to set up and run all three parts, please see the [Tutorial.MD](Tutorial.MD) file.

---

## 4. Class Overview

The project follows an object-oriented design, with each class having a distinct and well-defined responsibility.

- **`Program.cs`**: The main entry point of the application. It contains the primary game loop, handles command-line input, and initiates the game world.
- **`World.cs`**: The core of the game engine. It manages the game state and implements the logic for all player actions, including the decryption flow.
- **`ApiService.cs`**: Handles all communication with the API, including authentication and fetching key shares.
- **`CryptoHelper.cs`**: Contains the logic for decrypting the room content using the certificate and derived password.
- **`Room.cs`**: Defines a single location in the game.
- **`Player.cs`**: Represents the player character.
- **`Item.cs`**: Defines an object that can be found and picked up.

---

## 5. Testing Approach

The project includes unit, integration, and behavior-driven tests to ensure the quality and correctness of the code. The tests cover everything from individual components to end-to-end player scenarios.

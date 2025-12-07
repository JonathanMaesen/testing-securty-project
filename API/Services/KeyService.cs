using API.Models;

namespace API.Services
{
    public class KeyService : IKeyService
    {
        private readonly Dictionary<string, string> _keyShares = new Dictionary<string, string>
        {
            { "room_secret", "SecretKeyShare123ForRoom1" },
            { "room_treasure", "TreasureKeyShare456ForRoom2" },
            { "room_admin", "AdminOnlyKeyShare789ForRoom3" }
        };

        private readonly Dictionary<string, string> _roomRoleRequirements = new Dictionary<string, string>
        {
            { "room_secret", "Player" },      // Iedereen mag deze
            { "room_treasure", "Player" },    // Iedereen mag deze
            { "room_admin", "Admin" }         // Alleen Admin
        };

        public KeyShareResponse? GetKeyShare(string roomId, string userRole)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(roomId))
                return null;

            // Check of room bestaat
            if (!_keyShares.ContainsKey(roomId))
                return null;

            // Check of user de juiste rol heeft
            if (_roomRoleRequirements.TryGetValue(roomId, out var requiredRole))
            {
                if (requiredRole == "Admin" && userRole != "Admin")
                {
                    return null; // Niet geautoriseerd
                }
            }

            // Return keyshare
            return new KeyShareResponse
            {
                RoomId = roomId,
                KeyShare = _keyShares[roomId]
            };
        }
    }

}

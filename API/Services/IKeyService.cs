using API.Models;

namespace API.Services
{
    public interface IKeyService
    {
        KeyShareResponse? GetKeyShare(string roomId, string userRole);
    }
}

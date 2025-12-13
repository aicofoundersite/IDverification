using CrossSetaWeb.Models;

namespace CrossSetaWeb.Services
{
    public interface IUserService
    {
        void RegisterUser(UserModel user, string password);
    }
}

using Supabase.Gotrue;

namespace OrdexIn.Services
{
    public interface IAppSignInService
    {
        Task SignInAsync(Session session);
        Task SignOutAsync();
    }
}
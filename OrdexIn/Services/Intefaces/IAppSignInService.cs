using Supabase.Gotrue;

namespace OrdexIn.Services.Intefaces
{
    public interface IAppSignInService
    {
        Task SignInAsync(Session session);
        Task SignOutAsync();
    }
}
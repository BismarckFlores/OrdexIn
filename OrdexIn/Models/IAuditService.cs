using OrdexIn.Models;

namespace OrdexIn.Services
{
    public interface IAuditService
    {
        void Registrar(AuditLogModel log);
    }
}

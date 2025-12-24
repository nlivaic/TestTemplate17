using System.Threading.Tasks;

namespace TestTemplate17.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveAsync();
}
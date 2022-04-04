namespace Infrastructure;

public interface IUnitOfWork
{
    Task Commit();
}

namespace Domain;

public interface IUnitOfWork
{
    Task Commit();
}

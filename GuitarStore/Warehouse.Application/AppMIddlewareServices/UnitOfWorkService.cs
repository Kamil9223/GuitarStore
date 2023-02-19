using Domain;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.AppMIddlewareServices;

internal class UnitOfWorkService : IUnitOfWorkService
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Commit()
    {
        await _unitOfWork.Commit();
    }
}

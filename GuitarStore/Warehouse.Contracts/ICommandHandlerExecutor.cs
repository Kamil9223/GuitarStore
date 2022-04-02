using Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Contracts;

public interface ICommandHandlerExecutor<TCommand> where TCommand : ICommand
{
    Task Execute(TCommand command);
}

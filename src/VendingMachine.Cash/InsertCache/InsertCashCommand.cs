using MediatR;

namespace VendingMachine.Cash.InsertCache;

public sealed record InsertCashCommand(decimal Amount) : IRequest<Unit>;

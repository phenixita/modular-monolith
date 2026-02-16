using MediatR;

namespace VendingMachine.Cash;

internal sealed record InsertCashCommand(decimal Amount) : IRequest;

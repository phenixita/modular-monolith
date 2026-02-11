using MediatR;

namespace VendingMachine.Cash;

public sealed record InsertCashCommand(decimal Amount) : IRequest;

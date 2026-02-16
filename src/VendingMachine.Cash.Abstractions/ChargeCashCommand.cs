using MediatR;

namespace VendingMachine.Cash;

public sealed record ChargeCashCommand(decimal Amount) : IRequest<Unit>;

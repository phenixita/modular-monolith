using MediatR;

namespace VendingMachine.Cash.Charge;

public sealed record ChargeCashCommand(decimal Amount) : IRequest<Unit>;

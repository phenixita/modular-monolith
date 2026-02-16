using MediatR;

namespace VendingMachine.Cash;

internal sealed record ChargeCashCommand(decimal Amount) : IRequest;

using MediatR;

namespace VendingMachine.Cash;

internal sealed record GetBalanceQuery() : IRequest<decimal>;

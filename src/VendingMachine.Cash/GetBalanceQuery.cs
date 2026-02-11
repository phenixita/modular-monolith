using MediatR;

namespace VendingMachine.Cash;

public sealed record GetBalanceQuery() : IRequest<decimal>;

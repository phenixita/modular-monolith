using MediatR;

namespace VendingMachine.Cash.GetBalance;

public sealed record GetBalanceQuery() : IRequest<decimal>;

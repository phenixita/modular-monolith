using MediatR;

namespace VendingMachine.Cash.RefundAll;

public sealed record RefundAllCommand() : IRequest<decimal>;

using MediatR;

namespace VendingMachine.Cash;

public sealed record RefundAllCommand() : IRequest<decimal>;

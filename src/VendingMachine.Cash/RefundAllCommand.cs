using MediatR;

namespace VendingMachine.Cash;

internal sealed record RefundAllCommand() : IRequest<decimal>;

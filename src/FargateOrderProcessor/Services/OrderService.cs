using AWS.Messaging;
using FargateOrderProcessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateOrderProcessor.Services
{
    public class OrderService : IMessageHandler<CreateOrderCommand>
    {
        public Task<MessageProcessStatus> HandleAsync(MessageEnvelope<CreateOrderCommand> messageEnvelope, CancellationToken token = default)
        {
            Console.WriteLine($"Received message {messageEnvelope.Message.Order.OrderId} from Publisher");

            return Task.FromResult(MessageProcessStatus.Success());
        }
    }
}

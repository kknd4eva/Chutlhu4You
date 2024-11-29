using Amazon.CDK.AWS.SQS;
using Amazon.CDK;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateApiCdk.Constructs
{
    public class MessageQueue : Construct
    {
        public Queue Queue { get; private set; }

        public MessageQueue(Construct scope, string id) : base(scope, id)
        {
            // create our sqs queue that will be used for publishing messages
            Queue = new Queue(this, "FargateApi-SqsQueue", new QueueProps
            {
                QueueName = "PublishSale",
                Encryption = QueueEncryption.KMS_MANAGED,
                DeliveryDelay = Duration.Seconds(10), // simulate delay in processing
            });
        }
    }
}

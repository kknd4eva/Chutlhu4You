using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;
using FargateApiCdk;
using FargateApiCdk.Constructs;
using FargateApiCdk.Props;
using System;

namespace FargateApi
{
    public class FargateApiStack : Stack
    {
        private IVpc GetVpc(string vpcId)
        {
            return Vpc.FromLookup(this,
                "AlbSgVpc",
                new VpcLookupOptions
                {
                    VpcId = vpcId
                });
        }

        internal FargateApiStack(Construct scope, string id, ApiStackProps apiStackProps, IStackProps props = null) : base(scope, id, props)
        {
            var vpc = GetVpc(apiStackProps.VpcId);

            var messageQueue = new MessageQueue(this, "MessageQueue");
            var table = new Persistence(this, "Persistence");

            var cluster = new FargateCluster(this, "FargateCluster", new FargateClusterProps
            {
                Vpc = vpc,
                SqsQueue = messageQueue.Queue,
                SessionTable = table.SessionTable,
                WebShopTable = table.WebShopTable,
                EcrRepositoryName = apiStackProps.EcrRepositoryName
            });

            var publicLoadBalancer = new PublicLoadBalancer(this, "PublicLoadBalancer", new PublicLoadBalancerProps
            {
                Vpc = vpc,
                CertificateArn = apiStackProps.CertificateArn,
                WebService = cluster.WebService,
                ApiService = cluster.ApiService,
                FargateSecurityGroup = cluster.FargateServiceSecurityGroup
            });

            // output sqs queue url
            new CfnOutput(this, "SqsQueueUrl", new CfnOutputProps
            {
                Value = messageQueue.Queue.QueueUrl
            });
        }
    }
}

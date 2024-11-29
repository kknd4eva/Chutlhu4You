using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SQS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateApiCdk.Props
{
    public class FargateClusterProps
    {
        public IVpc Vpc { get; set; }
        public Queue SqsQueue { get; set; }
        public Table WebShopTable { get; set; }
        public Table SessionTable { get; set; }
        public string EcrRepositoryName { get; set; }  

    }
}

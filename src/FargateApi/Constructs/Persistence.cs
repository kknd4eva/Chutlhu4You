using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateApiCdk.Constructs
{
    internal class Persistence : Construct
    {
        public Table WebShopTable { get; private set; }
        public Table SessionTable { get; private set; }

        public Persistence(Construct scope, string id) : base(scope, id)
        {
            SessionTable = (new Table(this, "SessionTable", new TableProps
            {
                TableName = "session_table",
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "sort",
                    Type = AttributeType.STRING
                },
                TimeToLiveAttribute = "cache_ttl",               
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            }));

            WebShopTable = (new Table(this, "WebShopTable", new TableProps
            {
                TableName = "webshop_table",
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "sort",
                    Type = AttributeType.STRING
                },
                TimeToLiveAttribute = "cache_ttl",
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            }));
        }
    }
}

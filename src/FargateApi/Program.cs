using Amazon.CDK;
using FargateApi;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = Amazon.CDK.Environment;

namespace FargateApiCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            var vpcId = app.Node.TryGetContext("VPC_ID") as string;
            var certificateArn = app.Node.TryGetContext("CERTIFICATE_ARN") as string;
            var ecrRepo = app.Node.TryGetContext("ECR_REPO") as string;
            var account = app.Node.TryGetContext("ACCOUNT") as string;
            var region = app.Node.TryGetContext("REGION") as string;

            new FargateApiStack(app, "FargateApiStack", new ApiStackProps
            {
                VpcId = vpcId,
                CertificateArn = certificateArn,
                EcrRepositoryName = ecrRepo
            },
            new StackProps
            {
                Env = new Environment
                {
                    Account = account,
                    Region = region
                }
            });
            app.Synth();
        }
    }
}

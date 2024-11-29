using Amazon.CDK;
using FargateApiCdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = Amazon.CDK.Environment;

namespace FargateApi
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            var vpcId = app.Node.TryGetContext("VPC_ID") as string;
            var certificateArn = app.Node.TryGetContext("CERTIFICATE_ARN") as string; 
            var ecrRepo = app.Node.TryGetContext("ECR_REPO") as string; 

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
                    Account = "153247006570",
                    Region = "ap-southeast-2"
                }
            });
            app.Synth();
        }
    }
}

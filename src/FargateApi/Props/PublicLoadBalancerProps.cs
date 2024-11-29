using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateApiCdk.Props
{
    public class PublicLoadBalancerProps
    {
        public IVpc Vpc { get; set; }
        public string CertificateArn { get; set; }
        public FargateService WebService { get; set; }
        public FargateService ApiService { get; set; }

        public SecurityGroup FargateSecurityGroup { get; set; }

    }
}

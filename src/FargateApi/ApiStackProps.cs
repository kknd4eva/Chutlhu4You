using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateApiCdk
{
    public class ApiStackProps : Amazon.CDK.IStackProps
    {
        public string VpcId { get; set; }
        public string CertificateArn { get; set; }
        public string EcrRepositoryName { get; set; }
    }
}

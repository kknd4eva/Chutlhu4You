using Amazon.CDK.AWS.EC2;
using Amazon.CDK;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FargateApiCdk.Props;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.ServiceDiscovery;
using Amazon.CDK.AWS.EKS;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.Route53;

namespace FargateApiCdk.Constructs
{
    public class PublicLoadBalancer : Construct
    {
        public PublicLoadBalancer(Construct scope, string id, PublicLoadBalancerProps props) : base(scope, id)
        {
            var AlbSecurityGroup = new SecurityGroup(this, "FargateApi-ALB-SecurityGroup", new SecurityGroupProps
            {
                Vpc = props.Vpc
            });

            // allow any traffic on 443 from the internet
            AlbSecurityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(443));

            // allow traffic on port 80 only to the fargate security group
            AlbSecurityGroup.AddIngressRule(props.FargateSecurityGroup, Port.Tcp(8080));

            var alb = new ApplicationLoadBalancer(this, "FargateApi-ALB", new ApplicationLoadBalancerProps
            {
                Vpc = props.Vpc,
                InternetFacing = true,
                SecurityGroup = AlbSecurityGroup
            });

            var listener = alb.AddListener("FargateApi-Listener", new BaseApplicationListenerProps
            {
                Port = 443,
                Open = true,
                Certificates = new[] { ListenerCertificate.FromArn(props.CertificateArn) }
            });

            // API Target Group
            var apiTargetGroup = listener.AddTargets("FargateApi-TargetGroup", new AddApplicationTargetsProps
            {
                Port = 8080, // Assuming API container runs on 8080
                Targets = new IApplicationLoadBalancerTarget[] { props.ApiService },
                HealthCheck = new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
                {
                    Path = "/health",
                    Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol.HTTP
                }
            });

            // Web Target Group
            var webTargetGroup = listener.AddTargets("FargateWeb-TargetGroup", new AddApplicationTargetsProps
            {
                Port = 8080,
                Targets = new IApplicationLoadBalancerTarget[] { props.WebService },
                HealthCheck = new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
                {
                    Path = "/api/local/health",
                    Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol.HTTP
                }
            });


            // add the certificate
            listener.AddCertificates("FargateApi-Certificate", 
                new[] { ListenerCertificate.FromArn(props.CertificateArn) });

            listener.AddAction("ApiListenerRule", new AddApplicationActionProps
            {
                Priority = 2,
                Conditions = new[]
                {
                    ListenerCondition.HostHeaders(new[] { "api.earlgreyhot.org" })
                },
                Action = ListenerAction.Forward(new[] { apiTargetGroup })
            });

            listener.AddAction("WebListenerRule", new AddApplicationActionProps
            {
                Priority = 1,
                Conditions = new[]
                {
                    ListenerCondition.HostHeaders(new[] { "store.earlgreyhot.org" })
                },
                Action = ListenerAction.Forward(new[] { webTargetGroup })
            });

            // set up a friendly DNS name for the ALB
            var hostedZone = Amazon.CDK.AWS.Route53.HostedZone.FromLookup(this, "FargateApi-HostedZone", new Amazon.CDK.AWS.Route53.HostedZoneProviderProps
            {
                DomainName = "earlgreyhot.org",
                PrivateZone = false
            });

            new ARecord(this, "FargateApi-ARecord", new ARecordProps
            {
                Zone = hostedZone,
                RecordName = "api",
                Target = RecordTarget.FromAlias(new LoadBalancerTarget(alb)) // ALB target for the subdomain
            });

            // ARecord for Web subdomain
            new ARecord(this, "FargateWeb-ARecord", new ARecordProps
            {
                Zone = hostedZone,
                RecordName = "store",
                Target = RecordTarget.FromAlias(new LoadBalancerTarget(alb)) // ALB target for the subdomain
            });
        }
    }

}

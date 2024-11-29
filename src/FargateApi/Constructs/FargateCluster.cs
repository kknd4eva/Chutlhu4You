using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK;
using Constructs;
using FargateApiCdk.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CDK.AWS.ApplicationAutoScaling;
using LogGroupProps = Amazon.CDK.AWS.Logs.LogGroupProps;

namespace FargateApiCdk.Constructs
{
    public class FargateCluster : Construct
    {
        public FargateService WebService { get; private set; }
        public FargateService ApiService { get; private set; }
        public FargateService WorkerService { get; private set; }
        public SecurityGroup FargateServiceSecurityGroup { get; private set; }

        public FargateCluster(Construct scope, string id, FargateClusterProps props) : base(scope, id)
        {
            FargateServiceSecurityGroup = new SecurityGroup(this, "FargateApi-Service-SecurityGroup", new SecurityGroupProps
            {
                Vpc = props.Vpc,
                AllowAllOutbound = true
            });


            // create the ECS cluster
            var cluster = new Cluster(this, "FargateApi-Cluster", new ClusterProps
            {
                ContainerInsights = false,
                Vpc = props.Vpc,
            });

            // create the execution role
            var executionRole = new Role(this, "FargateApi-ExecRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
            });

            // create a task role that allows publishing to the sqs queue
            var apiTaskRole = new Role(this, "FargateApi-TaskRole", new RoleProps
            {
                Description = "Task role for FargateApi",
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
            });

            var webTaskRole = new Role(this, "FargateApi-WebTaskRole", new RoleProps
            {
                Description = "Task role for FargateWeb",
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
            });

            var workerTaskRole = new Role(this, "FargateApi-WorkerTaskRole", new RoleProps
            {
                Description = "Task role for FargateOrderProcessor",
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
            });

            // Define the inline policy
            var sqsSendPolicy = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "sqs:SendMessage" },
                        Effect = Effect.ALLOW,
                        Resources = new[] { props.SqsQueue.QueueArn }
                    })
                }
            });

            var sqsReceiveAndDeletePolicy = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "sqs:ReceiveMessage", "sqs:DeleteMessage" },
                        Effect = Effect.ALLOW,
                        Resources = new[] { props.SqsQueue.QueueArn }
                    })
                }
            });

            var dynamoDbSessionPolicy = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] {
                            "dynamodb:DescribeTable",
                            "dynamodb:PutItem", 
                            "dynamodb:GetItem", 
                            "dynamodb:DeleteItem", 
                            "dynamodb:UpdateItem" },
                        Effect = Effect.ALLOW,
                        Resources = new [] {props.SessionTable.TableArn}
                    })
                }
            });

            var dynamoDbShopPolicy = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] {
                            "dynamodb:DescribeTable",
                            "dynamodb:PutItem",
                            "dynamodb:GetItem",
                            "dynamodb:DeleteItem",
                            "dynamodb:UpdateItem" },
                        Effect = Effect.ALLOW,
                        Resources = new [] {props.WebShopTable.TableArn}
                    })
                }
            });


            // Attach the inline policy to the role
            apiTaskRole.AttachInlinePolicy(new Policy(this, "SQSPublishPolicy", new PolicyProps
            {
                Document = sqsSendPolicy
            }));
            apiTaskRole.AttachInlinePolicy(new Policy(this, "dynamoDbShopPolicy", new PolicyProps
            {
                Document = dynamoDbShopPolicy
            }));

            // attach the receive policy to the worker task role
            workerTaskRole.AttachInlinePolicy(new Policy(this, "SQSReceivePolicy", new PolicyProps
            {
                Document = sqsReceiveAndDeletePolicy
            }));

            webTaskRole.AttachInlinePolicy(new Policy(this, "DbPolicy", new PolicyProps
            {
                Document = dynamoDbSessionPolicy
            }));

            executionRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonECSTaskExecutionRolePolicy"));

            var taskDefinition = new FargateTaskDefinition(this,
                "FargateApi-TaskDefinition",
                new FargateTaskDefinitionProps
                {
                    MemoryLimitMiB = 512,
                    Cpu = 256,
                    ExecutionRole = executionRole,
                    TaskRole = apiTaskRole
                });

            var workerTaskDefinition = new FargateTaskDefinition(this,
                "Fargate-WorkerTaskDefinition",
                new FargateTaskDefinitionProps
                {
                    MemoryLimitMiB = 512,
                    Cpu = 256,
                    ExecutionRole = executionRole,
                    TaskRole = workerTaskRole
                });

            var webTaskDefinition = new FargateTaskDefinition(this,
                "Fargate-WebTaskDefinition",
                new FargateTaskDefinitionProps
                {
                    MemoryLimitMiB = 512,
                    Cpu = 256,
                    ExecutionRole = executionRole,
                    TaskRole = webTaskRole
                });

            var workerLogGroup = new LogGroup(this, "FargateApi-WorkerLogGroup",
                new LogGroupProps
                {
                    LogGroupName = "/ecs/FargateOrderProcessor",
                    Retention = RetentionDays.ONE_WEEK,
                    RemovalPolicy = RemovalPolicy.DESTROY
                });

            // create a log group
            var logGroup = new LogGroup(this, "FargateApi-LogGroup",
                new LogGroupProps
                {
                    LogGroupName = "/ecs/FargateApi",
                    Retention = RetentionDays.ONE_WEEK,
                    RemovalPolicy = RemovalPolicy.DESTROY
                });

            // web log group
            var webLogGroup = new LogGroup(this, "FargateApi-WebLogGroup",
                new LogGroupProps
                {
                    LogGroupName = "/ecs/FargateWeb",
                    Retention = RetentionDays.ONE_WEEK,
                    RemovalPolicy = RemovalPolicy.DESTROY
                });

            // log driver for sending docker logs to cloudwatch
            var logDriver = new AwsLogDriver(new AwsLogDriverProps
            {
                LogGroup = logGroup,
                StreamPrefix = "FargateApi"
            });

            var workerLogDriver = new AwsLogDriver(new AwsLogDriverProps
            {
                LogGroup = workerLogGroup,
                StreamPrefix = "FargateOrderProcessor"
            });

            var webLogDriver = new AwsLogDriver(new AwsLogDriverProps
            {
                LogGroup = webLogGroup,
                StreamPrefix = "FargateWeb"
            });

            // create the web container definition
            var webContainer = webTaskDefinition.AddContainer("FargateWeb-Container",
                new ContainerDefinitionOptions
                {
                    Image = ContainerImage.FromRegistry(props.EcrRepositoryName.Replace("api", "web")),
                    Logging = webLogDriver
                });

            webContainer.AddPortMappings(new PortMapping
            {
                ContainerPort = 8080
            });

            // create the worker container definition
            var workerContainer = workerTaskDefinition.AddContainer("FargateOrderProcessor-Container",
                new ContainerDefinitionOptions
                {
                    Image = ContainerImage.FromRegistry(props.EcrRepositoryName.Replace("api", "worker")),
                    Logging = workerLogDriver,
                    HealthCheck = new Amazon.CDK.AWS.ECS.HealthCheck
                    {
                        Command = new[] { "CMD-SHELL", "curl -f http://localhost:8080/health || exit 1" },
                        Interval = Duration.Seconds(30),
                        Timeout = Duration.Seconds(5),
                        Retries = 3
                    },
                    Environment = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "SQS_PUBLISH_QUEUE", props.SqsQueue.QueueUrl }
                    }
                });

            workerContainer.AddPortMappings(new PortMapping
            {
                ContainerPort = 8080
            });

            // create the container definition
            var container = taskDefinition.AddContainer("FargateApi-Container",
                new ContainerDefinitionOptions
                {
                    Image = ContainerImage.FromRegistry(props.EcrRepositoryName),
                    Logging = logDriver,
                    HealthCheck = new Amazon.CDK.AWS.ECS.HealthCheck
                    {
                        Command = new[] { "CMD-SHELL", "curl -f http://localhost:8080/health || exit 1" },
                        Interval = Duration.Seconds(30),
                        Timeout = Duration.Seconds(5),
                        Retries = 3
                    },
                    Environment = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "SQS_PUBLISH_QUEUE", props.SqsQueue.QueueUrl }
                }
                });

            container.AddPortMappings(new PortMapping
            {
                ContainerPort = 8080
            });

            // create the web service
            WebService = new FargateService(this, "FargateWeb-Service",
                new FargateServiceProps
                {
                    Cluster = cluster,
                    AssignPublicIp = false,
                    DesiredCount = 1,
                    TaskDefinition = webTaskDefinition,
                    SecurityGroups = new ISecurityGroup[] { FargateServiceSecurityGroup },
                    PlatformVersion = FargatePlatformVersion.LATEST
                });

            // create the service
            ApiService = new FargateService(this, "FargateApi-Service",
                new FargateServiceProps
                {
                    Cluster = cluster,
                    AssignPublicIp = false,
                    DesiredCount = 1,
                    TaskDefinition = taskDefinition,
                    SecurityGroups = new ISecurityGroup[] { FargateServiceSecurityGroup },
                    PlatformVersion = FargatePlatformVersion.LATEST
                });

            WorkerService = new FargateService(this, "FargateOrderProcessor-Service",
                new FargateServiceProps
                {
                    Cluster = cluster,
                    AssignPublicIp = false,
                    DesiredCount = 1,
                    TaskDefinition = workerTaskDefinition,
                    SecurityGroups = new ISecurityGroup[] { FargateServiceSecurityGroup },
                    PlatformVersion = FargatePlatformVersion.LATEST
                });

            var scaling = WebService.AutoScaleTaskCount(new EnableScalingProps
            {
                MaxCapacity = 2,
                MinCapacity = 1
            });

            scaling.ScaleOnCpuUtilization("CpuScaling", new CpuUtilizationScalingProps
            {
                TargetUtilizationPercent = 5,
                ScaleInCooldown = Duration.Seconds(60),
                ScaleOutCooldown = Duration.Seconds(60)
            });

            props.Vpc.AddGatewayEndpoint("DynamoDbEndpoint", new GatewayVpcEndpointOptions
            {
                Service = GatewayVpcEndpointAwsService.DYNAMODB
            });


        }
    }
}

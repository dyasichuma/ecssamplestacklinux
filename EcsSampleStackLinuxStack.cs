using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.AutoScaling;


namespace EcsSampleStackLinux
{
    public class EcsSampleStackLinuxStack : Stack
    {
        internal EcsSampleStackLinuxStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var vpc = new Vpc(this , "myvpc", new VpcProps
            {
                Cidr = "10.0.0.0/16"
            });
       
            var ecs_cluster = new Cluster(this, "Cluster", new ClusterProps{
                Vpc = vpc,
                ClusterName = $"{this.StackName}-Cluster",
            });
            var autoScalingGroup = new AutoScalingGroup(this, "ASG", new AutoScalingGroupProps {
                Vpc = vpc,
                InstanceType = InstanceType.Of(InstanceClass.COMPUTE4, InstanceSize.XLARGE),
                MachineImage = EcsOptimizedImage.AmazonLinux2(),
                MinCapacity = 1,
                MaxCapacity =1,
            });
            var capacityProvider = new AsgCapacityProvider(this, "AsgCapacityProvider", new AsgCapacityProviderProps {
                AutoScalingGroup = autoScalingGroup,
                CapacityProviderName = "ASGcapacityprovider",
                EnableManagedTerminationProtection = false
            });
            
            ecs_cluster.AddAsgCapacityProvider(capacityProvider);

            var managedpolicyforECSTask = ManagedPolicy.FromManagedPolicyArn(this, "ManagedECSTaskPolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy");

            var ecstaskrole = new Role(this, "ECSTaskExecutionRole1", new RoleProps
                { 
                    RoleName = "ecsTaskExecutionRole1",
                    ManagedPolicies = new [] {managedpolicyforECSTask},
                    AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });
          
            var taskDefinition = new Ec2TaskDefinition(this, "TaskDef", new Ec2TaskDefinitionProps
            {
                TaskRole = ecstaskrole,
                NetworkMode = NetworkMode.NAT
            });

            taskDefinition.AddContainer("mycdkDefaultContainer", new ContainerDefinitionOptions {
                Image = ContainerImage.FromRegistry("httpd:2.4"),
                MemoryLimitMiB = 2048,
                Cpu = 1024,
                PortMappings = new [] { new PortMapping {ContainerPort = 80}},       
                Logging = LogDrivers.AwsLogs(new AwsLogDriverProps { StreamPrefix = "api" } )
                });

            var AppLBservice= new ApplicationLoadBalancedEc2Service(this, "Service", new ApplicationLoadBalancedEc2ServiceProps {
                Cluster = ecs_cluster,
                TaskDefinition = taskDefinition,
                });

            var lbsg = new SecurityGroup(this, "LBSG", new SecurityGroupProps{
                Vpc = vpc
            });

            AppLBservice.LoadBalancer.AddSecurityGroup(lbsg);           
            autoScalingGroup.Connections.AllowFrom(lbsg.Connections, Port.AllTraffic());
            lbsg.AddEgressRule(Peer.AnyIpv4(), Port.AllTraffic());
        }
    }
}

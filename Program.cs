using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsSampleStackLinux
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new EcsSampleStackLinuxStack(app, "EcsSampleStackLinuxStack", new StackProps
            {

                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }

                // Uncomment the next block if you know exactly what Account and Region you
                // want to deploy the stack to.
                /*
                Env = new Amazon.CDK.Environment
                {
                    Account = "123456789012",
                    Region = "us-east-1",
                }
                */

                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            });
            app.Synth();
        }
    }
}

import * as cdk from 'aws-cdk-lib';
import * as events from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as secrets from 'aws-cdk-lib/aws-secretsmanager';
// import { checkovSkip } from '@diligentcorp/checkov-helper';
import { Construct } from 'constructs';
import { ProdDbSecretName, VpcId, ProdVpcSubnetId, TestVpcSubnetId, TestDbSecretName } from '../constants';
import { getBranchName, getLastCommit } from '../utils';
import { NotificationLambdaProps } from '../config';

export class NotificationStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: NotificationLambdaProps) {
    super(scope, id, props);

    const vpc = ec2.Vpc.fromLookup(this, 'Vpc', { vpcId: VpcId });
    const vpcSubnets = props?.isProduction 
                              ? { subnets: [ec2.Subnet.fromSubnetId(this, 'Subnet', ProdVpcSubnetId)] }
                              : { subnets: [ec2.Subnet.fromSubnetId(this, 'Subnet', TestVpcSubnetId)] }
    
    const dbResourceArn = props?.isProduction 
                                ? secrets.Secret.fromSecretNameV2(this, id, ProdDbSecretName).secretArn
                                : secrets.Secret.fromSecretNameV2(this, id, TestDbSecretName).secretArn;
                                
    const DbSecretObjectKey = props?.isProduction 
                                    ? ProdDbSecretName
                                    : TestDbSecretName;


    const notificationLambda = new lambda.Function(this, 'NotificationLambda', {
      runtime: lambda.Runtime.DOTNET_6,
      handler: 'NotificationTablePopulationLambda::NotificationTablePopulationLambda.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../Lambdas/ChangeLogLambdas/NotificationTablePopulationLambda/', {
        bundling: {
          image: lambda.Runtime.DOTNET_6.bundlingImage,
          user: "root",
          outputType: cdk.BundlingOutput.ARCHIVED,
          command: [
            "/bin/sh",
            "-c",
            " dotnet tool install -g Amazon.Lambda.Tools" +
            " && dotnet build" +
            " && dotnet lambda package --output-package /asset-output/function.zip"
          ]
        }
      }),
      environment: { DbSecretObjectKey },
      timeout: cdk.Duration.minutes(4),
      vpcSubnets,
      vpc: vpc,
      reservedConcurrentExecutions: 1,
      allowPublicSubnet: true,
      memorySize: 256
    });

    const secretManagerPolicy = new iam.PolicyStatement();
    secretManagerPolicy.addActions("secretsmanager:DescribeSecret", "secretsmanager:GetSecretValue");
    secretManagerPolicy.addResources(`${dbResourceArn}-*`);

    notificationLambda.addToRolePolicy(secretManagerPolicy);

    cdk.Tags.of(notificationLambda).add("git-branch-name", getBranchName());
    cdk.Tags.of(notificationLambda).add("git-latest-commit", getLastCommit());

    const rule = new events.Rule(this, 'Schedule Rule', {
      schedule: events.Schedule.rate(cdk.Duration.minutes(5)),
      targets: [
        new targets.LambdaFunction(notificationLambda)
      ]
    });

    // checkovSkip(cdk.Stack.of(this), { type: 'AWS::Lambda::Function' }, [
    //   { id: 'CKV_AWS_116', comment: 'No need for DLQ because lambda is triggered from scheduler' },
    //   { id: 'CKV_AWS_173', comment: 'No need for encryption because at the moment of writing the lambda does not have sensitive data' },
    // ]);
  }
}
import * as cdk from 'aws-cdk-lib';
import * as events from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as secrets from 'aws-cdk-lib/aws-secretsmanager';
// import { checkovSkip } from '@diligentcorp/checkov-helper';
import { Construct } from 'constructs';
import { DomainSecretName, VpcName } from '../constants';
import { getBranchName, getLastCommit } from '../utils';

export class NotificationStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // const vpc = ec2.Vpc.fromLookup(this, 'Vpc', {
    //   vpcName: VpcName,
    // });

    const vpc = ec2.Vpc.fromLookup(this, 'RdsVpc', { vpcId: 'vpc-0a8c8e78a8b705c2e' });

    const notificationLambda = new lambda.Function(this, 'NotificationLambda', {
      runtime: lambda.Runtime.DOTNET_6,
      handler: 'NotificationTablePopulationLambda::NotificationTablePopulationLambda.Function::FunctionHandler',
      // handler: 'notification-lambda::notification_lambda.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../Lambdas/ChangeLogLambdas/NotificationTablePopulationLambda/', {
      // code: lambda.Code.fromAsset('../Lambdas/SampleLambda/src/notification-lambda', {
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
      environment: { DbSecretObjectKey: DomainSecretName },
      timeout: cdk.Duration.minutes(2),
      // vpcSubnets: {
      //   subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
      // },
      vpc: vpc,
      reservedConcurrentExecutions: 1
    });

    const dbResourceArn = secrets.Secret.fromSecretNameV2(this, id, DomainSecretName).secretArn;

    const secretManagerPolicy = new iam.PolicyStatement();
    secretManagerPolicy.addActions("secretsmanager:DescribeSecret", "secretsmanager:GetSecretValue");
    secretManagerPolicy.addAllResources();

    notificationLambda.addToRolePolicy(secretManagerPolicy);

    cdk.Tags.of(notificationLambda).add("git-branch-name", getBranchName());
    cdk.Tags.of(notificationLambda).add("git-latest-commit", getLastCommit());

    const rule = new events.Rule(this, 'Schedule Rule', {
      schedule: events.Schedule.rate(cdk.Duration.minutes(35)),
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
import * as cdk from 'aws-cdk-lib';
import * as events from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as secrets from 'aws-cdk-lib/aws-secretsmanager';
// import { checkovSkip } from '@diligentcorp/checkov-helper';
import { Construct } from 'constructs';
import { AccuvioDEVVpcId, ProdDbSecretName, TestDbSecretName, VpcId, VpcSubnetIdTest } from '../constants';
import { getBranchName, getLastCommit } from '../utils';
import { EncryptedSecret, ToolsSecretsKMSKey } from '@diligentcorp/library-cdk-secret';
import { Key } from 'aws-cdk-lib/aws-kms';

export class NotificationStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // const vpc = ec2.Vpc.fromLookup(this, 'Vpc', {
    //   vpcName: VpcName,
    // });
    // const name = 'NotificationLamda';
    // const stageName = 'TEST';
    // new ToolsSecretsKMSKey(this, `${stageName}-SecretsEncryptionKey`, {
    //   keyProps: {
    //     alias: `alias/${name}-${stageName}`,
    //   },
    //   workloadAWSAccountId: '392912010766',
    // });

    // const kmsSecretKey = new Key(this, `AppSecretEncryptionKey`, {
    //   alias: `alias/${name}-${stageName}`,
    //   enableKeyRotation: true, // Rotate the key every year - Compliance requirement
    //   removalPolicy: cdk.RemovalPolicy.DESTROY, // Remove the key when the stack is destroyed, Retain is default
    //   pendingWindow: cdk.Duration.days(7), // Allow 7 days for key deletion
    // });


    // const mySecret = new EncryptedSecret(this, 'my-secret', {
    //   secretProps: {
    //     description: 'A Super Secret Secret for NotificationLambda',
    //     encryptionKey: kmsSecretKey, // `kmsSecretKey` is a KMS Key from your application stack - Required for Compliance
    //   },
    //   cipherText: 'AQICAHhFR8...',
    //   kmsKeyArn: 'arn:aws:kms:eu-west-1:392912010766:key/a4ec2319-07c2-4fd5-af64-7e8b19fb5366',
    // });

    const vpc = ec2.Vpc.fromLookup(this, 'RdsVpc', { vpcId: VpcId });
    // const vpc = ec2.Vpc.fromLookup(this, 'RdsVpc', { vpcId: AccuvioDEVVpcId });

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
      environment: { DbSecretObjectKey: TestDbSecretName },
      timeout: cdk.Duration.minutes(2),
      vpcSubnets: { subnets: [ec2.Subnet.fromSubnetId(this, 'Subnet', VpcSubnetIdTest)] },
      vpc: vpc,
      reservedConcurrentExecutions: 1,
      allowPublicSubnet: true,
    });

    const dbResourceArn = secrets.Secret.fromSecretNameV2(this, id, TestDbSecretName).secretArn;

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
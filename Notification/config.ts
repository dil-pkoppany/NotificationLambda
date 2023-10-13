import * as cdk from 'aws-cdk-lib';

export interface NotificationLambdaProps extends cdk.StackProps {
    isProduction: boolean;
  }
#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { NotificationStack } from '../lib/notification-stack';

const app = new cdk.App();
new NotificationStack(app, 'Devlim3NotificationStack', {
  env: { account: '737374578928', region: 'eu-west-1',  },
  isProduction: false
});

new NotificationStack(app, 'EcoNotificationStack', {
  env: { account: '737374578928', region: 'eu-west-1',  },
  isProduction: true
});
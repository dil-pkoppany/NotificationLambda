# ChangeLog Lambdas

This solution contains AWS Lambdas required for the ChangeLog feature.

At the moment there are following lambdas:
- NotificationTablePopulationLambda - required for populating 
`Notifcations` table based on the entries appeared in `ChangeLogNotifcation` table
([JIRA link](https://diligentbrands.atlassian.net/browse/ESG-4596))

## How to run (tested on Windows 10)

In order to locally run Lambda make sure you have installed:
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [AWS Toolkit for Visual Studio](https://aws.amazon.com/visualstudio/)

After installing and setting up the required tooling, in the Lambda project directory
find and modify two configuration files by your needs:
- `aws-lambda-tools-defaults.json` - contains default configuration settings for launching Lambda. If you are using x86_64 machine
you probably won't need to modify it
- `launchSettings.json` - stored in `Properties` directory and contains information
required for local development. Make sure to fill all the environment variables with needed values there as otherwise
Lambda won't work properly. Below you can find [documentation for each required environment variable](#environment-variables-description)

Finally, open the solution with Visual Studio 2022 and choose the startup project
if it is not detected automatically. If AWS Toolkit setup correctly it should identify the project
as the Lambda project and use `Mock Lambda Test Tool` for launching it locally.

### Environment variables description
- NotificationTablePopulationLambda:
    - `IsDevelopment` - flag that shows whether the Lambda is running in a local envrionemnt. If set to `true` will use envrionment
    variables instead of AWS Secrets Manager for retrieving database connection string.
    - `DevelopmentDbConnectionString` - when `IsDevelopment` is set to `true` this value will be used as the database connection string
    - `DbSecretObjectKey` - value that is used as a key for retrieving database connection string from AWS Secrets Manager 
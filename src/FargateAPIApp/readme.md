# FargateAPIApp

FargateAPIApp is a .NET 8.0 application designed to process orders using AWS Fargate. This application demonstrates how to set up a simple API with logging, authorization, and integration with AWS services.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Building](#building)
- [Endpoints](#endpoints)

## Prerequisites

- [.NET 8.0 SDK]
- [Docker](https://www.docker.com/get-started)
- AWS account with necessary permissions
- Auth0 account

## Configuration
You will need to be able to provide the following values for environment variables
- SQS_PUBLISH_QUEUE when running locally (the name of the queue to publish messages to)
- Auth0 Domain and Audience (for JWT validation) via https://auth0.com/

## running-the-application
To run locally, you can either run the application directly or use Docker. 

## Building
- Build the application using `dotnet build`
- Build the Docker image using `docker buildx build -t api .`
- Tag the image using `docker tag api:latest <account_id>.dkr.ecr.<region>.amazonaws.com/api:latest`
- Push the image to ECR using `docker push <account_id>.dkr.ecr.<region>.amazonaws.com/api:latest`
- Run the CDK deployment using the project FargateApiCdk, and provide the necessary values for the stack

`$ cdk deploy --require-approval never FargateApiStack -c VPC_ID=vpc-xxxxxxxxxxxxxxxx -c CERTIFICATE_ARN=arn:aws:acm:ap-southeast-2:<account_id>:certificate/xxxxxxxxxxxxxxxxxxxx -c ECR_REPO=<account_id>.dkr.ecr.ap-southeast-2.amazonaws.com/dotnet/api:latest`

- Once you have an image in ECR and your stack deployed, you can push new images to ECR and update the service using the following command:

`$ aws ecs update-service --cluster <cluster_name> --service <service_name> --force-new-deployment`

## Endpoints 
Please use the provided .http file to test the endpoints, or alternatively you can run the Swagger UI by navigating to the root URL of the application.
To obtain a token in order to test the deployed API, you can use the following command:
`
$ curl --request POST --url https://dev-xxxxxx.us.auth0.com/oauth/token   --header 'content-type: application/json'   --data '{"client_id":"xxxxxxxxxxxxxxxxxx","client_secret":"xxxxxx-xxxxx-xxxxxx","audience":"https://xxxxxxxx/","grant_type":"client_credentials"}'
`

    
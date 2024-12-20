# Cthulhu4You Demo

[![Cthulhu4You](https://store.earlgreyhot.org/images/logo.png)](https://store.earlgreyhot.org/images/logo.png)

This repository is a .NET 8.0 application designed to provide a basic reference project for a number of different .NET and AWS technologies. The basic premise is a front end and back end for a 'Cthulhu4You' web store, selling all the latest arcane goodies for the eldritch Lovecraftian horror in your life.

Within the solution you'll find the following components: 
* A Razor pages webshop with simple browse and checkout facilities including DockerFile
* A backend REST API combining vertical slice architecture and ASP.NET Minimal APIs to handle products and orders including DockerFile
* Use of the [AWS Messageing Framework for .NET](https://github.com/awslabs/aws-dotnet-messaging), which provides a great set of tools for managing event based/pubsub messaging using services like SQS. 
* Use of Auth0 for machine-to-machine OAUTH2.0 authentication (for the backend API)
* OrderProcessor and Payment (Payment TBC) microservices for handling downstream processe, including DockerFiles. 
* A complete CDK IaC project which will deploy these resources into a well structured set of Fargate containers, including load balancing, health checks and more. 

## Table of Contents

- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Building](#building)
- [Endpoints](#endpoints)

## Prerequisites

- [.NET 8.0 SDK]
- [Docker](https://www.docker.com/get-started)
- Auth0 account
- An AWS account that contains:
  - Appropriate permissions to deploy (including CDK)
  - An Elastic Container Repository (ECR) for the container images the CDK will require
  - a VPC with a public subnet (for your load balancer) and a private subnet for your webshop, API and other resources. <i>(Remember you can quick-create an appropriate VPC using the AWS console if you don't already have one set up.)</i> 


## Configuration
You will need to be able to provide the following values for environment variables
- SQS_PUBLISH_QUEUE when running locally (the name of the queue to publish messages to)
- Auth0 Domain and Audience (for JWT validation) via https://auth0.com/. You should add thse values to the appsettings.json file in the FargateAPIApp project.

```
{
  "AllowedHosts": "*",
  "Auth0": {
    "Domain": "dev-uv2qt6v37zxnefcv.us.auth0.com",
    "Audience": "https://api.earlgreyhot.org/"
  },
```

## running-the-application
To run locally, you can either run the application directly or use Docker. 

## Building
- Build the application using `dotnet build`
- Build the Docker images in the appropriate project folder using 
 - Backend API `docker buildx build -t api .`
 - Website `docker buildx build -t web .`
 - Worker `docker buildx build -t worker .`

(Why the specific image names? Just because the CDK code refers to these images. You can adjust them and the CDK code if you like.)

- Tag each image using  - for example (adjust the image name) `docker tag api:latest <account_id>.dkr.ecr.<region>.amazonaws.com/api:latest`
- Push the image to ECR using - for example (adjust the image name) `docker push <account_id>.dkr.ecr.<region>.amazonaws.com/api:latest`
- Run the CDK deployment using the project FargateApiCdk, and provide the necessary values for the stack. You only need to provide the api repository, the CDK will use the domain and namespace with a `replace` to find the rest.

`$ $ cdk deploy --require-approval never FargateApiStack -c ACCOUNT=xxxxxxxxxxxxxxx -c REGION=ap-southeast-2 -c VPC_ID=vpc-xxxxxxxxxxxxxxxx -c CERTIFICATE_ARN=arn:aws:acm:ap-southeast-2:<account_id>:certificate/xxxxxxxxxxxxxxxxxxxx -c ECR_REPO=<account_id>.dkr.ecr.ap-southeast-2.amazonaws.com/dotnet/api:latest`

- Once you have images in ECR and your stack deployed, you can push new images to ECR and update the service 'in place' without a CDK deployment using the following command:

`$ aws ecs update-service --cluster <cluster_name> --service <service_name> --force-new-deployment`

This will pull the images tagged `latest` from ECR and update your ECS tasks with those codebases. 

## Endpoints 
Please use the provided .http file to test the endpoints, or alternatively you can run the Swagger UI by navigating to the root URL of the application.
To obtain a token in order to test the deployed API, you can use the following command:
`
$ curl --request POST --url https://dev-xxxxxx.us.auth0.com/oauth/token   --header 'content-type: application/json'   --data '{"client_id":"xxxxxxxxxxxxxxxxxx","client_secret":"xxxxxx-xxxxx-xxxxxx","audience":"https://xxxxxxxx/","grant_type":"client_credentials"}'
`
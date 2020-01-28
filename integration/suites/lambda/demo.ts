import * as awsSdk from "aws-sdk";
import * as fs from "fs";
import * as path from "path";
import { verify } from "approvals";
import { normaliseStringForFilename } from "../../utils/normalize";
import { isCi } from "../../utils/environment";
import { LAMBDA_ENDPOINT, S3_ENDPOINT, SSM_ENDPOINT } from "../../utils/endpoints";
import { PutParameterRequest } from "aws-sdk/clients/ssm";
import { integer } from "aws-sdk/clients/cloudfront";

// TODO: Before (at some level - could be global) to deploy lambda
// TODO: After (at some level - to mirror the above) to remove lambda

const relativePathToLambda = isCi ? "../../" : "../../../";
const lambdaDir = path.resolve(__dirname, relativePathToLambda);
const zip = fs.readFileSync(`${lambdaDir}/lambda.zip`);
const functionName = "lookup-quote";

const REGION = 'us-east-1';
const BUCKET_NAME = 'ci-test-bucket';

const s3 = new awsSdk.S3({
  endpoint: S3_ENDPOINT,
  region: REGION,
  s3ForcePathStyle: true
});

const cachedQuote = {
	clientId: 1,
	quoteId: 2,
	text: "cached quote"
};


// const uncachedQuote = {
// 	clientId: 2,
// 	quoteId: 1,
// 	text: "uncached quote"
// };

async function cleanBucket(bucketName: string) {
  const item = await s3.listObjectsV2({ Bucket: bucketName }).promise();
  if (item.Contents) {
    for (const content of item.Contents) {
      if (content.Key) {
        await s3.deleteObject({ Bucket: bucketName, Key: content.Key }).promise();
      }
    }
  }
}

function createBucket(bucketName: string) {
  return s3
    .createBucket({
      Bucket: bucketName
    })
    .promise();
}

function createS3Item(bucketName: string, key: string, body: string) {
  return s3
    .putObject({
      Bucket: bucketName,
      Key: key,
      Body: body,
      ContentType:'application/json'
    })
    .promise();
}

// async function tearDownBucket(bucketName: string) {
//   await cleanBucket(bucketName);
//   await s3.deleteBucket({ Bucket: bucketName }).promise();
// }

// async function rebuildS3Bucket(bucketName: string) {
//   await tearDownBucket(bucketName);
//   await createBucket(bucketName);
// }

const ssm = new awsSdk.SSM({
  endpoint: SSM_ENDPOINT,
  region: REGION  
});

async function createSecret(id:integer) {
  const name = `/demo/quotes/${id}/secret`
  const value = `client-${id}-secret`

  const putReq:PutParameterRequest = {
    Name: name,
    Type: 'String',
    Value: value,
    Overwrite: true
  }

  await ssm.putParameter(putReq).promise();
}

describe("Demonstration tests", () => {
  const lambda = new awsSdk.Lambda({
    endpoint: LAMBDA_ENDPOINT,
    region: REGION
  });

  const createParams = {
    Code: {
      ZipFile: zip
    },
    FunctionName: functionName,
    Handler: "pipeline-demo-app::pipeline_demo_app.Function::FunctionHandler",
    Role: "test",
    Runtime: "dotnetcore2.1",
    Environment: {
      Variables: {
        environment: "ci",
        AWS_SECRET_ACCESS_KEY: "key",
        AWS_ACCESS_KEY_ID: "secret",
        AWS_DEFAULT_REGION: REGION
      }
    }
  };

  const deleteParams = {
    FunctionName: functionName
  };

  before(async function() {
    this.timeout(60000); // Give the lambda image time to download

    await lambda.createFunction(createParams).promise();
    await createBucket(BUCKET_NAME);
    // set secrets in SSM
    await createSecret(1);
    await createSecret(2);
  });

  after(async () => {
    await lambda.deleteFunction(deleteParams).promise();
    // await tearDownBucket(BUCKET_NAME);
    // remove secrets from SSM
  });

  context.only("on invoking lambda for a cached quote", () => {
    const invokeParams = {
      FunctionName: functionName,
      Payload: "{ \"clientId\": 1, \"quoteId\":2 }"
    };

    let result: awsSdk.Lambda.InvocationResponse;

    before(async function() {
      this.timeout(30000);

      // clear bucket
      await cleanBucket(BUCKET_NAME);

      // write cached file to bucket
      const quotebody = JSON.stringify(cachedQuote);
      await createS3Item(BUCKET_NAME, '1/2.json', quotebody);

      // setup mountebank

      result = await lambda.invoke(invokeParams).promise();
    });

    it("Should be possible to invoke the lambda", async () => {
      (result.StatusCode || -1).should.equal(200);
    }).timeout(10000);

    it("should return a quote", () => {
      const payload = result.Payload as string;
      verify(
        `${__dirname}/approvals`,
        `cached-${normaliseStringForFilename("on invoking lambda should return cached quote")}`,
        payload
      );
    });

    it("should not call the API")
  });

  context("on invoking lambda for an uncached quote", () => {
    const invokeParams = {
      FunctionName: functionName,
      Payload: "{ \"clientId\": 1, \"quoteId\":2 }"
    };

    let result: awsSdk.Lambda.InvocationResponse;

    before(async function() {
      this.timeout(10000);

      result = await lambda.invoke(invokeParams).promise();
    });

    it("Should be possible to invoke the lambda", async () => {
      (result.StatusCode || -1).should.equal(200);
    }).timeout(10000);

    it("should return a quote", () => {
      const payload = result.Payload as string;
      verify(
        `${__dirname}/approvals`,
        `hello-world-${normaliseStringForFilename("on invoking lambda should return 'Hello World!'")}`,
        payload
      );
    });
  });
});

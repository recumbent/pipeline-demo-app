import * as awsSdk from "aws-sdk";
import * as fs from "fs";
import * as path from "path";
import { verify } from "approvals";
import { normaliseStringForFilename } from "../../utils/normalize";
import { isCi } from "../../utils/environment";
import { LAMBDA_ENDPOINT } from "../../utils/endpoints";

// TODO: Before (at some level - could be global) to deploy lambda
// TODO: After (at some level - to mirror the above) to remove lambda

const relativePathToLambda = isCi ? "../../" : "../../../";
const lambdaDir = path.resolve(__dirname, relativePathToLambda);
const zip = fs.readFileSync(`${lambdaDir}/lambda.zip`);
const functionName = "lookup-quote";

describe("Demonstration tests", () => {
  const lambda = new awsSdk.Lambda({
    endpoint: LAMBDA_ENDPOINT,
    region: "us-east-1"
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
        AWS_SECRET_ACCESS_KEY: "key",
        AWS_ACCESS_KEY_ID: "secret",
        AWS_DEFAULT_REGION: "us-east-1"
      }
    }
  };

  const deleteParams = {
    FunctionName: functionName
  };

  before(async function() {
    this.timeout(60000); // Give the lambda image time to download

    await lambda.createFunction(createParams).promise();
  });

  after(async () => {
    await lambda.deleteFunction(deleteParams).promise();
  });

  context("on invoking lambda", () => {
    const invokeParams = {
      FunctionName: functionName,
      Payload: "{}"
    };

    let result: awsSdk.Lambda.InvocationResponse;

    before(async function() {
      this.timeout(10000);

      result = await lambda.invoke(invokeParams).promise();
    });

    it("Should be possible to invoke the lambda", async () => {
      (result.StatusCode || -1).should.equal(200);
    }).timeout(10000);

    it("should return 'Hello World!'", () => {
      const payload = result.Payload as string;
      verify(
        `${__dirname}/approvals`,
        `hello-world-${normaliseStringForFilename("on invoking lambda should return 'Hello World!'")}`,
        payload
      );
    });
  });
});

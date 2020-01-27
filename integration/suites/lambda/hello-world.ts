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
const functionName = "link-transform";

describe("hello-world", () => {
  const lambda = new awsSdk.Lambda({
    endpoint: LAMBDA_ENDPOINT,
    region: "eu-west-1"
  });

  const createParams = {
    Code: {
      ZipFile: zip
    },
    FunctionName: functionName,
    Handler: "lambda.handler",
    Role: "test",
    Runtime: "nodejs12.x"
  };

  const deleteParams = {
    FunctionName: functionName
  };

  before(async function() {
    this.timeout(10000); // Give the lambda image time to download

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

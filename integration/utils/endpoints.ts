import { isCi } from "./environment";

// const _awsAddressBase = isCi ? "localstack" : "0.0.0.0";
const _awsAddressBase = isCi ? "localstack" : "localhost"; // WINDOWS VERSION

export const LAMBDA_ENDPOINT = `http://${_awsAddressBase}:4574`;
export const S3_ENDPOINT = `http://${_awsAddressBase}:4572`;
export const SSM_ENDPOINT = `http://${_awsAddressBase}:4583`;

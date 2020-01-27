import { isCi } from "./environment";

const _awsAddressBase = isCi ? "localstack" : "0.0.0.0";

export const LAMBDA_ENDPOINT = `http://${_awsAddressBase}:4574`;

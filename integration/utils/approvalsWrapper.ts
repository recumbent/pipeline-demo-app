import { verify } from "approvals";

const approvalsOptions = { normalizeLineEndingsTo: "\n" };

export function verifyWithApprovals(dirname: string, approvalName: string, data: object): void {
  verify(dirname + "/approvals", approvalName, JSON.stringify(data, null, 2), approvalsOptions);
}

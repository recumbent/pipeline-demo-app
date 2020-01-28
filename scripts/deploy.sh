#!/usr/bin/env bash
set -ex

function usage() {
  cat <<EOF
  ##### deploy.sh #####
  Deploy the lambda function
    -r             AWS Region Name. May also be set as environment variable AWS_DEFAULT_REGION defaults to eu-west-1
    -e             Environment to deploy to, can be poc or prod. Defaults to poc
  Examples:
    deploy.sh -r eu-west-1 -e poc
EOF

  exit 2
}

# set default AWS Region & env, may be overridden by args
aws_region="eu-west-2"
environment="dev"
deploy_artefact="./publish/lambda.zip"
stack_name="pipeline-demo-stack"

OPTIND=1
while getopts "r:e:h" arg; do
  case $arg in
    r)
      aws_region=$OPTARG
      ;;
    e)
      environment=$OPTARG
      ;;
    *)
      usage
      ;;
  esac
done
shift "$((OPTIND-1))"

artefact_sfx=$(echo ${deploy_artefact} | awk -F. 'END { print $NF }')

lambda_s3_key=${CIRCLE_PROJECT_REPONAME}/${environment}/${CIRCLE_SHA1}.${artefact_sfx}

# add lambda deploy file to parameter file
jq --arg functions3key ${lambda_s3_key} '. + [ { "ParameterKey":"FunctionS3Key", "ParameterValue":$functions3key } ]' < infrastructure/parameters.${environment}.json > /tmp/parameters.json

if ! aws ${aws_env_str} cloudformation describe-stacks --stack-name ${stack_name} 2>&1; then
  echo "${stack_name} does not yet exist. Deploying CloudFormation template to create it..."

  aws ${aws_env_str} cloudformation create-stack \
    --stack-name ${stack_name} \
    --template-body file://infrastructure/resources.cf.json \
    --parameters file:///tmp/parameters.json \
    --capabilities CAPABILITY_NAMED_IAM 

  aws ${aws_env_str} cloudformation wait stack-create-complete --stack-name "${stack_name}"
else
  echo "${stack_name} exists. Updating with current template..."

  # create change set updating parameters
  aws ${aws_env_str} cloudformation update-stack \
    --stack-name ${stack_name} \
    --template-body file://infrastructure/resources.cf.json \
    --parameters file:///tmp/parameters.json \
    --capabilities CAPABILITY_NAMED_IAM 

  aws ${aws_env_str} cloudformation wait stack-update-complete \
    --stack-name ${stack_name}
fi

exit 0
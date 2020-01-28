#!/usr/bin/env bash
set -ex

function usage() {
  cat <<EOF
  ##### promote.sh #####
  Promote lambda package in S3 from one environment to the next
    -r             AWS Region Name. May also be set as environment variable AWS_DEFAULT_REGION defaults to eu-west-1
    -s             Source environment to promote from (i.e. poc)
    -e             environment to promote to (usually prod, might be loadtest?)
  Examples:
    promote.sh -s dev -e test
EOF

  exit 2
}

while getopts "r:s:e:h" arg; do
  case $arg in
    r)
      aws_region=$OPTARG
      ;;
    s)
      source_environment=$OPTARG
      ;;
    e)
      environment=$OPTARG
      ;;
    *)
      usage
      ;;
  esac
done

if [[ -z $source_environment ]]
then
    "Source environment (-s env) not specified"
    exit 1
fi

if [[ -z $environment ]]
then
    "Target environment (-e env) not specified"
    exit 1
fi

deploy_artefact="./publish/lambda.zip"
deploy_base="s3://murph-deployment/"

artefact_sfx=$(echo ${deploy_artefact} | awk -F. 'END { print $NF }')

lambda_source=${CIRCLE_PROJECT_REPONAME}/${source_environment}/${CIRCLE_SHA1}.${artefact_sfx}
lambda_target=${CIRCLE_PROJECT_REPONAME}/${environment}/${CIRCLE_SHA1}.${artefact_sfx}

# copy lamdba function package from source to target
aws s3 cp ${deploy_base}/${lambda_source} ${deploy_base}/${lambda_target}

exit 0
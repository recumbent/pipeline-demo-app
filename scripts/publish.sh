#!/usr/bin/env bash
set -ex

deploy_artefact="./publish/lambda.zip"

functionkey=${CIRCLE_PROJECT_REPONAME}/dev/${CIRCLE_SHA1}.zip

# copy deployment artefact
aws s3 cp ${deploy_artefact} s3://murph-deployment/${functionkey}

exit 0
# Pipeline Demo App

A lambda function for demonstrating CircleCI pipelines and related

## Spec

* Receives a request with a client id and a quote id
* Looks for the quote in S3
* If not found
	* Read client credentials from SSM
	* Call out to 3rd party API with credentials
	* Map API response to internal format
	* Save to S3
* return quote (clientId, quoteId, quote)

Don't  worry too much about error handling, kinda not the point here

## TODO

* Define schema for function request
* Define schema for function response
* Define schema/api for called service
* Called service... (variation on the one I already have, possibly as a lambda since there's a template for that)
* Return hard coded response - with test
* Return mocked response (extends test, manages to "DI" the called code, if I can work that out...)
* List progression in handler don't worry about failure cases (much)
* Stub out all the dependencies (cheat the responses based on the request)
* API call implementation (use type provider)
* Localstack S3/SSM - docker-compose
* S3 imp
	* Test ??
	* read
	* write
* SSM imp
* Somewhere in the midst of this simple config
* And logging I suppose...


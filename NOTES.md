# All that stuff to make life easier

`install-module ...`

`import-module AWS.Tools.S3`

PS C:\Windows\system32> Get-S3Bucket -EndpointUrl http://localhost:4572 -AccessKey key -SecretKey secret
PS C:\Windows\system32> New-S3Bucket -EndpointUrl http://localhost:4572 -AccessKey key -SecretKey secret -BucketName test-bucket

Write-S3Bucket -EndpointUrl http://localhost:4572 -AccessKey key -SecretKey secret -BucketName test-bucket -file quote-01.json

## SSM

get-ssmparameterlist -EndpointUrl http://localhost:4583 -AccessKey key -SecretKey key -region eu-west-2

To setup...

Write-SSMParameter -EndpointUrl http://localhost:4583 -AccessKey key -SecretKey key -region eu-west-2 -Name "demo/quote/1/secret" -Type "String" -Value "client-1-secret" -Overwrite $true

(get-ssmparameter -EndpointUrl http://localhost:4583 -AccessKey key -SecretKey key -region eu-west-2 -Name "demo/quote/1/secret").Value

You have to use us-east-1 or it will all go a bit pear shaped because SSM is assuming us-east-1


# All that stuff to make life easier

`install-module ...`

`import-module AWS.Tools.S3`

PS C:\Windows\system32> Get-S3Bucket -EndpointUrl http://localhost:4572 -AccessKey key -SecretKey secret
PS C:\Windows\system32> New-S3Bucket -EndpointUrl http://localhost:4572 -AccessKey key -SecretKey secret -BucketName test-bucket

Write-S3Bucket -EndpointUrl http://localhost:4572 -AccessKey key -SecretKey secret -BucketName test-bucket -file quote-01.json


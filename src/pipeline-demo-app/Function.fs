namespace pipeline_demo_app

open Amazon.Lambda.Core

open System
open System.IO
open FSharp.Data

open Amazon
open Amazon.S3
open Amazon.S3.Model
open Amazon.SimpleSystemsManagement
open Amazon.SimpleSystemsManagement.Model

// type Request = JsonProvider<""" { "clientId": 1, "quoteId": 2 } """>

[<CLIMutable>]
type Request =
    {
        ClientId : int
        QuoteId : int
    }


type Response = JsonProvider<""" { "clientId": 1, "quoteId": 2, "text": "this is a quote" } """>
type ApiResponse = JsonProvider<""" { "author": "Douglas Adams", "quote": "this is a quote" } """>

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
()

type Lookup = int -> int -> Response.Root option

module aws =
    let s3Config = AmazonS3Config(ForcePathStyle = true, ServiceURL = "http://localhost:4572")
    let ssmConfig = AmazonSimpleSystemsManagementConfig(ServiceURL = "http://localhost:4583")
    
    let existsInS3 bucket key =
        let s3 = new AmazonS3Client(s3Config)
        let req = new ListObjectsRequest(BucketName = bucket, Prefix = key)
        let resp = s3.ListObjectsAsync(req) |> Async.AwaitTask |> Async.RunSynchronously
        resp.S3Objects.Count = 1

    let readS3 bucket key =
        let s3 = new AmazonS3Client(s3Config)
        let req = GetObjectRequest( BucketName = bucket, Key = key)
        let s3Object = s3.GetObjectAsync(req) |> Async.AwaitTask |> Async.RunSynchronously
        use sr = new StreamReader(s3Object.ResponseStream)
        let content = sr.ReadToEnd()
        content

    let writeS3 bucket key content = 
        let s3 = new AmazonS3Client(s3Config)
        let req = PutObjectRequest( BucketName = bucket, Key = key, ContentBody = content)
        s3.PutObjectAsync(req) |> Async.AwaitTask|> Async.RunSynchronously |> ignore
    
    let readSSM name = 
        let ssm = new AmazonSimpleSystemsManagementClient(ssmConfig)
        let req = GetParameterRequest(Name = name)
        let ssmObject = ssm.GetParameterAsync(req)  |> Async.AwaitTask |> Async.RunSynchronously
        ssmObject.Parameter.Value

    let readSSMPath path = 
        let ssm = new AmazonSimpleSystemsManagementClient(ssmConfig)
        let req = GetParametersByPathRequest(Path = path, Recursive = true, WithDecryption = true)
        let ssmObject = ssm.GetParametersByPathAsync(req) |> Async.AwaitTask |> Async.RunSynchronously
        ssmObject //.Parameter.Value

    let writeSSM name param =
        let ssm = new AmazonSimpleSystemsManagementClient(ssmConfig)
        let req = PutParameterRequest(Name = name, Value = param, Overwrite = true, Type = ParameterType.String)
        ssm.PutParameterAsync(req) |> Async.AwaitTask|> Async.RunSynchronously |> ignore

module helpers =
    let bucket = "test-bucket" // This should be from config

    let MakeSecretName clientId =
        sprintf "/demo/quote/%i/secret" clientId

    let MakeFileKey clientId quoteId =
        sprintf "%i/%i.json" clientId quoteId

    let ReadFromCache clientId quoteId = 
        let key = MakeFileKey clientId quoteId
        if (aws.existsInS3 bucket key) then
            let quote = Response.Parse (aws.readS3 bucket key)
            Some(quote)
        else
            None

    let WriteToCache (quote:Response.Root) =
        let key = MakeFileKey quote.ClientId quote.QuoteId
        aws.writeS3 bucket key (quote.ToString())

    let GetSecret clientId =
        aws.readSSM (MakeSecretName clientId)

    let MakeApiRequest quoteId secret = 

        let uri = sprintf "https://86w5r4xyvk.execute-api.eu-west-2.amazonaws.com/Prod/quote/dna/%i" quoteId // Should be function, server should be from config
        let content = HttpRequestHeaders.Accept HttpContentTypes.Json // Ditto
        let auth = HttpRequestHeaders.Authorization secret

        let data = 
            Http.RequestString
                ( uri,
                  headers = [ auth; content ]
                )

        ApiResponse.Parse data

    let MapToQuote clientId quoteId (apiQuote:ApiResponse.Root) =
        Response.Root(clientId, quoteId, apiQuote.Quote)

    let FetchFromApi clientId quoteId = 
        let quote = 
            GetSecret clientId
            |> MakeApiRequest quoteId
            |> MapToQuote clientId quoteId

        Some(quote)

    let Handler (readFromCache:Lookup) (fetchFromApi:Lookup) (lookupRequest:Request) =
        match readFromCache lookupRequest.ClientId lookupRequest.QuoteId with
        | Some(quote) -> quote
        | None ->
            let newQuote = fetchFromApi lookupRequest.ClientId lookupRequest.QuoteId
            WriteToCache newQuote.Value
            newQuote.Value
        

type Function() =

    let Handler = helpers.Handler helpers.ReadFromCache helpers.FetchFromApi

    member __.FunctionHandler (input: Request) (_: ILambdaContext) =
        (Handler input).ToString()

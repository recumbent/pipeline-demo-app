namespace pipeline_demo_app

open Amazon.Lambda.Core

open System
open System.IO
open FSharp.Data

open Amazon
open Amazon.S3
open Amazon.S3.Model

type Request = JsonProvider<""" { "clientId": 1, "quoteId": 2 } """>
type Response = JsonProvider<""" { "clientId": 1, "quoteId": 2, "text": "this is a quote" } """>
type ApiResponse = JsonProvider<""" { "author": "Douglas Adams", "quote": "this is a quote" } """>

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
()

type Lookup = int -> int -> Response.Root option

module aws =
    let s3Config = AmazonS3Config(RegionEndpoint = RegionEndpoint.EUWest2, ForcePathStyle = true, ServiceURL = "http://localhost:4572" )
    
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
            
module helpers =
    let ReadFromCache clientId quoteId = 
        match clientId with
        | 1 -> Some(Response.Root(clientId, quoteId, "cached quote"))
        | _ -> None

    let FetchFromApi clientId quoteId = 
        Some(Response.Root(clientId, quoteId, "api quote"))

    let Handler (readFromCache:Lookup) (fetchFromApi:Lookup) (lookupRequest:Request.Root) =
        match readFromCache lookupRequest.ClientId lookupRequest.QuoteId with
        | Some(quote) -> quote
        | None ->
            (fetchFromApi lookupRequest.ClientId lookupRequest.QuoteId).Value

type Function() =

    let Handler = helpers.Handler helpers.ReadFromCache helpers.FetchFromApi


    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    member __.FunctionHandler (input: Request.Root) (_: ILambdaContext) =
        Handler input

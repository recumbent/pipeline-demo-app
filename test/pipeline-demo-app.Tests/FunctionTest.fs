namespace pipeline_demo_app.Tests


open Xunit
open Amazon.Lambda.TestUtilities

open pipeline_demo_app


module FunctionTest =
    [<Fact>]
    let ``Invoke Quote Lambda Function``() =
        // Invoke the lambda function and confirm the string was upper cased.
        let lambdaFunction = Function()
        let context = TestLambdaContext()
        let request = Request.Root(1,2)
        let result = lambdaFunction.FunctionHandler request context

        Assert.Equal(request.ClientId, result.ClientId)
        Assert.Equal(request.QuoteId, result.QuoteId)
        Assert.Equal("cached quote", result.Text)

    [<Fact>]
    let ``Invoke Quote Lambda Function for no cache``() =
        // Invoke the lambda function and confirm the string was upper cased.
        let lambdaFunction = Function()
        let context = TestLambdaContext()
        let request = Request.Root(2,1)
        let result = lambdaFunction.FunctionHandler request context

        Assert.Equal(request.ClientId, result.ClientId)
        Assert.Equal(request.QuoteId, result.QuoteId)
        Assert.Equal("api quote", result.Text)

    // let ``Data in cache should not call api``
    // let ``Data in cache should not call SSM``
    // let ``No data in cache should call api with parans``

    [<EntryPoint>]
    let main _ = 0

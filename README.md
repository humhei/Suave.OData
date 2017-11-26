# Suave.OData.LiteDB
An Experimental OData Implementation in Suave With LiteDB.FSharp 
# Run Test
* cd Suave.OData.LiteDB.Tests
* dotnet restore
* dotnet run
# Debug Test In VSCode
* cd Suave.OData.LiteDB.Tests
* dotnet restore
* Press F5 
# NugetPackage
* https://www.nuget.org/packages/Suave.OData.LiteDB/
# How to use it
  ```fsharp
  [<EntryPoint>]
  let main _ =
    let mapper = FSharpBsonMapper()
    use memoryStream = new MemoryStream()
    use db = new LiteRepository(memoryStream, mapper)    
    db.Insert(defaultCompany)
    let odataRouter=resource "odata/company" (db.Database.GetCollection<Company>()) |> OData.CRUD
    let app=choose[
                     odataRouter
                     //other Suave Router
    ]
    startWebServer defaultConfig app
    0 
  ```
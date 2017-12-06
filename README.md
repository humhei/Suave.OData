# Suave.OData.LiteDB
[![Travis](https://img.shields.io/travis/humhei/Suave.OData.LiteDB.svg)](https://travis-ci.org/humhei/Suave.OData.LiteDB)
[![NuGet](https://img.shields.io/nuget/v/Suave.OData.LiteDB.svg?colorB=Green)](https://www.nuget.org/packages/Suave.OData.LiteDB/)
An Experimental OData Implementation in Suave With [LiteDB.FSharp]
(https://github.com/Zaid-Ajaj/LiteDB.FSharp)
> The library is experiemental and not production-ready. The Api is subject to change.
### Usage
  [Sample is available](https://github.com/humhei/Suave.OData.LiteDB.Samples)

  ```fsharp
  open Suave
  open LiteDB
  open LiteDB.FSharp
  open Suave.OData.LiteDB
  type Company = {
    Id: int
    Name: string
}
  [<EntryPoint>]
  let main _ =
    let mapper = FSharpBsonMapper()
    use memoryStream = new MemoryStream()
    use db = new LiteRepository(memoryStream, mapper)    
    db.Insert({Id=1;Name="testCompany"})
    let odataRouter=resource "odata/company" (db.Database.GetCollection<Company>()) |> OData.CRUD
    let app=choose[
                     odataRouter
                     //other Suave Router
    ]
    startWebServer defaultConfig app
    0 
  ```
## Supported Opreations
* Basic CRUD Opreations:Get,Add,Delete,Update
* Query Opreations:$select $expand
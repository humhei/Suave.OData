module Tests.OData

open Expecto
open System.IO
open LiteDB
open LiteDB.FSharp
open Types
open Suave.Testing
open Suave
open Suave.OData.LiteDB
open Suave.OData.LiteDB.Json
open System.Net.Http
open LiteDB.FSharp.Help
let useDatabase (f: LiteRepository -> WebPart) = 
    let mapper = FSharpBsonMapper()
    mapper.Entity<Order>().DbRef(toLinq(<@fun c->c.Company@>))|>ignore
    mapper.Entity<Order>().DbRef(toLinq(<@fun c->c.EOrders@>))|>ignore  
    let memoryStream = new MemoryStream()
    let db = new LiteRepository(memoryStream, mapper)
    f db
    
let odataRouter() = 
  useDatabase<| fun db->
    let c1={Id=1;Name="test"}
    db.Insert(c1)|>ignore
    db.Insert({Id=2;Name="Hello"})|>ignore
    db.Insert({Id=1;Company=c1;EOrders=[]})|>ignore
    choose[
       resource "odata/company" (db.Database.GetCollection<Company>()) |> OData.CRUD
       resource "odata/order" (db.Database.GetCollection<Order>()) |> OData.CRUD
    ]
//use different port to run test in parallel
let mutable port=9000    
let runWithConfig= 
  port<-port+1
  runWith 
    {defaultConfig with 
      bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" port ]}
let ODataTests =
  testList "ODataTests" [
    testCase "OData GetEntityById Test" <| fun _ -> 
      let ctx=runWithConfig  <|odataRouter()
      let res=
       ctx
       |>req GET "odata/company(1)" None
       |>ofJson<Company>
      Expect.equal res.Name  "test" "OData GetById Test Corrently" 
    // testCase "OData  $select Query Test" <| fun _ -> 
    //   let ctx=runWithConfig  <|odataRouter()
    //   let res=
    //     ctx
    //       |>req GET "odata/company" None
    //       |>ofJson<list<string>> 
    //   Expect.equal res ["test";"Hello"] "OData Filter EntityTest Corrently"         
    testCase "OData Add Entity Test" <| fun _ -> 
      let ctx=runWithConfig <|odataRouter()
      let newCompany={Id=3;Name="newCompany"}|>toJson
      let data=new StringContent(newCompany)
      let res=
        ctx
        |>req POST "odata/company" (Some data)
        |>ofJson<Company>
      Expect.equal res.Name  "newCompany" "OData Add Entity Test Corrently"  
    testCase "OData Delete Entity Test" <| fun _ -> 
      let ctx=runWithConfig <|odataRouter()
      let res=
        ctx
        |>req DELETE "odata/company(2)" None
        |>ofJson<Company>
      Expect.equal res.Name  "Hello" "OData Delete Entity Test Corrently"  
    testCase "OData Update Entity Test" <| fun _ -> 
      let ctx=runWithConfig <|odataRouter()

      let updatedCompany={Id=2;Name="updatedCompany"}|>toJson
      let data=new StringContent(updatedCompany)
      let res=
        ctx
          |>req PUT "odata/company(2)" (Some data)
          |>ofJson<Company>
      Expect.equal res.Name  "updatedCompany" "OData Update Entity Test Corrently" 
    testCase "OData  $select Query Test" <| fun _ -> 
      let ctx=runWithConfig <|odataRouter()
      let res=
        ctx
          |>reqQuery GET "odata/company" "$select=Name"
          |>ofJson<list<string>> 
      Expect.equal res ["test";"Hello"] "OData Filter EntityTest Corrently"              
    // testCase "OData  $expand entity Query Test" <| fun _ -> 
    //   let ctx=runWithConfig<|odataRouter()
    //   let res=
    //     ctx
    //       |>reqQuery GET "odata/order(1)" "$expand=Company"
    //       |>ofJson<Order> 
    //   Expect.equal "Hello2" "Hello" "OData Filter EntityTest Corrently"                
  ]
namespace Suave.OData.LiteDB
open Suave
open Successful
open RequestErrors
open Filters
open Operators
open ServerErrors
open LiteDB.FSharp
open Json
/// A derivative of Suave.OData. Code adapted from Tamizh's origin
//see https://goo.gl/JketNx
[<AutoOpen>]
module Types =
  type Resource<'a> = {
    Name : string
    FindById : int -> Async<'a option>
    Add : 'a -> Async<'a option*exn>
    DeleteById : int -> Async<'a option>
    UpdateById : int -> 'a -> Async<'a option*exn>
    Entities : seq<'a>
  }
[<RequireQualifiedAccess>]
module OData =
  open System.Text
  // type Webpart = HttpContext -> Async<HttpContext option>
  // ('a -> Async<'b>) -> 'a -> WebPart
  let FindById f id (ctx : HttpContext) = async {
    let! findResult = f id
    match findResult with
    | Some entity ->
      return! JSON OK entity ctx
    | _ -> return! NOT_FOUND "" ctx
  }
  let DeleteById = FindById
  let Create add (ctx : HttpContext) = async {
    let entity = getResourceFromReq ctx.request
    try
     let! addResult,ex = add entity
     match addResult with
     | Some entity -> return! JSON CREATED  entity ctx
     | None -> return! JSON INTERNAL_ERROR ex ctx
    with exn->
      return! JSON BAD_REQUEST exn ctx
  }
  let UpdateById find update id (ctx : HttpContext) = async {
    let entity = getResourceFromReq ctx.request
    let! findResult = find id
    match findResult with
    | Some _ -> 
      let! updateResult,exn = update id entity
      match updateResult with
        | Some entity -> return! JSON OK entity ctx
        | _ -> return!JSON INTERNAL_ERROR  exn ctx
    | None -> return! NOT_FOUND "" ctx   
  }  
  let Filter dbSet (ctx : HttpContext) = async {
    let nv=ctx.request.query
        |> List.filter (fun (k,v) -> k <> "" && Option.isSome v)
        |> List.map(fun (k,v) -> k+"="+v.Value)
    let filteredEntities = ODataParser.filter dbSet nv.[0]
    return!  JSON OK  filteredEntities ctx  
  }
  let CRUD resource (ctx : HttpContext) = async {
    let odata =
      let resourcePath = "/" + resource.Name
      let resourceIdPath =
        new PrintfFormat<(int -> string),unit,string,string,int>
          (resourcePath + "(%d)")
      choose [
        path resourcePath >=> choose
          [GET >=> Filter resource.Entities
           POST >=> Create resource.Add]
        GET >=> pathScan resourceIdPath (FindById resource.FindById)
        DELETE >=> pathScan resourceIdPath (DeleteById resource.DeleteById)
        PUT >=>pathScan resourceIdPath (UpdateById resource.FindById resource.UpdateById)
      ]
    return! odata ctx
   }
   
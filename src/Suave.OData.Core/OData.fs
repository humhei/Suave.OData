namespace Suave.OData.Core
open Linq2Rest.Parser
open System.Collections.Specialized
open System.Linq
open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.ServerErrors
open Suave.RequestErrors
open System.Threading.Tasks
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open Json

[<AutoOpen>]
module Types =
  type Resource<'a> = {
    Name : string
    Entities : IEnumerable<'a>
    Add : 'a -> Async<int>
    Update : 'a -> Async<int>
    Find : int -> Async<'a option>
  }

  type InsertOrUpdate<'a> =
  | Insert of ('a -> Async<int>)
  | Update of ('a -> Async<int>)

[<RequireQualifiedAccess>]
module OData =
  let Filter dbSet (ctx : HttpContext) = async {
    let nv = new NameValueCollection()
    ctx.request.query
    |> List.filter (fun (k,v) -> k <> "" && Option.isSome v)
    |> List.map(fun (k,v) -> (k,v.Value))
    |> List.iter (fun (k,v) -> nv.Add(k,v))
    let parser = new ParameterParser<'a>()
    let filteredEntities = parser.Parse(nv).Filter(dbSet)
    return! JSON OK filteredEntities ctx
  }

  let CreateOrUpdate action (ctx : HttpContext) = async {
    let entity = getResourceFromReq ctx.request
    let vctx = new ValidationContext(entity)
    let results = new List<ValidationResult>()
    let isValid = Validator.TryValidateObject(entity, vctx, results)
    if isValid then
      try
        match action with
        | Insert action ->
          let! _ = action entity
          return! JSON CREATED entity ctx
        | Update action ->
          let! _ = action entity
          return! JSON OK entity ctx
      with
      | ex -> return! JSON INTERNAL_ERROR ex ctx
    else
      return! JSON BAD_REQUEST results ctx
  }

  let CRUD resource (ctx : HttpContext) = async {
    let odata =
      let resourcePath = "/" + resource.Name
      choose [
        path resourcePath >=> choose [
          GET >=> Filter resource.Entities
          POST >=> CreateOrUpdate (Insert resource.Add)
          PUT >=>  CreateOrUpdate (Update resource.Update)
        ]
      ]
    return! odata ctx
  }


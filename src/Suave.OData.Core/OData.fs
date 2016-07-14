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
open System.Data.Entity
open System.ComponentModel.DataAnnotations
open Json

[<AutoOpen>]
module Types =
    type EntityConfig<'a when 'a: not struct> = {
      Name : string
      DbSet : DbSet<'a>
      SaveChanges : unit -> Task<int>
    }

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
    return! JSON filteredEntities ctx
  }

  let Create config (ctx : HttpContext) = async {
    let entity = getResourceFromReq ctx.request
    let vctx = new ValidationContext(entity)
    let results = new List<ValidationResult>()
    let isValid = Validator.TryValidateObject(entity, vctx, results)
    if isValid then
      config.DbSet.Add(entity) |> ignore
      try
        let! _ = config.SaveChanges() |> Async.AwaitTask
        return! CREATED "" ctx
      with
      | ex -> return! INTERNAL_ERROR ex.Message ctx
    else
      return! BAD_REQUEST (toJsonStr results) ctx
  }

  let CRUD entityConfig (ctx : HttpContext) = async {
    let odata =
      path ("/" + entityConfig.Name) >=> choose [
        GET >=> Filter entityConfig.DbSet
        POST >=> Create entityConfig
      ]
    return! odata ctx
  }


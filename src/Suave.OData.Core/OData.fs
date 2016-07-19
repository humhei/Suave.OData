namespace Suave.OData.Core

open Linq2Rest.Parser
open System.Collections.Specialized
open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.ServerErrors
open Suave.RequestErrors
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open Json

[<AutoOpen>]
module Types =
  type Resource<'a> = {
    Name : string
    Entities : IEnumerable<'a>
    Add : 'a -> Async<'a option>
    UpdateById : int -> 'a -> Async<'a option>
    FindById : int -> Async<'a option>
    DeleteById : int -> Async<'a option>
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
    return! JSON OK filteredEntities ctx
  }

  let private validate entity =
    let vctx = new ValidationContext(entity)
    let results = new List<ValidationResult>()
    let isValid = Validator.TryValidateObject(entity, vctx, results)
    (isValid, results)

  let Create add (ctx : HttpContext) = async {
    let entity = getResourceFromReq ctx.request
    let isValid, results = validate entity
    if isValid then
      let! addResult = add entity
      match addResult with
      | Some entity -> return! JSON CREATED entity ctx
      | None -> return! INTERNAL_ERROR "" ctx
    else
      return! JSON BAD_REQUEST results ctx
  }

  let FindById f id (ctx : HttpContext) = async {
    let! findResult = f id
    match findResult with
    | Some entity ->
      return! JSON OK entity ctx
    | _ -> return! NOT_FOUND "" ctx
  }
  let DeleteById = FindById

  let UpdateById find update id (ctx : HttpContext) = async {
    let entity = getResourceFromReq ctx.request
    let isValid, results = validate entity
    if isValid then
      let! findResult = find id
      match findResult with
      | Some _ ->
        let! updateResult = update id entity
        match updateResult with
        | Some entity -> return! JSON OK entity ctx
        | _ -> return! INTERNAL_ERROR "" ctx
      | _ -> return! NOT_FOUND "" ctx
    else
      return! JSON BAD_REQUEST results ctx
  }

  let CRUD resource (ctx : HttpContext) = async {
    let odata =
      let resourcePath = "/" + resource.Name
      let resourceIdPath =
        new PrintfFormat<(int -> string),unit,string,string,int>
          (resourcePath + "(%d)")
      choose [
        path resourcePath >=> choose [
          GET >=> Filter resource.Entities
          POST >=> Create resource.Add
        ]
        GET >=> pathScan resourceIdPath (FindById resource.FindById)
        PUT >=>
          pathScan resourceIdPath 
            (UpdateById resource.FindById resource.UpdateById)
        DELETE >=> pathScan resourceIdPath (DeleteById resource.DeleteById)
      ]
    return! odata ctx
  }


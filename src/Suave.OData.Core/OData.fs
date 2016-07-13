namespace Suave.OData.Core
open Linq2Rest.Parser
open System.Collections.Specialized
open System.Linq
open Suave.Http
open Json

[<AutoOpen>]
module Extensions =

  let ODataFilter entities (ctx : HttpContext) = async {
    let nv = new NameValueCollection()
    ctx.request.query
    |> List.filter (fun (k,v) -> k <> "" && Option.isSome v)
    |> List.map(fun (k,v) -> (k,v.Value))
    |> List.iter (fun (k,v) -> nv.Add(k,v))
    let parser = new ParameterParser<'a>()
    let filteredEntities = parser.Parse(nv).Filter(entities)
    return! JSON filteredEntities ctx
  }

namespace Suave.OData.Core
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open System.Text
open Suave.Operators
open Suave

module internal Json =

  let toJsonStr v =
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver
      <- new CamelCasePropertyNamesContractResolver()
    JsonConvert.SerializeObject(v, jsonSerializerSettings)

  let JSON webpartCombinator v =
    toJsonStr v
    |> webpartCombinator
    >=> Writers.setMimeType "application/json; charset=utf-8"

  let fromJson<'a> json =
      JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

  let getResourceFromReq<'a> (req : HttpRequest) =
      req.rawForm |> Encoding.UTF8.GetString |> fromJson<'a>
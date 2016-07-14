namespace Suave.OData.Core
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Operators
open Suave

module internal Json =

  let toJsonStr v =
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    JsonConvert.SerializeObject(v, jsonSerializerSettings)

  let JSON webpart v =
    toJsonStr v
    |> webpart
    >=> Writers.setMimeType "application/json; charset=utf-8"

  let fromJson<'a> json =
      JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

  let getResourceFromReq<'a> (req : HttpRequest) =
      let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
      req.rawForm |> getString |> fromJson<'a>
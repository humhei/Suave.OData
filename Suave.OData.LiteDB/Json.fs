namespace Suave.OData.LiteDB
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open System.Text
open Suave.Operators
open Suave

module  Json =
    open LiteDB.FSharp

    let private fsharpJsonConverter = FSharpJsonConverter()
    let private converters : JsonConverter[] = [| fsharpJsonConverter |]
    let private toJson value =
        JsonConvert.SerializeObject(value, converters)
    let  ofJson<'a> (json:string) : 'a =
        JsonConvert.DeserializeObject<'a>(json, converters)   
    let  getResourceFromReq<'a> (req : HttpRequest) =
        req.rawForm |> Encoding.UTF8.GetString |> ofJson<'a>    
    let  JSON (body:string->WebPart) entity (ctx:HttpContext) =body (toJson entity) ctx
    
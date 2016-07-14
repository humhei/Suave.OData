namespace Suave.OData.EF
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<AllowNullLiteral>]
type Entity () =
  [<Key>]
  [<Column("id")>]
  member val ID = 0 with get, set

[<AllowNullLiteral>]
[<Table("people",Schema="public")>]
type People () =
  inherit Entity()
  [<Column("firstName")>]
  [<Required>]
  member val FirstName = "" with get, set
  [<Column("lastName")>]
  member val LastName = "" with get, set
  [<Column("email")>]
  [<Required>]
  member val Email = "" with get, set
  [<Column("age")>]
  member val Age = 0 with get, set


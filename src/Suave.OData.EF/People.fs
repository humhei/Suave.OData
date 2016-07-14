namespace Suave.OData.EF
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<Table("people",Schema="public")>]
type People () =
  [<Key>]
  [<Column("id")>]
  member val ID = 0 with get, set
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


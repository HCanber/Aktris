module ``ActorSystem Tests``

open System
open Xunit
open FsUnit.Xunit
open Aktris
open Aktris.Internals

// Naming-----------------------------------------------------------
[<Fact>]
let ``When creating an InternalActorSystem with null a ArgumentNullException is thrown``() =
  Assert.Throws<ArgumentNullException>(fun () -> InternalActorSystem(null) |> ignore)

[<Fact>]
let ``When creating an InternalActorSystem with "" as name a ArgumentException is thrown``() =
  Assert.Throws<ArgumentException>(fun () -> InternalActorSystem("") |> ignore)

[<Fact>]
let ``When creating an InternalActorSystem with invalid characters in the name a ArgumentException is thrown``() =
  Assert.Throws<ArgumentException>(fun () -> InternalActorSystem("Aktör") |> ignore)

[<Fact>]
let ``Given a InternalActorSystem Then its´ name can be retrieved``() =
  let system=InternalActorSystem("MySystem")
  system.Name |> should equal "MySystem"

[<Fact>]
let ``When creating a ActorSystem using static Create method and null as name, then "default" is used as name``() =
  let system = ActorSystem.Create(null)
  system.Name |> should equal "default"

module ``ActorSystem CreateActor Tests``

open System
open Xunit
open FsUnit.Xunit
open FluentAssertions
open Aktris
open Aktris.Internals

[<Fact>]
let ``Given an ActorSystem we can create an local actor``() =
  let system = ActorSystem.Create()
  let actorRef = system.CreateActor()
  actorRef.Should().BeOfType<LocalActorRef>(null, null) |> ignore

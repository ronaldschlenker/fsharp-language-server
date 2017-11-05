module LSP.JsonTests

open Types
open Parser
open Json
open System.Runtime.Serialization
open NUnit.Framework
open System.Text.RegularExpressions

let removeSpace (expected: string) = 
    Regex.Replace(expected, @"\s", "")

[<Test>]
let ``remove space from string`` () = 
    Assert.That(removeSpace "foo bar", Is.EqualTo "foobar")

[<Test>]
let ``remove newline from string`` () = 
    let actual = """foo 
    bar"""
    Assert.That(removeSpace actual, Is.EqualTo "foobar")

[<Test>]
let ``serialize primitive types to JSON`` () = 
    Assert.That(serializerFactory<bool> defaultJsonWriteOptions true, Is.EqualTo("true"))
    Assert.That(serializerFactory<int> defaultJsonWriteOptions 1, Is.EqualTo("1"))
    Assert.That(serializerFactory<string> defaultJsonWriteOptions "foo", Is.EqualTo("\"foo\""))

[<Test>]
let ``serialize option to JSON`` () = 
    Assert.That(serializerFactory<option<int>> defaultJsonWriteOptions (Some 1), Is.EqualTo("1"))
    Assert.That(serializerFactory<option<int>> defaultJsonWriteOptions (None), Is.EqualTo("null"))

type SimpleRecord = {simpleMember: int}

[<Test>]
let ``serialize record to JSON`` () = 
    let record = {simpleMember = 1}
    Assert.That(serializerFactory<SimpleRecord> defaultJsonWriteOptions record, Is.EqualTo("""{"simpleMember":1}"""))

[<Test>]
let ``serialize a record with a custom writer`` () = 
    let record = {simpleMember = 1}
    let customWriter (r: SimpleRecord): string = sprintf "simpleMember=%d" r.simpleMember
    let options = {defaultJsonWriteOptions with customWriters = [customWriter]}
    Assert.That(serializerFactory<SimpleRecord> options record, Is.EqualTo("\"simpleMember=1\""))

type Foo = Bar | Doh 
type FooRecord = {foo: Foo}

[<Test>]
let ``serialize a union with a custom writer`` () = 
    let record = {foo = Bar}
    let customWriter = function 
    | Bar -> 10
    | Doh -> 20
    let options = {defaultJsonWriteOptions with customWriters = [customWriter]}
    Assert.That(serializerFactory<FooRecord> options record, Is.EqualTo("""{"foo":10}"""))
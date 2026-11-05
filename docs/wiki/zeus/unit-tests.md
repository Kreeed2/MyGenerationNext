---
title: UnitTests project (stubs and sample template)
summary: States what the legacy UnitTests project actually contains (empty test shells and one C# template) so agents do not expect test coverage.
keywords: [UnitTests, BaseTemplateTest, CSharpTests, VbNetTests, JScriptTests, VbScriptTests, CSharpTestTemplate001]
source_paths:
  - mygeneration-legacy-code/mygeneration/UnitTests/BaseTemplateTest.cs
  - mygeneration-legacy-code/mygeneration/UnitTests/CSharpTests.cs
  - mygeneration-legacy-code/mygeneration/UnitTests/UnitTests.csproj
  - mygeneration-legacy-code/mygeneration/UnitTests/Templates/CSharpTestTemplate001.csgen
  - mygeneration-legacy-code/templates/UnitTestTemplates/CSharpTestTemplate001.csgen
---

# UnitTests

- Namespace `MyGeneration.UnitTests`. `CSharpTests`, `VbNetTests`, `JScriptTests`, `VbScriptTests` are **empty classes** (no test attributes or methods).
- `BaseTemplateTest` declares `BuildSqLiteDatbase()` and `BuildEffiprozDatbase()` returning `Tuple<string,string,dbRoot>`, with empty bodies.
- `UnitTests.csproj` references no test framework; only System, Data, Xml, Linq.
- `UnitTests/Templates/CSharpTestTemplate001.csgen` is a 3-byte file; the real sample is `templates/UnitTestTemplates/CSharpTestTemplate001.csgen`: uses `<%#VERSION v3.5%><%#REF System.Core.dll %><%#NS System.Linq %>` and a LINQ loop writing even numbers 2..100 with `output.writeln`. Useful as a golden-test seed (expected output: the even numbers one per line).

There is no behavioral spec here; derive tests from [execution-flow.md](execution-flow.md).

## Related pages
- [execution-flow.md](execution-flow.md)
- [template-file-format.md](template-file-format.md)
- [INDEX.md](INDEX.md)

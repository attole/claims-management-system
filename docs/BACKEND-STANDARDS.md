# Backend standards

Layout, declarations, naming and test shapes for the C# code in `backend/`. `.claude/CLAUDE.md` points here; read this file before writing or reviewing backend code. The frontend and the deployment have their own: `docs/FRONTEND-STANDARDS.md` and `docs/DEPLOYMENT-STANDARDS.md`. Domain and Application shapes are in `docs/layers/domain.md` and `docs/layers/application.md`.

## Code style

- **Line length:** 120 characters maximum.
- **LINQ:** method syntax. Two or more calls, or a line over the limit, put each call on its own line starting with `.`, indented one level.
- **Arguments and parameters:** one line when it fits, else one per line indented one level (constructor injection and initializers too). Never nest a wrapped call inside another: put the inner one in a named variable. Keep long literal text out of a call as a constant.
- **Records and constructors:** the base list stays on the declaration line when it fits, else on the next line indented. A constructor initializer stays on the signature line when it fits, else on its own line.
- **Blank lines:** one between members; none after `{` or before `}`; one before the final `return` unless it is the first statement of its block; one between test steps.
- **Wrapped lists:** one item per line with a trailing comma. **Ternaries** that do not fit put `?` and `:` on their own lines, true branch first.
- **Usings:** `System` first, then alphabetical, one block above the file-scoped namespace; no unused usings (the build errors).
- **Declarations:** `var` where the right side shows the type; explicit types only for built-in keyword types, lambdas, collection expressions and a base or interface type chosen on purpose. Never `Type name = new(...)`. Collections are `[]`. Classes are `sealed` unless built for inheritance. Records for commands, queries, DTOs, inputs, events, findings, details. Primary constructors for DI. Constants over `static readonly` for compile-time values.
- **Member order:** constants, fields, constructors (private EF one first), properties (data, collections, computed, capability and audit last), public methods, internal, protected, private; a static factory first within its group.
- No `#region`, no commented-out code, no TODO markers.

## Naming

- English; PascalCase for types, methods, properties, constants and enum members; camelCase for parameters and locals; `_camelCase` for private fields; `I` prefix for interfaces. Whole words; only `Id`, `Dto`, `Url`, `Json` and spec names (`RowVer`) are abbreviations. No type in a name.
- A collection is plural, an item singular; a lambda parameter is the singular element word or `_`, never a single letter.
- Ids end in `Id`, timestamps in `At`, calendar dates in `Date`; names fixed by the spec or schema (audit columns, `IsDeleted`, `DeletedAt`, `RowVer`, `OrganisationId`, `ReportDate`) are kept as written. Every `bool` member starts with `Is`, `Has`, `Can` or `Should` (except framework-fixed members and options properties bound to config keys).
- Methods are a verb phrase; `Async` suffix on async methods (not `Handle` or controller actions); `CancellationToken` last. `Create` makes a new object, `From` parses or converts to a validated value, `As<Thing>` returns a converted copy, `Get<Thing>` is a query. A child's mutator is a bare verb (`Remove`); the root's is verb plus thing (`RemoveRiskObject`). Engine names (`Apply`, `Check`, `Require`, `AllowedFrom`) are fixed.
- A type's suffix is its role: rule `Is<Statement>`/`Has<Thing>`; `<Thing>Details`; `<Verb><Thing>` + `Input`/`Operation`/`Transition`; `<Scope>Base<Role>`; event `<Thing><PastVerb>Event`; value object the plain noun, absent value `Empty` with `IsSet`; closed set `<Concept>Kind` enum, abstract `<Concept>` record, variants `<Owner><Value><Concept>` (without behavior: a plain enum); `<Name>Options`; `<Verb><Noun>Command`, `Get<Noun>Query`/`List<Nouns>Query`/`Check<Noun>Query`, `...Dto`/`...Request`/`...Response`. A finding's code names who reports it.

## Tests

- One test class per type under test, `<Type>Tests`, in the mirrored folder and namespace (only `EntityConstructionTests` has no twin); Application use-case and request files follow `docs/layers/application.md` 'Tests'. Fixtures live in `Support/` mirroring source folders; fill claims through aggregate methods (`TestClaims`), never private state; shared setup is a private static helper or a fixture, never a mutated field.
- Name `<Subject>_<Condition>_<Result>`. Arrange, act, assert, separated by one blank line, no comments. Arrange holds everything the act needs, including the input record. Act is exactly one statement; its result is `outcome` (operation or transition) or `result`; a call that must throw is `Action act`/`Func<T> act` with `act.Should().Throw<...>()`. Assert reads only the result and arranged objects.
- Each case is tested once, where it lives: a fact's owner tests its edge cases (`[Theory]` for data-only differences); a rule tests a pass and a fail; a step tests each declared source, one undeclared source, one refusal per required rule and what it changes; an aggregate method tests only its follow-ups and one refusal. One act and one reason to fail per test.
- Attributes: `[Fact]`/`[Theory]`, then `[Trait("Rule", ...)]` per covered rule or contract code, then `[InlineData]`. FluentAssertions; assert a finding by code, never by message, except the `S:V-Messages` tests. Classes are `public sealed`. Test code follows production standards.

status: done

The Domain is finished: the claim with its children, the reserve component with its history rows, claim documents, policies, cause-of-loss codes, roles and the engine, with tests. The settled architecture, interpretations, schema departures and decisions live in docs/ARCHITECTURE.md; code shapes and conventions live in .claude/CLAUDE.md. This note keeps only what is not recorded there.

OPEN

- Valid reason codes: the spec defines no list; the code stays free text and must not be invented.
- Whether a party needs a valid email: the spec gives only nullable columns (the name is Application shape validation).
- Which warnings need acknowledgement is an Application question, kept in notes-layer-application.md.

DECISIONS

- Test fixtures build claims through the public aggregate methods, so a fixture claim has been evaluated and has raised its events like a real one. Two shortcuts bypass them on purpose: `TestClaims.MoveTo` places a claim in a state through a test transition, and `TestClaims.ChangeLoss` sets the loss on the child to skip the after-Draft guard.
- Reserve fixtures (`TestReserves`) build a component through `ReserveComponent.Open` and a pending row through the submit operation, so a row starts pending unless a test calls the component method (which auto-approves within the limit).

ANALOGUE

- The engine is `backend/src/ClaimsModule.Domain/Core`; the aggregates are `Claims/`, `ReserveComponents/` (with `History/`), `ClaimDocuments/`, `Policies/` and `CauseOfLoss/` (reference data, no operations).
- `OpenClaimTransition` is the reference transition; `OpenReserveComponentOperation` and `UploadClaimDocumentOperation` are the reference creation operations with outside facts; `ReserveComponent.SubmitHistory` is the reference chained aggregate method on a child.
- The layer structure and registration pattern follow `backend/src/ClaimsModule.Application/ApplicationServiceExtensions.cs`.

NEXT

- The Application, Persistence, Infrastructure and API packs, each starting from notes-layer-application.md and notes-layer-persistence.md.

# GitHub Copilot Review Instructions (AOP.Logging.NET — netstandard2.0 / .NET Framework 4.8 + Clean Code)

You are reviewing AOP.Logging.NET (attribute-based AOP logging using Source Generators). The library must support .NET Framework 4.8 via netstandard2.0. Review for compatibility, correctness, security/privacy, performance, and clean code / best practices. Be strict and propose actionable fixes.

## 0) Non-negotiable: netstandard2.0 compatibility
	•	Runtime projects must target netstandard2.0 (works on .NET Framework 4.8).
	•	Do NOT introduce APIs unavailable in netstandard2.0 / .NET 4.8 (e.g., ArgumentNullException.ThrowIfNull, newer Regex APIs, DateOnly/TimeOnly, many modern helpers).
	•	Validate NuGet dependencies: ensure Microsoft.Extensions.Logging and related packages support netstandard2.0.
	•	Keep Source Generator dependencies separate; generator can target modern TFMs, but generated/runtime code must be netstandard2.0-safe.

## 1) Correctness: does it actually intercept & log?
	•	Verify the Source Generator produces real interception (entry/exit/exception) matching README claims.
	•	Ensure attributes are truly applied (not just defined):
	•	[LogClass], [LogMethod], [LogParameter], [LogResult], [LogException], [SensitiveData]
	•	Verify DI integration works on .NET Framework 4.8:
	•	Prefer interface-based proxies/wrappers; explicitly note limitations (self-calls, concrete-class calls).
	•	Exception handling:
	•	preserve stack traces (throw; only)
	•	never swallow business exceptions
	•	logging failures must never throw.

## 2) Security & privacy (PII/secret leakage)
	•	Treat logging as high-risk for data exposure.
	•	Ensure sensitive data masking is enforced:
	•	[SensitiveData] must actually mask values.
	•	Add optional heuristic masking by name (password, pwd, token, secret, apikey, authorization, cookie, etc.).
	•	Structured logging:
	•	Do not attach parameter/return values to structured state unless explicitly enabled.
	•	If structured payload is always included, flag as privacy issue and propose an option like IncludeStructuredPayload=false by default.
	•	Exception details:
	•	safe defaults for production; avoid logging full stack traces/messages unless configured.

## 3) Performance: hot-path discipline
	•	Require early logger.IsEnabled(level) checks before any formatting/enumeration/allocations.
	•	Identify and reduce allocations:
	•	avoid LINQ (Select, ToList, string.Join) in hot paths
	•	avoid repeated string.Replace chains
	•	avoid per-call dictionary creation if not needed
	•	Filtering performance:
	•	cache/compile wildcard/regex matchers once; do not build regex per call.

## 4) DoS protections in value formatting
	•	Never enumerate full IEnumerable to compute count.
	•	Avoid ToList() on unknown enumerables.
	•	Implement bounded enumeration (take MaxCollectionSize + 1, then stop).
	•	Add recursion depth limits and cycle protection for object graphs.
	•	Guard against expensive or throwing ToString().

## 5) Clean Code & Best Practices (must check)

Review code against these principles and call out violations with concrete refactors:
	•	Single Responsibility / Separation of concerns
	•	generator logic vs runtime logging vs DI extensions must be cleanly separated
	•	Naming & readability
	•	meaningful names, avoid ambiguous abbreviations, consistent terminology (Entry/Exit/Exception)
	•	Immutability & thread safety
	•	options objects should be immutable after build; avoid shared mutable state
	•	Error handling
	•	no broad catch (Exception) unless rethrowing or explicitly swallowing with justification
	•	avoid side effects in formatting; keep “best effort” logging non-throwing
	•	API design
	•	minimize public surface area, keep internal helpers internal
	•	avoid breaking changes; keep options backwards compatible
	•	XML docs for public APIs; clear defaults
	•	Consistency
	•	consistent nullability annotations
	•	consistent logging event IDs and message templates
	•	Testing
	•	unit tests for generator output and runtime behavior
	•	include at least one integration test consuming the netstandard2.0 package (simulating .NET Framework 4.8 usage if possible)
	•	Maintainability
	•	avoid duplication; extract shared formatting logic
	•	keep methods small; avoid deep nesting; prefer guard clauses
	•	Style
	•	prefer explicitness over cleverness
	•	no hidden behavior (e.g., structured state containing data that templates do not show) unless clearly documented and opt-in

## 6) Source Generator (Roslyn) best practices
	•	Output must be deterministic and stable.
	•	Handle: partial classes, generics, nested types, async patterns, ref/out/in.
	•	Avoid expensive semantic model work per node; prefer incremental generator approach.
	•	Generated code must compile cleanly under analyzers; no warnings.

## 7) Review output format
	•	Categorize findings: Blocker / High / Medium / Low.
	•	For each finding:
	•	exact file(s) + code region
	•	impact (compatibility/security/perf/correctness/maintainability)
	•	concrete fix proposal (patch-like snippet preferred)
	•	tests to add

## 8) Blocker conditions (must flag)

Mark as Blocker if any of the following are true:
	•	runtime cannot target netstandard2.0
	•	generator does not implement actual interception/wrappers
	•	sensitive data can leak by default (structured payload or lack of masking)
	•	collection formatting can cause unbounded enumeration / DoS
  

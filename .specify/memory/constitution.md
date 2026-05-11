<!--
Sync Impact Report
Version change: template -> 1.0.0
Modified principles:
- Template Principle 1 -> I. Code Quality Is the Baseline
- Template Principle 2 -> II. Tests Define Done
- Template Principle 3 -> III. User Experience Must Stay Consistent
- Template Principle 4 -> IV. Performance Budgets Are Mandatory
- Template Principle 5 -> V. Simplicity Preserves Maintainability
Added sections:
- Delivery Standards
- Review and Release Gates
Removed sections:
- None
Templates requiring updates:
- ✅ .specify/templates/plan-template.md
- ✅ .specify/templates/spec-template.md
- ✅ .specify/templates/tasks-template.md
Follow-up TODOs:
- None
-->

# Duotify Membership Constitution

## Core Principles

### I. Code Quality Is the Baseline

Every production change MUST keep the codebase easier to understand than before
the change. Implementations MUST use clear naming, bounded responsibilities,
explicit error handling, and the smallest practical change surface. Duplication,
dead code, hidden side effects, and undocumented exceptions MUST be removed or
explicitly justified in the plan. Reviews MUST reject changes that add accidental
complexity without a documented need.

### II. Tests Define Done

No change is complete until automated tests prove the intended behavior and
protect against regression. Each change MUST include test coverage proportional to
its risk: unit tests for isolated logic, integration or contract tests for
cross-boundary behavior, and regression tests for defect fixes. Manual-only
verification is insufficient for merge approval. When fixing a bug, the failing
test that reproduces the bug MUST exist before or alongside the fix.

### III. User Experience Must Stay Consistent

User-facing work MUST preserve the product's established interaction patterns,
copy tone, visual states, and accessibility semantics across comparable flows.
New screens and components MUST define loading, empty, success, and error states.
Any deliberate deviation from an existing pattern MUST be called out in the spec
or plan with rationale and reviewer approval. Consistency is a product quality
requirement, not a design preference.

### IV. Performance Budgets Are Mandatory

Every feature MUST declare measurable performance expectations before
implementation begins. Plans MUST capture the relevant budget, such as render
time, bundle impact, query latency, throughput, or memory use, and MUST define
how compliance will be verified. Changes that exceed the stated budget MAY only
proceed with a documented exception, mitigation, and follow-up task. Performance
regressions discovered during review block release until resolved or explicitly
accepted.

### V. Simplicity Preserves Maintainability

The default solution MUST be the simplest design that satisfies current
requirements. New dependencies, abstractions, and configuration layers MUST be
introduced only when the immediate value is clear and documented. Teams MUST
prefer extending proven patterns over inventing parallel ones, and MUST document
any unavoidable tradeoff where short-term delivery increases long-term cost.

## Delivery Standards

All specifications MUST describe user value, acceptance scenarios, edge cases,
experience consistency expectations, and measurable success criteria. All plans
MUST document the code quality strategy, required test layers, UX consistency
approach, and performance validation method before implementation work starts.
All task lists MUST include the work needed to implement, verify, and document
those commitments; testing, UX validation, and performance checks cannot be
deferred into an unspecified polish phase.

## Review and Release Gates

Every pull request MUST summarize scope, risk, and verification evidence. That
evidence MUST include automated test results, and for user-facing changes it MUST
also include proof of UX review such as screenshots, recordings, or explicit
state coverage. Performance-sensitive work MUST include benchmark or profiling
results against the declared budget. Reviewers MUST confirm constitution
compliance before approval, and releases MUST not ship while critical quality,
test, UX, or performance regressions remain unresolved.

## Governance

This constitution takes precedence over local habits and ad hoc process choices.
Amendments MUST be recorded in this document, MUST describe the behavioral change,
and MUST update any affected templates before the amendment is considered active.
Versioning follows semantic rules: MAJOR for incompatible governance changes or
principle removal, MINOR for new principles or materially stronger requirements,
and PATCH for clarifications that do not change expected behavior. Compliance
review is mandatory during planning, pull request review, and release readiness
checks.

**Version**: 1.0.0 | **Ratified**: 2026-05-11 | **Last Amended**: 2026-05-11

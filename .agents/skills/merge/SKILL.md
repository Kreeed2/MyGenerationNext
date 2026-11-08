---
name: merge
description: >-
  Merge changes into main for the MyGenerationNext project (Kreeed2/MyGenerationNext).
  Invoke ONLY when the user has explicitly requested merging changes (Merge Change
  button, /merge, or equivalent "land this" language). Do NOT invoke for review,
  preparation, passing checks, or skill installation alone.
disable-model-invocation: true
metadata:
  delta-action: land
---

# Merge changes into main (MyGenerationNext)

Carry out an explicit merge request for this repository end to end: prepare the
branch, verify locally, open a pull request, confirm checks, merge, and verify the
result on `main`.

The user invoked this skill because they asked to merge. Do NOT respond by asking
whether they want to merge, and do not say you are waiting for an explicit merge
request. Proceed with the workflow below. Stop only for genuine blockers listed
under "Stop and report".

## Repository facts

- GitHub repo: `Kreeed2/MyGenerationNext`, destination branch: `main`.
- Remote for publication: `origin` (https://github.com/Kreeed2/MyGenerationNext.git).
  The `local` remote is a Delta backlink — never push to it.
- Merge practice: pull requests merged with a merge commit (see PRs #1 and #2 in
  history). Use merge method `merge`, not squash or rebase.
- No CI workflows, branch protection, or required reviews are configured. Verify
  this still holds at merge time (see "Checks" below); do not assume it.
- `gh` CLI is NOT installed. Use the GitHub MCP server tools for all GitHub
  operations.
- Solution: `src/MyGenerationNext.slnx` (projects: Core, Cli, Tests; xunit).
  Source: `src/MyGenerationNext.slnx` and the `.csproj` files under `src/`.

## Workflow

### 1. Establish the change state

- Run `git --no-optional-locks status` and `git branch --show-current`.
- If there is uncommitted work, commit it first with a clear message describing
  the change. Never merge with a dirty working tree.
- If HEAD is `main`, create a feature branch from the current state:
  `git checkout -b feature/<short-description>` (the repo uses `feature/...`
  names, e.g. `feature/zeus-dotnetscript-engine`).
- If HEAD is already a feature branch, use it.

### 2. Verify locally

Run both from the repo root and require success before continuing:

```sh
dotnet build src/MyGenerationNext.slnx --nologo
dotnet test src/MyGenerationNext.slnx --nologo
```

(Verified working with .NET SDK 10.0.403; 4 xunit tests in
`src/MyGenerationNext.Tests/`.) If either fails, stop and report the failure —
do not open a PR.

### 3. Publish the branch

```sh
git push -u origin <branch>
```

If the push is rejected as non-fast-forward, stop and report; do not force-push.

### 4. Open the pull request

Use the GitHub MCP tool `create_pull_request` with owner `Kreeed2`, repo
`MyGenerationNext`, base `main`, head `<branch>`, and a title/body summarizing
the change. If a PR for the branch already exists, use it instead.

### 5. Checks

Use `pull_request_read` with method `get_check_runs` (and `get_status`) for the
PR. The repo currently defines no checks, so an empty list is expected and is
success. If any checks DO exist:

- All required checks must have completed successfully for the PR head commit.
- Pending, failing, missing, or unverifiable required checks are NOT success —
  stop and report which check blocks the merge.

### 6. Merge

Use the GitHub MCP tool `merge_pull_request` with `merge_method: "merge"` and
`expectedHeadSha` set to the PR head SHA (guards against merging a different
version than was verified).

If the merge reports conflicts:

- Resolve automatically ONLY when the intended result is clear: the two sides
  touch disjoint regions, or one side is a strict superset of the other. Do the
  resolution locally (fetch `origin/main`, `git merge origin/main` on the feature
  branch, edit, commit, push), then re-run step 2 (build + test) and step 5
  before merging.
- Never drop unrelated work while resolving.
- If intent is ambiguous — both sides changed the same logic in different ways,
  or the correct outcome is a judgment call — stop, describe the conflicting
  hunks, and wait for the user's decision.

### 7. Verify and clean up

- Confirm the merge: `git fetch origin main` and check that the PR head SHA is an
  ancestor of `origin/main` (`git merge-base --is-ancestor <sha> origin/main`),
  or confirm via `pull_request_read` method `get` that `merged` is true.
- After the merge is verified, delete the remote feature branch:
  `git push origin --delete <branch>`.
- Report the resulting `main` commit SHA and PR number.

## Stop and report

Halt the workflow and explain the blocker (changes NOT merged) when any of these
occur:

- Build or test failure locally.
- Push rejected (non-fast-forward) or PR creation fails.
- Required checks pending, failing, or unverifiable.
- Branch protection or review requirements appear that the workflow cannot
  satisfy — report them and ask how to proceed rather than bypassing.
- Ambiguous merge conflicts.
- Any unexpected repository state (detached HEAD with no commits, wrong repo,
  destination branch moved unexpectedly).

A merge is complete only when the change is verified on `origin/main`. Opening a
PR, pushing a branch, or starting checks is not completion.

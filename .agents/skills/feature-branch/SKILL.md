---
name: feature-branch
description: >-
  Enforces feature-branch workflow in this repository: after planning a feature
  and before writing code or committing anything, the agent must create and
  switch to a feature/<short-description> branch. Use whenever the user asks to
  implement, build, or change a feature, fix, or other code change — never let
  work start or land directly on main.
---

# Feature branch before any work

This repository never receives feature work directly on `main`. Every change —
feature, fix, refactor — happens on a `feature/<short-description>` branch.

## The rule

After the feature is planned (scope and approach are clear) and BEFORE writing
any code or creating any commit:

1. Check where you are:

   ```sh
   git --no-optional-locks status
   git branch --show-current
   ```

2. If HEAD is `main` (or any non-feature branch), create and switch to a
   feature branch from the current state:

   ```sh
   git checkout -b feature/<short-description>
   ```

   Use a short kebab-case description, e.g. `feature/zeus-dotnetscript-engine`.

3. If HEAD is already a `feature/...` branch, stay on it — do not create
   another branch for the same work.

4. Only then start editing files and committing.

## Hard constraints

- NEVER commit directly to `main`. If you notice you are about to, stop,
  create the feature branch (uncommitted changes carry over), and continue
  there.
- NEVER commit before the branch exists. The branch comes first, then code,
  then commits.
- If the working tree already has uncommitted changes on `main` when this
  rule kicks in, create the branch anyway — `git checkout -b` keeps the
  changes — and commit them on the feature branch.
- If the user explicitly asks to commit straight to `main`, confirm once that
  they really want to bypass the feature-branch workflow before doing so.

## Relation to merging

When the work is later merged, the `merge` skill handles the PR workflow. This
skill only governs where the work happens: always on a feature branch.

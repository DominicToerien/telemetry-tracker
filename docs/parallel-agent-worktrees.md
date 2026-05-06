# Parallel Agent Worktrees

Use git worktrees so each chat or agent gets:

- its own branch
- its own checkout directory
- its own local port allocation
- its own optional copied `.env`

This avoids branch switching conflicts and makes it safe to run multiple local API instances at the same time.

## Branching Rule

- create every new issue branch from `main`
- create every new implementation worktree from `main`
- merge `main` into long-running branches to keep them current
- do not branch new implementation work from another in-progress feature branch

## Scripts

Create a worktree:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\New-AgentWorkspace.ps1 `
  -Name tracking `
  -BranchName Spike/tracking-control `
  -BaseBranch main
```

Run the app inside a worktree with its generated workspace ports:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Invoke-AgentWorkspace.ps1 -Command run
```

Run tests inside a worktree:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Invoke-AgentWorkspace.ps1 `
  -Command test `
  -ProjectPath .\telemetry-tracker.Tests\telemetry-tracker.Tests.csproj
```

Remove a worktree when you are done:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Remove-AgentWorkspace.ps1 -Name tracking
```

## What Gets Generated

Each worktree gets:

- `.env.agent`
  - generated workspace-only settings such as `ASPNETCORE_URLS`
- `.agent-workspace.json`
  - metadata about branch, ports, and creation time
- optional copied `.env`
  - copied from the source checkout if one exists so local secrets do not need to be re-entered

## Default Location

By default, new worktrees are created under a sibling folder:

```text
../telemetry-tracker-worktrees/<name>
```

This keeps each checkout isolated while still close to the main repository.

## Suggested Workflow

1. Keep your main checkout as the coordination workspace.
2. Create one worktree per chat or agent task.
3. Open each worktree in its own Codex or editor window.
4. Use `run-workspace` or `Invoke-AgentWorkspace.ps1 -Command run` inside that worktree.
5. Merge or cherry-pick completed work back intentionally.

## Notes

- Branch names should follow the project naming rules in `agent/rules.md`.
- `Invoke-AgentWorkspace.ps1` runs `dotnet run --no-launch-profile` so the generated workspace ports win over shared launch-profile defaults.
- If `.env` is missing, the app still runs; persistence stays disabled unless configured.

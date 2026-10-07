# Releasing

A release publishes two things from one git tag:

- the NuGet package `SudokuGen` on nuget.org
- a GitHub Release with the Windows TUI (`sudoku-tui-win-x64.zip` and its `.sha256` file)

See ADR 0007 and ADR 0008 for the reasons.

## One-time setup

### 1. Make the repository public

The install command and nuget.org verification need a public repository.
Before you make it public, check that the README, `LICENSE` and `NOTICE.md` are correct.

### 2. Create the GitHub Environment

1. Open **Settings > Environments > New environment**.
2. Name it `nuget`.
3. Optional: add **Required reviewers**. The workflow then waits for your approval before it pushes to nuget.org. NuGet cannot delete a version, so this is recommended.

### 3. Create the trusted publishing policy on nuget.org

1. Sign in to nuget.org.
2. Open your user name menu, then **Trusted Publishing**.
3. Select **Create**. Enter these values:

   | Field | Value |
   | --- | --- |
   | Policy name | `sudoku-gen` |
   | Package owner | your user or organization |
   | Repository owner | `kwdevhq` |
   | Repository | `sudoku-gen` |
   | Workflow file | `release.yml` |
   | Environment | `nuget` |

4. Save the policy.

The workflow gets a short-lived API key at run time. Do not create a long-lived API key.

If the first push fails with an authorization error, the policy may need an existing package. In that case, push version 1.0.0 once with a temporary API key, then use trusted publishing for later versions. Check the current nuget.org Trusted Publishing documentation.

### 4. Set the nuget.org user name

Add a repository variable `NUGET_USER` (**Settings > Secrets and variables > Actions > Variables**). Its value is your nuget.org profile name, not your e-mail address. The login step uses it.

### 5. Optional: reserve an ID prefix

This project uses no prefix. To reserve one anyway, send a request through **nuget.org > Account > Reserve prefix**. A prefix prevents other users from publishing IDs that start with it.

## Make a release

1. Make sure `main` is green.
2. Create and push a tag:

   ```
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. Watch the **Release** workflow. If the `nuget` environment has reviewers, approve the push.
4. Check the package on nuget.org and the files on the GitHub Release.

A tag with a hyphen, such as `v1.1.0-preview.1`, is a pre-release on both NuGet and GitHub.

## If a release fails

- Failure before the push: delete the tag (`git push --delete origin vX.Y.Z`, then `git tag -d vX.Y.Z`), fix the cause, and tag again.
- Failure after the push to nuget.org: the version is used. Create the next version.

---
status: accepted
---
# Release by git tag and publish to nuget.org with trusted publishing

A tag `vX.Y.Z` starts `.github/workflows/release.yml`. The workflow runs `scripts/check.ps1`, packs the library with the version taken from the tag, and pushes it to nuget.org from the GitHub Environment `nuget`. Authentication uses nuget.org trusted publishing (OIDC), so the repository holds no API key. The `.csproj` has no `<Version>`.

The package ID is `SudokuGen`, the same as the assembly and namespace. There is no company prefix: this project stays separate from business packages. NuGet cannot delete a published version, so the environment can require a manual approval before the push.

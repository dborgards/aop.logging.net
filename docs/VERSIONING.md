# Versioning Strategy

This project uses **Semantic Versioning** (SemVer) automated through **[semantic-release](https://github.com/semantic-release/semantic-release)** and **Conventional Commits**.

## Semantic Versioning

We follow [Semantic Versioning 2.0.0](https://semver.org/):

```
MAJOR.MINOR.PATCH
```

- **MAJOR**: Breaking changes (incompatible API changes)
- **MINOR**: New features (backward-compatible)
- **PATCH**: Bug fixes (backward-compatible)

## Conventional Commits

All commit messages MUST follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Commit Types and Version Impact

| Type | Description | Version Bump | Example |
|------|-------------|--------------|---------|
| `breaking:` or `major:` | Breaking change | MAJOR | `breaking: remove deprecated API` |
| `feat:` or `feature:` | New feature | MINOR | `feat: add async logging support` |
| `fix:` | Bug fix | PATCH | `fix: resolve null reference exception` |
| `perf:` | Performance improvement | PATCH | `perf: optimize string formatting` |
| `revert:` | Revert previous commit | PATCH | `revert: undo feature X` |
| `docs:` | Documentation | NONE | `docs: update README examples` |
| `chore:` | Maintenance | NONE | `chore: update dependencies` |
| `refactor:` | Code refactoring | NONE | `refactor: simplify logger interface` |
| `test:` | Add/update tests | NONE | `test: add unit tests for LogMethod` |
| `style:` | Code style changes | NONE | `style: format code` |
| `build:` | Build system changes | NONE | `build: update npm scripts` |
| `ci:` | CI/CD changes | NONE | `ci: update GitHub Actions` |

### Breaking Changes

Breaking changes can be indicated in two ways:

1. **Using type prefix**:
   ```
   breaking: remove ILogger dependency
   ```

2. **Using footer**:
   ```
   feat: redesign configuration API

   BREAKING CHANGE: Configuration API has been completely redesigned
   ```

## Branch Strategy

### Main Branches

- **`main`**: Production releases (stable)
  - Triggers automatic release to NuGet
  - Version format: `1.2.3`

- **`develop`**: Development pre-releases
  - Triggers automatic pre-release
  - Version format: `1.2.3-beta.1`

### Supporting Branches

- **`alpha`**: Alpha pre-releases
  - Version format: `1.2.3-alpha.1`

- **`feature/*`**: Feature development
  - No automatic release
  - Create PR to `develop`

- **`hotfix/*`**: Critical bug fixes
  - Create PR to `main`

## Automated Versioning Workflow

### 1. Development

```bash
# Create feature branch
git checkout -b feature/my-feature

# Make changes with conventional commits
git commit -m "feat: add new logging attribute"

# Push to trigger CI build
git push origin feature/my-feature
```

### 2. Create Pull Request

- PR to `develop` for features
- PR to `main` for releases
- CI automatically runs tests

### 3. Merge to Develop

```bash
# Merge PR to develop
# semantic-release workflow automatically:
# 1. Analyzes commits
# 2. Calculates new version
# 3. Updates CHANGELOG.md
# 4. Updates .csproj files
# 5. Creates pre-release on GitHub
# 6. Tags the release
```

### 4. Release to Main

```bash
# Merge develop to main
git checkout main
git merge develop

# Push to trigger release
git push origin main

# semantic-release workflow automatically:
# 1. Analyzes commits since last release
# 2. Calculates new version
# 3. Updates CHANGELOG.md
# 4. Updates all .csproj files
# 5. Builds and tests
# 6. Creates NuGet packages
# 7. Publishes to NuGet.org
# 8. Creates GitHub Release
# 9. Commits version updates
# 10. Tags the release
```

## Version Calculation Examples

### Example 1: Feature Addition

```
Last version: 1.2.3

Commits:
- feat: add custom interceptor support
- docs: update README

New version: 1.3.0
```

### Example 2: Bug Fix

```
Last version: 1.2.3

Commits:
- fix: resolve memory leak in logger
- test: add regression test

New version: 1.2.4
```

### Example 3: Breaking Change

```
Last version: 1.2.3

Commits:
- breaking: redesign attribute API
- feat: add new configuration options
- fix: correct parameter serialization

New version: 2.0.0
```

### Example 4: Mixed Changes

```
Last version: 1.2.3

Commits:
- feat: add support for custom formatters
- feat: add performance counters
- fix: resolve race condition
- docs: update examples

New version: 1.3.0
(Minor bump from features, patch fix is absorbed)
```

## semantic-release Configuration

The project uses `.releaserc.json` for semantic-release configuration:

```json
{
  "branches": ["main", {"name": "develop", "prerelease": "beta"}],
  "plugins": [
    "@semantic-release/commit-analyzer",
    "@semantic-release/release-notes-generator",
    "@semantic-release/changelog",
    "@semantic-release/exec",
    "@semantic-release/git",
    "@semantic-release/github"
  ]
}
```

### Plugins Explained

1. **commit-analyzer**: Analyzes commits and determines version bump
2. **release-notes-generator**: Generates release notes from commits
3. **changelog**: Updates CHANGELOG.md
4. **exec**: Runs custom commands (update .csproj, pack, publish to NuGet)
5. **git**: Commits changed files back to repo
6. **github**: Creates GitHub release and uploads assets

## Checking Next Version Locally

Install semantic-release CLI:
```bash
npm install
```

Dry run to see what version would be released:
```bash
npx semantic-release --dry-run
```

## Release Checklist

semantic-release handles most of this automatically, but before merging to `main`:

- [ ] All tests pass
- [ ] All commits follow Conventional Commits format
- [ ] Breaking changes are documented in commit messages
- [ ] Migration guide exists (if breaking changes)

## Manual Version Override (Emergency Only)

If you need to manually create a release:

```bash
# Create a commit with desired version in package.json
git commit -m "chore(release): 2.0.0 [skip ci]"

# Create and push tag
git tag v2.0.0
git push origin v2.0.0
```

**Note**: This should only be used in emergency situations.

## Pre-release Versions

### Beta (develop branch)
```
1.3.0-beta.1
1.3.0-beta.2
```

### Alpha (alpha branch)
```
1.3.0-alpha.1
1.3.0-alpha.2
```

## Troubleshooting

### Version not incrementing

- Check commit messages follow Conventional Commits format
- Verify commits since last release contain version bump keywords
- Review semantic-release logs in GitHub Actions

### Wrong version calculated

- Review commit history: `git log --oneline`
- Run `npx semantic-release --dry-run` locally
- Check `.releaserc.json` configuration

### Release failed

- Check GitHub Actions logs
- Verify `NUGET_API_KEY` secret is set
- Ensure `GITHUB_TOKEN` has correct permissions
- Check for semantic-release errors

### Package not published to NuGet

- Verify NuGet API key is valid
- Check package ID is not already taken
- Review NuGet publish logs in GitHub Actions

## semantic-release vs Manual Versioning

### Benefits of semantic-release:

✅ **Fully automated** - No manual version updates needed
✅ **Consistent** - Version always matches commits
✅ **Traceable** - Clear link between version and changes
✅ **Time-saving** - No manual CHANGELOG updates
✅ **Error-proof** - No human mistakes in versioning
✅ **Standardized** - Industry-standard tool and workflow

### What semantic-release does:

1. Analyzes commits since last release
2. Determines version bump type (major/minor/patch)
3. Generates new version number
4. Updates CHANGELOG.md with categorized changes
5. Updates all .csproj files with new version
6. Builds and tests the project
7. Creates and uploads NuGet packages
8. Publishes to NuGet.org
9. Creates GitHub Release with release notes
10. Commits version changes back to repo
11. Tags the release

All of this happens **automatically** when you push to `main` or `develop`.

## References

- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [semantic-release](https://github.com/semantic-release/semantic-release)
- [semantic-release Plugins](https://semantic-release.gitbook.io/semantic-release/extending/plugins-list)

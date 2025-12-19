# Versioning Strategy

This project uses **Semantic Versioning** (SemVer) automated through **GitVersion** and **Conventional Commits**.

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
| `docs:` | Documentation | NONE | `docs: update README examples` |
| `chore:` | Maintenance | NONE | `chore: update dependencies` |
| `refactor:` | Code refactoring | NONE | `refactor: simplify logger interface` |
| `perf:` | Performance improvement | NONE | `perf: optimize string formatting` |
| `test:` | Add/update tests | NONE | `test: add unit tests for LogMethod` |
| `style:` | Code style changes | NONE | `style: format code` |
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

- **`feature/*`**: Feature development
  - Version format: `1.2.3-alpha.feature-name.1`
  - Example: `feature/add-custom-interceptors`

- **`hotfix/*`**: Critical bug fixes
  - Version format: `1.2.4`
  - Merged to both `main` and `develop`

- **`release/*`**: Release preparation
  - Version format: `1.2.3-rc.1`

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
# Semantic Release workflow automatically:
# 1. Analyzes commits
# 2. Calculates new version
# 3. Updates project files
# 4. Creates pre-release
```

### 4. Release to Main

```bash
# Merge develop to main (or create release branch)
git checkout main
git merge develop

# Push to trigger release
git push origin main

# Semantic Release workflow automatically:
# 1. Analyzes commits since last release
# 2. Calculates new version
# 3. Updates CHANGELOG.md
# 4. Creates GitHub Release
# 5. Publishes to NuGet
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

## Manual Version Override

In rare cases, you may need to manually set a version:

```bash
# Tag with specific version
git tag v2.0.0
git push origin v2.0.0
```

## Pre-release Versions

### Alpha (feature branches)
```
1.3.0-alpha.my-feature.1
```

### Beta (develop branch)
```
1.3.0-beta.1
1.3.0-beta.2
```

### Release Candidate
```
1.3.0-rc.1
1.3.0-rc.2
```

## GitVersion Configuration

The project uses `GitVersion.yml` for configuration:

```yaml
mode: ContinuousDeployment
commit-message-incrementing: Enabled
major-version-bump-message: "^(breaking|major)(\\(.+\\))?(!:|:)"
minor-version-bump-message: "^(feat|feature|minor)(\\(.+\\))?:"
patch-version-bump-message: "^(fix|patch)(\\(.+\\))?:"
```

## Checking Version Locally

Install GitVersion:
```bash
dotnet tool install --global GitVersion.Tool
```

Check version:
```bash
dotnet gitversion
```

## Release Checklist

Before merging to `main`:

- [ ] All tests pass
- [ ] CHANGELOG.md is up to date (auto-generated)
- [ ] Breaking changes are documented
- [ ] Migration guide exists (if breaking changes)
- [ ] All commits follow Conventional Commits
- [ ] Version bump is correct

## Troubleshooting

### Version not incrementing

- Check commit messages follow Conventional Commits format
- Ensure commits contain version bump keywords
- Check GitVersion.yml configuration

### Wrong version calculated

- Review commit history: `git log --oneline`
- Check GitVersion output: `dotnet gitversion`
- Verify branch name matches configuration

### Release failed

- Check GitHub Actions logs
- Verify NUGET_API_KEY secret is set
- Ensure permissions are correct

## References

- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [GitVersion Documentation](https://gitversion.net/)

# EasyButtons — Store Sync

Package: `com.voluntarytransactions.easybuttons`

## Pull (store → local)

```bash
# Run from repo root — requires google-play-listings MCP or Claude
# get_listing(packageName, language: en-US) → write to listing/en-US/*.txt
# get_app_info(packageName) → sync metadata.json
# get_releases(packageName) → check track status
```

## Push (local → store)

```bash
# Read listing/en-US/*.txt and call:
# update_listing(packageName, language, title, shortDescription, fullDescription)
```

## Graphics (manual — no MCP upload yet)

Upload via Play Console → Store listing → Graphics:
- `graphics/icon-512.png`         — App icon (512×512)
- `graphics/feature-graphic.png`  — Feature graphic (1024×500)
- `graphics/screenshots/en-US/`   — Phone screenshots (add after first build)

## Release checklist

1. Bump `ApplicationVersion` + `ApplicationDisplayVersion` in csproj
2. Add `release-notes/v{versionCode}.txt`
3. Build signed AAB (see CLAUDE.md)
4. `deploy_app` → internal track
5. Test → `promote_release` → production
6. If listing changed → push listing

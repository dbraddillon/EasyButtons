# EasyButtons — AI Store Submission Guide

Navigate to https://play.google.com/console and open (or create) the EasyButtons app.
Paste values from code blocks exactly. Confirm each field before moving to the next section.

---

## Status

- [ ] App created in Play Console
- [ ] Service account has Releases + Store presence permissions
- [ ] Phase 1 complete (all text/form fields)
- [ ] Images uploaded (icon, feature graphic, screenshots)
- [ ] Phase 2 complete (verification sweep, no draft blockers)
- [ ] AAB uploaded to internal track
- [ ] Promoted to production

---

## Service account permission check

Play Console → Users and permissions → verify `play-store-service-account` has:
- ✅ Release to production, exclude devices, and use app signing by Google Play
- ✅ Manage store presence (required for Claude's update_listing API call — without this it returns 403)

---

## Part 0 — Create app (skip if already exists)

Play Console → All apps → Create app
- App name: `EasyButtons: One-Tap Launcher`
- Default language: English (United States)
- App or game: App
- Free or paid: Free
- Accept declarations → Create app

---

# PHASE 1 — AI fills all text and form fields

---

## Part 1 — Store listing

Play Console → Store listing (Main store listing)

**App name** (30 chars max):
```
EasyButtons: One-Tap Launcher
```

**Short description** (80 chars max):
```
Big satisfying buttons that launch any app or link — one tap, done.
```

**Full description** (4000 chars max):
```
EasyButtons turns anything you launch repeatedly into a giant, satisfying button you can tap in one shot.

No menus. No searching. No fumbling. Just big, bold, colorful dome buttons — tap one and you're there.

── WHAT IT DOES ──

Each button is a shortcut to anything your phone can open:
• Apps — launch any installed app directly
• Websites — open a URL in your browser
• Phone calls — dial a contact instantly
• SMS — open a message thread
• Navigation — jump straight to a Maps destination
• Custom URI schemes — trigger any deep link or app action
• Sound buttons — tap to play any audio file (sound effects, voice clips, whatever)

Up to 4 buttons. Pick colors, name them whatever makes sense to you. Long-press any button to edit or delete it.

── WHY EASY BUTTONS ──

Inspired by the classic "That was easy" button — the idea that some things should just be one press. Quick habits, frequent destinations, shortcuts you use every day. Stop digging through app drawers and just press the button.

── BACKUP & RESTORE ──

Export all your buttons to a JSON file anytime. Import them back on any device or after a reinstall. Your setup is always safe.
```

**Category:** Tools

**Tags** (add one at a time):
```
launcher
```
```
shortcuts
```
```
productivity
```
```
buttons
```
```
quick launch
```

**Contact email:**
```
voluntarytransactions@gmail.com
```

**Privacy policy URL:**
```
https://voluntarytransactions.com/easybuttons-privacy-policy.html
```

---

## Part 2 — Content rating (IARC questionnaire)

Play Console → Content ratings → Start questionnaire
- Category: **Utility**

| Question | Answer |
|---|---|
| Violence | No |
| Sexual content | No |
| Profanity | No |
| Controlled substances | No |
| Gambling | No |
| User-generated content | No |
| Data sharing with third parties for advertising | No |

Expected rating: **Everyone**

---

## Part 3 — Data safety

Play Console → Data safety

**Does your app collect or share any of the required data types?** No — the app stores button data only on-device (SQLite). No data is transmitted to any server.

| Section | Answer |
|---|---|
| Location data | Not collected |
| Personal info | Not collected |
| Financial info | Not collected |
| Health and fitness | Not collected |
| Messages | Not collected |
| Photos and videos | Not collected |
| Audio files | Not collected (sound files stay on device, never transmitted) |
| Files and docs | Not collected |
| Calendar | Not collected |
| Contacts | Not collected |
| App activity | Not collected |
| Web browsing | Not collected |
| App info and performance | Not collected |
| Device and other IDs | Not collected |

**Is your data encrypted in transit?** N/A — no data in transit.
**Can users request data deletion?** Yes — uninstalling the app removes all data.

---

## Part 4 — App access, ads, target audience

**App access:** All functionality is available without special access

**Ads:** No, this app does not contain ads

**Target audience:** 13 and older

**News app:** No

---

# ⚠️ IMAGE UPLOAD PAUSE

The following must be uploaded manually — browser AI cannot do file uploads:

1. **App icon** (512×512 PNG): `store/graphics/icon-512.png`
2. **Feature graphic** (1024×500 PNG): `store/graphics/feature-graphic.png`
3. **Phone screenshots**: `store/graphics/screenshots/en-US/` (add after first real-device build)

Tell the user: "Please upload the app icon and feature graphic from the store/graphics/ folder in the EasyButtons repo, then let me know when done."

---

# PHASE 2 — Verification sweep

---

## Part 5 — AAB upload

If not already uploaded:
- Play Console → Internal testing → Create new release
- Upload `bin/Release/net10.0-android/com.voluntarytransactions.easybuttons-Signed.aab`
- Release notes:
```
Initial release — create big colorful dome buttons that launch any app, website, URI, or sound in one tap. Long-press to edit. Export/import backup included.
```
- Save → Review release → Start rollout to Internal testing

## Part 6 — Verification sweep

Go through each section in Play Console and confirm green/complete:
- [ ] Store listing — title, descriptions, category, contact, privacy policy
- [ ] Content rating — complete, shows "Everyone"
- [ ] Data safety — complete
- [ ] App access — declared
- [ ] Ads — declared
- [ ] Target audience — set
- [ ] Internal testing release — AAB uploaded, rollout started
- [ ] Pricing — Free

Fix any incomplete sections, then confirm all green.

---

## Handoff back to Claude

When Phase 2 is complete, tell Claude:
> "EasyButtons submission is done. Promote to production and push the listing."

Claude will then:
1. Call `promote_release` from internal to production
2. Call `update_listing` to push the store text

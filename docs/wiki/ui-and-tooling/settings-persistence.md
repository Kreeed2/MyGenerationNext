---
title: Settings and layout persistence
summary: The shared DefaultSettings singleton stores preferences and connection profiles as XML, while MyGenerationMDI persists dock layout separately.
keywords: [settings, XML, persistence, recent files, connection profiles, layout]
source_paths: [mygeneration-legacy-code/mygeneration/MyGenUtility/DefaultSettings.cs, mygeneration-legacy-code/mygeneration/MyGeneration/MyGenerationMDI.cs, mygeneration-legacy-code/mygeneration/MyGeneration/Settings/DefaultSettings.xml, mygeneration-legacy-code/mygeneration/MyGeneration/Settings/MyGeneration.xml]
---

# Settings and layout persistence

## Shared settings model

`MyGenUtility/DefaultSettings.cs` is the application-wide settings implementation used by the launched `MyGenerationMDI`. Its constructor is private; `DefaultSettings.Instance` lazily constructs the singleton, `Refresh` clears the cached instance, and `DiscardChanges` reloads the document. `Load` reads a `DefaultSettings` XML element, creates it if missing/invalid, compares the stored version with the executable assembly version, fills defaults, then saves. `Save` writes modified saved connections, database-user-metadata aliases, and recent-file nodes before serializing the XML document.

The default `SettingsFileName` is `<UserDataPath>\Settings\DefaultSettings.xml`. The implementation's intended user profile path under `My Documents\MyGeneration` is guarded by `RUN_AS_NON_ADMIN`; in the checked-in source that conditional is disabled and `UserDataPath` returns `ApplicationPath`. Thus, do not document the user profile location as the active default. `-configfile` in `MyGenerationMDI` can override the file at startup. `ApplicationPath` derives from the entry assembly unless explicitly assigned.

Settings accessors include connection driver/string, language/target mapping paths and values, user metadata path, default template/output paths, timeout, UI preferences, window state/coordinates, and update behavior. Generic `GetSetting`/`SetSetting` add/read `Setting` elements on demand. Default template/output directory values point at expected subdirectories under the application location and fall back to that location when the expected subdirectory is absent.

## Persisted collections

- **Recent files:** `RecentFiles` is loaded lazily; `UpdateRecentFiles` rewrites `RecentFile` elements and caps persistence at 20 entries.
- **Saved connections:** `SavedConnections` loads `SavedSettings` nodes as `ConnectionInfo` objects. `UpdateSavedConnections` writes profile name, driver, connection string, mapping paths, language/target values, default-database-only, and nested alias mappings.
- **Database aliases:** `DatabaseUserDataXmlMappings` maps a database name to user metadata. Separate per-connection alias data is also represented.
- **Window geometry:** `WindowState`, `WindowPosLeft`, `WindowPosTop`, `WindowPosWidth`, and `WindowPosHeight` are ordinary settings saved when the shell closes.

`mygeneration-legacy-code/mygeneration/MyGeneration/Settings/DefaultSettings.xml` is a sample installed/settings file; it demonstrates `Setting` and `SavedSettings` shapes, not a guaranteed current-user configuration. Do not copy its sample provider or connection string as a production default.

## Separate files

The shell serializes docking layout separately to `<startupPath>\settings\dock.config` using `DockPanel.SaveAsXml`; it restores the file on load and deletes an unreadable layout before continuing. This is placement/visibility state, not application preferences. `Settings/MyGeneration.xml` is an older configuration resource with options and a Scintilla include; the active shell loads Scintilla from its designated configuration path rather than treating that file as the settings database.

`MyGeneration/DefaultSettings.cs` is an older, separate XML settings class that reads/writes `<startupPath>\Settings\DefaultSettings.xml` and has its own attribute-backed properties. The active `Program.Main` uses `MyGenerationMDI`, whose code imports the shared `MyGenUtility.DefaultSettings` singleton. Keep these similarly named implementations distinct when migrating or citing state.

## Migration implications

Use an explicit, versioned .NET settings format and a deterministic per-user path in the new app. Separate durable connection profiles from transient UI preferences, and never write raw credentials in clear text merely because the legacy XML profile stores a `connstr` attribute. Replacing legacy XML can be a migration concern; do not make dock-layout persistence a requirement for a TUI.

## Related pages

- [Connection and settings](connection-and-settings.md)
- [Main application](main-application.md)
- [Overview](overview.md)
- [TUI mapping](tui-mapping.md)

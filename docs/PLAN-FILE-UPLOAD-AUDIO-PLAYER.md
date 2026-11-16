# Shared file upload and audio player

Objective: add reusable SUI file selection and audio playback components and adopt them in sufficit-blazor's audio catalog, preserving the validated upload/publication flow.

## Checkpoints
1. Completed — lock public APIs and composition against existing SUI fields, tokens, native browser file/media semantics and the current audio screen.
2. Completed — implement components with executable catalog examples, documentation and meaningful interaction tests; regenerate CSS/catalog and pass library gates.
3. Completed — integrate in the audio catalog, update AI sidecar and remove its residual Mud icon/native markup; validate consumer tests and browser workflows at desktop/tablet/mobile, themes, keyboard and zoom.
4. In progress — commit/push both repositories, deploy the validated consumer by its official workflow, verify delivery and convert this plan to an activity.

Reference lock: existing DESIGN.md/SUI tokens and field components; user's validated audio workflow; native file chooser and media controls for platform accessibility. No feature CSS or new dependency. File chooser remains keyboard accessible, selected filename wraps, external clearing resets native input. Audio does not autoplay and respects the available width. File acceptance hints and optional client size limits do not replace server validation. Publication and actual central application remain distinct.

Validation: library Release tests/build, CSS/catalog gates and executable examples; consumer AudioCatalog/AIChat tests and synthetic browser workflows. No customer audio changes as tests.

Library checkpoint: 1,185 tests passed; 80 copied catalog examples compiled; CSS build/check and 15 Python release tests passed. Public API additions reviewed and baseline regenerated; README/catalog count updated from 78 to 80. Scoped CSS total is 51,792 B, with 53,504 B ceiling per measured ×1.03 rounded to 256. Existing HEAD JavaScript (unchanged by this task) already measured 14,706 B against stale 14,336 B budget; the same documented formula gives 15,360 B. No JS source was changed or added. Both components compile without warnings as errors.

Consumer checkpoint: AudioCatalogPage now uses SUIFileUpload and SUIAudioPlayer, with no direct InputFile/audio element, inline media style or Mud icon. Selection still never sends the file before footer confirmation; discard resets the native input. AI sidecar documents both components. 226 focused consumer tests passed; Release build includes client/server/tests. Browser workflow passed 1440/768/390, light/dark, 200% zoom and reduced motion: long filename wrap, native reset, size rejection, real synthetic WAV playback, download and media-error recovery. Axe scoped to both new components reported zero violations in all four scenarios. Existing synthetic publication workflow passed at 1440/768/390. Inspected screenshots /tmp/audio-sui-upload-390-false.png and /tmp/audio-sui-player-390-false.png. No customer media used.

Package checkpoint: dotnet pack and scripts/validate-package.sh passed, including executable package consumers at root/pathbase. Local package 2.26.1116.2012 is a validation artifact, not a claimed NuGet.org release. Public API baseline adds 31 signatures and removes none. No JavaScript added. Shared native controls remain inside library components; server validation and media authorization remain owned by the application.

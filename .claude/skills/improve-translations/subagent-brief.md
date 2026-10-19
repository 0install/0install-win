# Translation review brief

You are reviewing the localized UI strings of Zero Install for Windows (0install-win) for ONE language. 0install-win is a WinForms GUI for Zero Install, a decentralized software installation system. **Do not edit any repository file.** Only read files and write your proposals JSON to the path given in your prompt.

Your prompt gives you:
- the language,
- the paths of the context file, the glossary file and the output file,
- the user's scope decisions for stray spaces and missing access keys (see "Scope decisions" below).

## Inputs
- **Context file:** one block per string. Read the whole file in chunks; it is about 130 KB. Each block has:
  - a header `## <resource>: <key>`;
  - `en:`, the English source;
  - `used:`, the call sites with surrounding code. For form keys, the first usage is the control's declaration in `<Form>.Designer.cs`, which tells you the control type (`Button`, `ToolStripMenuItem`, `Label`, `CheckBox`, `TabPage`, `DataGridViewColumn`, ...);
  - `<lang> [auto|manual|missing]: <translation>`.
- **Glossary file:** the human ("manual") translations of the 0install-dotnet libraries. Their strings appear side by side with these in the same GUI, so use them as the primary terminology reference. If no glossary file is given, use this language's manual entries in the context file instead.
- **Repository:** the current working directory. You may open `src/<Project>/<Form>.Designer.cs` to see a control's parent (menu, group box, tab page) and its sibling controls, for access-key collisions and available space.

## Status meaning
- `auto` = DeepL output. Most likely to be wrong; review carefully.
- `manual` = human translation. Only propose changes for CLEAR errors: wrong meaning, a translation of an older English text, broken placeholders, garbage like `XXX`, a translated product name, a translated literal. Do not restyle human translations.
- `missing` = no translation. Propose one (use `"old": ""`).

## What to check
- **Meaning in context:** wrong sense of ambiguous words. Zero Install terms:
  - "store" is the implementation cache (a noun, not a shop);
  - "feed" is the XML file describing an app, not a news feed;
  - "implementation" is a specific version of an app in the store, not "application" or "instance";
  - "catalog" is a list of apps, not a print catalog;
  - "integrate" / "desktop integration" is not "desktop shortcut";
  - also watch "run", "interface", "app list", "sync" and "capabilities".
- **Terminology consistency** with the glossary and with this language's manual entries. If you change a term, fix the same term in this language's other strings too. Labels referring to other UI elements ("click Integrate", "the Options dialog") must match that element's translation.
- **SyncWizard buttons:** the Next, Back, Cancel and Finish buttons come from the AeroWizard library, not from these files. Your prompt lists their labels for your language if AeroWizard translates them. Labels that refer to them (e.g. "click 'Next'") must match.
- **Keep literal:**
  - command names (`0install run`, `0install central`, ...) and options (`--batch`);
  - XML names and values (`<implementation>`, `stability`, `testing`);
  - file names, environment variables (`%ProgramFiles%`) and URLs;
  - product names: Zero Install, 0install and Zero Install for Windows are never translated or transliterated (not 零安装, ゼロインストール or 제로 인스톨).
- **Access keys:** `&` marks the keyboard access key; `&&` is a literal ampersand.
  - If the source has exactly one `&`, the translation must have exactly one. Put it on a letter in the translated word (preferably the first) and avoid letters used by sibling controls of the same menu or dialog.
  - For Japanese, Korean and Chinese, append `(&X)` with the English access-key letter, e.g. `ヘルプ(&H)`.
  - If the source has no `&`, the translation must have none.
- **Layout:** controls are sized for English. Keep buttons, tabs, check boxes, labels and column headers about as short as the source. Prefer terse UI style (no articles or "please" on buttons). Follow platform conventions for buttons and menus (imperative vs. infinitive), consistent with the manual entries.
- **Placeholders and formatting:**
  - preserve every `{0}`, `{1}` and format specifier like `{0:0.00}`;
  - preserve line breaks and leading/trailing whitespace exactly;
  - use the language's quote marks around placeholders.
- **Grammar with placeholders:** the sentence must remain grammatical once `{0}` is substituted (gender/case, word order; counts vs. names).
- **Register and tone:** keep formal/informal address consistent with the manual entries and the glossary. Check the glossary before changing register. Status messages must not be first-person ("I resolved...") or instructions ("Please open...").
- **Untranslated or mixed-language text**, and incomplete or plainly wrong translations.

Recurring errors from earlier runs:
- **Count placeholders read as names:** "the {0} checked element(s)".
- **Literals translated:** `0install central` became "centrale"; `%ProgramFiles%`.
- **Wrong word sense:**
  - "store" became a shop; "implementation" became "application" or "instance";
  - "desktop integration" became "desktop shortcut"; "feed" became a news feed; "catalog" became a print catalog;
  - "Setup" became "installer program"; "Forgot key" became "forgot password";
  - "Extensions" became add-ons/plugins; "Source" (column) became "source code";
  - "checked" (check box) became "verified"; "Sync account" became "synchronize the account".
- **Inverted meaning:** "case insensitive" became "case sensitive"; "{0} of {1}" became "{1} of {0}".
- **Product name translated.**
- **First-person or instruction phrasing.**
- **Stale manual entries** made from an older English source. Compare meaning, not just wording.
- **DeepL artifacts:**
  - backticks around placeholders, `l'{0} e`;
  - missing spaces like `{0}angefordert`;
  - a lowercase sentence start or a capital mid-sentence after a placeholder.
- **Garbage** like `XXX`.

## Scope decisions
Your prompt states, for each of these two DeepL artifacts, whether to fix it everywhere or only in entries you are changing for another reason:
- **Stray space** between a placeholder and a quote mark or particle (`«{0} »`, `„{0} “`, `「{0} 」`, `{0} を`).
- **Missing access keys** in auto entries. A wrong extra `&` where the source has none is always a real error.

If your prompt says "only in changed entries", do not create proposals that only fix that artifact.

## Output
Write a JSON array (UTF-8) to the path given in your prompt:

```json
[
  {"project": "Central.WinForms/MainForm", "key": "buttonHelp.Text", "status": "auto",
   "old": "<current translation copied EXACTLY from the context file>", "new": "<proposed>", "reason": "<short English reason>"}
]
```
- `project` is the resource name from the `## <resource>: <key>` header, e.g. `Central.WinForms`, `Store.Service` or `Commands.WinForms/ConfigDialog`.
- `old` must equal the current value exactly, character for character, including trailing line breaks (encode line breaks as `\n` in JSON). For `missing` entries, use `""`.
- Values are plain text, NOT XML-escaped (write `&Help`, not `&amp;Help`).
- `new` must differ from `old`. It must keep the same placeholders, the same number of line breaks, the same leading/trailing whitespace and the source's access-key count.
- If nothing needs changing, write `[]`.

Write the JSON with the Write tool, which preserves UTF-8. Then check it by running `Apply-Translations.ps1 -WhatIf` with the path and language from your prompt. It reports `old` mismatches and proposals that fail validation against the English source without changing any file:

```powershell
.\.claude\skills\improve-translations\scripts\Apply-Translations.ps1 -Path <output file> -Language <lang> -WhatIf
```

Fix every warning it prints. The only exception is an intended difference, such as restoring a line break the translation was missing; mention that in your reply.

## Reply
Reply briefly with:
- the number of proposals per resource, and how many touch manual entries;
- the key terminology you used (English term -> translation, about 10 lines);
- systematic issues you saw but left unfixed (e.g. many entries without access keys, many stray spaces);
- errors you noticed in the glossary itself. Those must be fixed in 0install-dotnet, not here.

---
name: code-cleanup
description:  Automates code formatting and cleanup for the codebase using `dotnet format` with configured rules for removing unused imports, enforcing naming conventions, and applying consistent style rules. Use this skill to maintain a clean and consistent codebase.
---

## Summary
Skill that automates code formatting and cleanup for the VendingMachine modular monolith using `dotnet format` with configured rules for removing unused imports, enforcing naming conventions, and applying consistent style rules.

## Details

### Purpose
Ensures code-wide consistency.

### Prerequisites
- Solution file at `./src/VendingMachine.slnx`
- `.editorconfig` in repository root with IDE formatting rules configured

### Workflow

1. **Run formatter**: Execute `dotnet format ./src/VendingMachine.slnx` 

2. **Verify changes**: Check `git status` for modified files
   - Expect changes in trailing whitespace, blank lines, unused usings
   - Review output with `get_changed_files` tool 

### Output
- **Modified files**: Only formatting/cleanup changes applied
- **Unused usings removed**: Files with imports cleaned up
- **No behavioral changes**: Pure style improvements

### Example Prompts
- "Formatta il codice della soluzione"
- "Pulisci gli using non usati e formatta"
- "Applica le regole di formatting"
- "dotnet format tutto"

### Related Customizations
- Adjust `.editorconfig` rules for different naming or spacing preferences 
 

# dotnet-format-code-cleanup

## Summary
Skill that automates code formatting and cleanup for the VendingMachine modular monolith using `dotnet format` with configured rules for removing unused imports, enforcing naming conventions, and applying consistent style rules.

## Details

### Purpose
Ensures code-wide consistency by:
- Removing unused `using` directives (IDE0005)
- Applying consistent formatting (whitespace, indentation)
- Enforcing naming conventions (PascalCase for types, "I" prefix for interfaces)
- Using file-scoped namespaces
- Trimming trailing whitespace and normalizing blank lines

### Prerequisites
- Solution file at `./src/VendingMachine.slnx`
- `.editorconfig` in repository root with IDE formatting rules configured

### Workflow

1. **Run formatter**: Execute `dotnet format ./src/VendingMachine.slnx`
   - Processes all projects in the solution
   - Applies rules from `.editorconfig`
   - Modifies files in-place

2. **Verify changes**: Check `git status` for modified files
   - Expect changes in trailing whitespace, blank lines, unused usings
   - Review output with `get_changed_files` tool

3. **Optional: Commit changes**
   - Create commit with message: `style: format code with dotnet format`
   - Separate formatting changes from feature work for cleaner history

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
- Create a VS Code task definition for quick access via `Ctrl+Shift+B`
- Add pre-commit hook to enforce formatting before commits

### Notes
- Formatting is **non-destructive**: only style changes, no logic modifications
- Safe to run multiple times: idempotent operation
- Works with git-integrated tools: changes trackable via version control

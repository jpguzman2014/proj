#!/usr/bin/env bash
# Copy the Launch Check skill, principle, and checklist into another project.
#
#   ./scripts/apply-to-project.sh /path/to/project
#
# Safe to re-run: the skill is refreshed, and CLAUDE.md and CHECKLIST.md are only
# created if absent so local edits are never overwritten.

set -euo pipefail

TARGET="${1:-}"
SRC="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if [[ -z "$TARGET" ]]; then
  echo "usage: $(basename "$0") <path-to-project>" >&2
  exit 2
fi
if [[ ! -d "$TARGET" ]]; then
  echo "error: no such directory: $TARGET" >&2
  exit 1
fi

TARGET="$(cd "$TARGET" && pwd)"
if [[ "$TARGET" == "$SRC" ]]; then
  echo "error: target is the launch-check repo itself" >&2
  exit 1
fi

# 1. The skill, so Claude can run the audit in that project.
mkdir -p "$TARGET/.claude/skills"
rm -rf "$TARGET/.claude/skills/launch-check"
cp -R "$SRC/skills/launch-check" "$TARGET/.claude/skills/launch-check"
echo "installed  .claude/skills/launch-check"

# 2. The checklist, so audit state is tracked in the project.
if [[ -e "$TARGET/CHECKLIST.md" ]]; then
  echo "kept       CHECKLIST.md (already present)"
else
  cp "$SRC/skills/launch-check/assets/CHECKLIST.md" "$TARGET/CHECKLIST.md"
  echo "installed  CHECKLIST.md"
fi

# 3. The principle, so it holds while code is being written.
if [[ ! -e "$TARGET/CLAUDE.md" ]]; then
  cp "$SRC/CLAUDE.md" "$TARGET/CLAUDE.md"
  echo "installed  CLAUDE.md"
elif grep -q "Launch Check" "$TARGET/CLAUDE.md"; then
  echo "kept       CLAUDE.md (already references Launch Check)"
else
  {
    echo
    echo "---"
    echo
    cat "$SRC/CLAUDE.md"
  } >> "$TARGET/CLAUDE.md"
  echo "appended   CLAUDE.md"
fi

echo
echo "Done. In $TARGET, run: claude \"/launch-check\""

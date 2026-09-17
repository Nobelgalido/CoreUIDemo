#!/usr/bin/env bash
# Stop hook: keep docs/BUILD_GUIDE.md, docs/ARCHITECTURE.md, docs/COREUI_GUIDE.md and README.md
# in sync with the code. If source files changed during this session (committed or not) and
# none of the docs did, block the stop and hand Claude the list so it reconciles the docs.
#
# Usage: docs-sync.sh session-start   -> records the session start marker
#        docs-sync.sh check           -> the Stop hook (reads hook JSON on stdin)

set -u
cd "$(dirname "$0")/../.." || exit 0
MARKER=".claude/.session-start"

if [ "${1:-check}" = "session-start" ]; then
  mkdir -p .claude
  date -u +%Y-%m-%dT%H:%M:%SZ > "$MARKER"
  exit 0
fi

INPUT="$(cat)"
# Already continuing from a previous block in this turn — never loop.
case "$INPUT" in *'"stop_hook_active":true'*|*'"stop_hook_active": true'*) exit 0 ;; esac

git rev-parse --is-inside-work-tree >/dev/null 2>&1 || exit 0

SINCE=""
[ -f "$MARKER" ] && SINCE="$(cat "$MARKER")"

# Files touched this session: commits since the marker + anything uncommitted/untracked.
{
  if [ -n "$SINCE" ]; then
    git log --since="$SINCE" --name-only --pretty=format: 2>/dev/null
  fi
  git status --porcelain --untracked-files=all 2>/dev/null | sed 's/^...//' | sed 's/.* -> //'
} | sed 's#\\#/#g' | grep -v '^\s*$' | sort -u > /tmp/.docs-sync-changed.$$ 2>/dev/null || true
CHANGED="$(cat /tmp/.docs-sync-changed.$$ 2>/dev/null)"; rm -f /tmp/.docs-sync-changed.$$
[ -z "$CHANGED" ] && exit 0

SRC_RE='^(Controllers/|Services/|Helpers/|App_Start/|App/|Views/|Models/[^/]*\.(cs|edmx)$|Database/|Content/Site\.css$|Web\.config$|Global\.asax\.cs$|packages\.config$|CoreUIDemo\.csproj$)'
DOC_RE='^(docs/BUILD_GUIDE\.md|docs/ARCHITECTURE\.md|docs/COREUI_GUIDE\.md|README\.md)$'

SRC_CHANGED="$(printf '%s\n' "$CHANGED" | grep -E "$SRC_RE" | grep -Ev '^Models/LoginDemoEntities\.(Context\.cs|Designer\.cs|cs)$' || true)"
DOC_CHANGED="$(printf '%s\n' "$CHANGED" | grep -E "$DOC_RE" || true)"

[ -z "$SRC_CHANGED" ] && exit 0
[ -n "$DOC_CHANGED" ] && exit 0

LIST="$(printf '%s\n' "$SRC_CHANGED" | sed 's/^/  - /')"
REASON="Docs sync check: these source files changed this session but none of the project docs did:
$LIST

Before finishing, reconcile the documentation with the code:
  - docs/BUILD_GUIDE.md   -> the file listing / step for each changed file (code blocks are complete files; update them verbatim)
  - docs/ARCHITECTURE.md  -> folder map, request flows, endpoint table, data model, mapping tables, known limitations
  - docs/COREUI_GUIDE.md  -> only if theme/vendoring/bundle/layout rules changed
  - README.md             -> only if stack, prerequisites or quick start changed
If a change genuinely needs no doc update, say so explicitly and make a one-line note in the relevant doc section (e.g. a version/date line) so the docs remain the source of truth."

# Escape for JSON (backslash, double quote, newline).
ESC="$(printf '%s' "$REASON" | sed 's/\\/\\\\/g; s/"/\\"/g' | awk 'BEGIN{ORS="\\n"} {print}' | sed 's/\\n$//')"
printf '{"decision":"block","reason":"%s"}\n' "$ESC"
exit 0

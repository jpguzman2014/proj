#!/usr/bin/env bash
#
# Type-checks the Desk Quest C# scripts without a Unity installation.
#
# What this does and does not prove is described in README.md. In short: it catches
# syntax errors, typos, bad signatures and broken references inside our own code. It
# cannot prove the project compiles in Unity, because the Unity API it compiles against
# is a hand-written stub, not the real thing.

set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
STUBS="$ROOT/Tools/CompileCheck/Stubs"
SCRIPTS="$ROOT/Assets/DeskQuest/Scripts"
EDITOR="$ROOT/Assets/DeskQuest/Editor"
OUT="${TMPDIR:-/tmp}/deskquest-compilecheck"

mkdir -p "$OUT"

if ! command -v mcs >/dev/null 2>&1; then
    echo "mcs (Mono C# compiler) not found. Install with: apt-get install -y mono-devel"
    exit 127
fi

stub_files() { find "$STUBS" -name '*.cs' | sort; }
runtime_files() { find "$SCRIPTS" -name '*.cs' | sort; }
editor_files() { find "$EDITOR" -name '*.cs' | sort; }

failures=0

run_pass() {
    local label="$1"; shift
    local defines="$1"; shift

    local define_args=()
    if [ -n "$defines" ]; then
        define_args=(-define:"$defines")
    fi

    echo "--- $label ---"
    if mcs -target:library -nologo -langversion:latest \
           -out:"$OUT/$(echo "$label" | tr ' /' '__').dll" \
           "${define_args[@]}" "$@" 2>&1 | sed 's/^/    /'; then
        echo "    PASS"
    else
        echo "    FAIL"
        failures=$((failures + 1))
    fi
}

# The input backend is a compile-time branch, so both sides need checking. A project
# created from Unity's AR template uses the new Input System; a plain 3D project
# usually does not.
run_pass "runtime (new Input System)" "ENABLE_INPUT_SYSTEM" $(stub_files) $(runtime_files)
run_pass "runtime (legacy input)"     ""                    $(stub_files) $(runtime_files)
run_pass "runtime + editor"           "ENABLE_INPUT_SYSTEM" $(stub_files) $(runtime_files) $(editor_files)

# Movement rules are pure maths with no Unity dependency, so they can be genuinely
# tested here rather than only compiled. This is the one part of the project that is
# verified rather than merely checked.
echo "--- motor unit tests ---"
TEST_EXE="$OUT/MotorTests.exe"
if mcs -target:exe -nologo -langversion:latest -out:"$TEST_EXE" \
       $(stub_files) $(find "$ROOT/Assets/DeskQuest/Scripts/Gameplay" -name '*.cs' | sort) \
       $(find "$ROOT/Tools/CompileCheck/Tests" -name '*.cs' | sort) 2>&1 | sed 's/^/    /'; then
    if mono "$TEST_EXE" 2>&1 | sed 's/^/    /'; then
        :
    else
        failures=$((failures + 1))
    fi
else
    echo "    FAIL (tests did not compile)"
    failures=$((failures + 1))
fi

echo
if [ "$failures" -eq 0 ]; then
    echo "All passes compiled cleanly and all motor tests passed."
else
    echo "$failures check(s) failed."
fi

exit "$failures"

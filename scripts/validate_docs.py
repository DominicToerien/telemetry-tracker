#!/usr/bin/env python3
"""Validate objective invariants of the repository's Markdown documentation."""

from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote


ROOT = Path(__file__).resolve().parent.parent
REQUIRED_FILES = (
    "AGENTS.md",
    "agent/current.md",
    "agent/rules.md",
    "agent/plans/current.md",
    "agent/plans/roadmap.md",
    "agent/decisions/README.md",
)
LINK_PATTERN = re.compile(r"!?\[[^\]]*\]\(([^)]+)\)")
EXTERNAL_PREFIXES = ("http://", "https://", "mailto:")


def markdown_files() -> list[Path]:
    return sorted(
        path
        for path in ROOT.rglob("*.md")
        if ".git" not in path.parts and "bin" not in path.parts and "obj" not in path.parts
    )


def local_link_target(raw_target: str) -> str:
    target = raw_target.strip()
    if target.startswith("<") and ">" in target:
        target = target[1 : target.index(">")]
    else:
        target = target.split(maxsplit=1)[0]
    return unquote(target.split("#", 1)[0])


def main() -> int:
    errors: list[str] = []

    for relative_path in REQUIRED_FILES:
        if not (ROOT / relative_path).is_file():
            errors.append(f"missing required documentation file: {relative_path}")

    agent_guide = ROOT / "AGENTS.md"
    if agent_guide.is_file() and len(agent_guide.read_text(encoding="utf-8").split()) > 1_200:
        errors.append("AGENTS.md exceeds the 1,200-word routing budget")

    baseline_words = 0
    for relative_path in ("AGENTS.md", "agent/current.md", "agent/rules.md"):
        path = ROOT / relative_path
        if path.is_file():
            baseline_words += len(path.read_text(encoding="utf-8").split())
    if baseline_words > 1_500:
        errors.append(f"minimal startup context is {baseline_words} words; limit is 1,500")

    for path in markdown_files():
        text = path.read_text(encoding="utf-8")
        relative_path = path.relative_to(ROOT)

        if "/abs/path" in text or re.search(r"(?i)c:/users/", text):
            errors.append(f"{relative_path}: contains a machine-specific path")

        for match in LINK_PATTERN.finditer(text):
            raw_target = match.group(1)
            target = local_link_target(raw_target)
            if target.startswith(EXTERNAL_PREFIXES) or target.startswith("#"):
                continue
            if not target:
                continue
            if target.startswith("/"):
                errors.append(f"{relative_path}: repository link must be relative: {raw_target}")
                continue

            resolved = (path.parent / target).resolve()
            try:
                resolved.relative_to(ROOT)
            except ValueError:
                errors.append(f"{relative_path}: link escapes repository: {raw_target}")
                continue
            if not resolved.exists():
                errors.append(f"{relative_path}: broken local link: {raw_target}")

    if errors:
        print("Documentation validation failed:")
        for error in errors:
            print(f"- {error}")
        return 1

    print(f"Documentation validation passed ({len(markdown_files())} Markdown files, {baseline_words} startup words).")
    return 0


if __name__ == "__main__":
    sys.exit(main())

#!/usr/bin/env python3
"""Install reviewed local snapshots; never fetch or overwrite untracked edits."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import uuid

NAME = "sufficit-frontend"
RECEIPT = ".sufficit-installation.json"


def file_hashes(root):
    result = {}
    for path in sorted(root.rglob("*")):
        if path.is_symlink():
            raise ValueError(f"Unexpected symlink in bundle: {path}")
        if path.is_file() and path.name != RECEIPT and "__pycache__" not in path.parts:
            result[path.relative_to(root).as_posix()] = hashlib.sha256(path.read_bytes()).hexdigest()
    return result


def destinations(home, targets):
    roots = {"codex": home / ".codex/skills", "claude": home / ".claude/skills",
             "zcode": home / ".zcode/skills", "symposium": home / ".symposium/repo/skills",
             "symposium-compat": home / ".symposium/skills"}
    if home == Path.home() and os.environ.get("CODEX_HOME"):
        roots["codex"] = Path(os.environ["CODEX_HOME"]) / "skills"
    selected = list(roots) if "all" in targets else list(dict.fromkeys(targets))
    if "symposium" in selected and "symposium-compat" not in selected:
        selected.append("symposium-compat")
    return [(name, roots[name] / NAME) for name in selected]


def revision(source):
    proc = subprocess.run(["git", "-C", str(source), "log", "-1", "--format=%H", "--", "."],
                          capture_output=True, text=True)
    return proc.stdout.strip() if proc.returncode == 0 else None


def expected(source, profile, version):
    hashes = file_hashes(source)
    # Symposium's frontmatter parser expects version at the top level.
    if profile.startswith("symposium"):
        body = (source / "SKILL.md").read_text()
        body = body.replace("---\n", f'---\nversion: "{version}"\n', 1)
        hashes["SKILL.md"] = hashlib.sha256(body.encode()).hexdigest()
    return hashes


def inspect(target, wanted):
    if target.is_symlink():
        return "legacy-link", None
    if not target.exists():
        return "missing", None
    receipt_path = target / RECEIPT
    if not receipt_path.is_file():
        return "unmanaged", None
    receipt = json.loads(receipt_path.read_text())
    if receipt.get("name") != NAME:
        return "unmanaged", receipt
    actual = file_hashes(target)
    if actual != receipt.get("files"):
        return "modified", receipt
    return ("current" if actual == wanted else "update-available"), receipt


def install(source, target, profile, meta, wanted, update, migrate_from):
    state, previous = inspect(target, wanted)
    if state == "current":
        return
    allowed_link = (state == "legacy-link" and migrate_from is not None
                    and target.resolve() == migrate_from.resolve())
    if state != "missing" and not (state == "update-available" and update) and not allowed_link:
        raise ValueError(f"{target}: {state}; preserve it and review before replacing")
    target.parent.mkdir(parents=True, exist_ok=True)
    stage = Path(tempfile.mkdtemp(prefix=f".{NAME}-stage-", dir=target.parent))
    backup = target.with_name(f".{NAME}-backup-{uuid.uuid4().hex}")
    moved = False
    try:
        shutil.copytree(source, stage, dirs_exist_ok=True,
                        ignore=shutil.ignore_patterns(RECEIPT, "__pycache__", "*.pyc"))
        if profile.startswith("symposium"):
            manifest = stage / "SKILL.md"
            manifest.write_text(manifest.read_text().replace("---\n", f'---\nversion: "{meta["version"]}"\n', 1))
        if file_hashes(stage) != wanted:
            raise ValueError("Source changed during copy; installation aborted")
        receipt = {**meta, "profile": profile, "sourceRevision": revision(source), "files": wanted}
        (stage / RECEIPT).write_text(json.dumps(receipt, indent=2) + "\n")
        if target.exists() or target.is_symlink():
            target.rename(backup)
            moved = True
        try:
            stage.rename(target)
            if file_hashes(target) != wanted:
                raise ValueError("Installed content failed verification")
        except Exception:
            if target.exists():
                shutil.rmtree(target)
            if moved:
                backup.rename(target)
                moved = False
            raise
        if moved:
            if backup.is_symlink():
                backup.unlink()
            else:
                shutil.rmtree(backup)
            moved = False
    finally:
        if stage.exists():
            shutil.rmtree(stage)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["install", "status"])
    parser.add_argument("--target", action="append", choices=["all", "codex", "claude", "zcode", "symposium"], default=[])
    parser.add_argument("--home", type=Path, default=Path.home(), help="Alternate home for isolated validation")
    parser.add_argument("--source", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--update", action="store_true", help="Replace an unchanged managed snapshot")
    parser.add_argument("--migrate-from", type=Path, help="Replace only a legacy link pointing to this exact source")
    args = parser.parse_args()
    source = args.source.resolve()
    meta = json.loads((source / "release.json").read_text())
    if meta["name"] != NAME or not (source / "SKILL.md").is_file():
        parser.error("Invalid source bundle")
    failed = False
    for profile, target in destinations(args.home, args.target or ["all"]):
        wanted = expected(source, profile, meta["version"])
        try:
            if args.action == "install":
                install(source, target, profile, meta, wanted, args.update, args.migrate_from)
            state, receipt = inspect(target, wanted)
            print(json.dumps({"target": profile, "path": str(target), "state": state,
                              "version": receipt.get("version") if receipt else None,
                              "sourceVersion": meta["version"],
                              "revision": receipt.get("sourceRevision") if receipt else None}))
            failed |= state != "current"
        except (ValueError, OSError) as error:
            failed = True
            print(json.dumps({"target": profile, "path": str(target), "error": str(error)}))
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())

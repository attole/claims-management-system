"""Read-only consistency check for the AI-workflow files of this repository.

Run from anywhere:  python -I .claude/tools/check_workflow.py
Prints one line per finding and one summary line. Exit code 1 when anything is found.
Standard library only. Never writes, never calls git, never prints document text.
"""

import json
import re
import sys
from pathlib import Path

ROOT = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else Path(__file__).resolve().parents[2]
SKIP_DIRS = {".git", "node_modules", "bin", "obj", "dist", ".angular", ".vs", ".idea"}
MESSAGE_ALLOWED = re.compile(
    r"^backend/src/ClaimsModule\.Domain/(.+/Rules/[^/]+|Core/Flows/Transition)\.cs$"
    r"|^backend/tests/ClaimsModule\.Domain\.Tests/(.+/Rules/[^/]+Tests|Claims/Flows/CloseClaimTransitionTests)\.cs$")
TEXT_SUFFIXES = {".md", ".json", ".py", ".ts", ".cs", ".html", ".scss", ".yml", ".yaml", ".editorconfig"}
LOCAL_DOCS = ["docs/spec.local.md", "docs/assessment.local.md"]
REGISTER = "docs/rules.local.md"
AREAS = {"claim", "party", "status", "reserve", "job", "document", "audit", "validation", "api", "data", "ui"}
DOMAIN = "backend/src/ClaimsModule.Domain"
DOMAIN_TESTS = "backend/tests/ClaimsModule.Domain.Tests"
TEST_WITHOUT_SOURCE = {"EntityConstructionTests.cs"}
BOOL_PREFIX = re.compile(r"^_?(?:Is|Has|Can|Should|is|has|can|should)[A-Z]")
BOOL_MEMBER = re.compile(
    r"^\s*(?:public|internal|protected|private)(?: (?:protected|internal|static|abstract|override|virtual|sealed|readonly|required|new))* bool (\w+)",
    re.M)
BOOL_EXEMPT = {"Equals", "Authorize"}
TEST_METHOD = re.compile(r"\[(?:Fact|Theory)\](?:\s*\[[^\]]*\])*\s*public (?:async )?(?:void|Task) (\w+)\(")
INTERFACE = re.compile(r"^\s*(?:public|internal)(?: partial)? interface (\w+)", re.M)
ENTITY_SUBFOLDERS = {"Inputs", "Flows", "Events", "Rules"}
AGGREGATE_FOLDERS = ("Claims", "ReserveComponents", "Policies", "ClaimDocuments", "CauseOfLoss")
LAYER_FORBIDS = {
    "Common": ["Claims", "Core", "Roles"],
    "Core/Rules": ["Core.Flows", "Claims", "Roles"],
    "Core/Flows": ["Claims", "Roles"],
    "Roles": ["Claims", "ReserveComponents", "Policies", "ClaimDocuments", "CauseOfLoss"],
    "Claims": ["ReserveComponents", "ClaimDocuments"],
    "ReserveComponents": ["ClaimDocuments"],
    "ClaimDocuments": ["ReserveComponents"],
    "Policies": ["Claims", "ReserveComponents", "ClaimDocuments", "CauseOfLoss"],
    "CauseOfLoss": ["Claims", "ReserveComponents", "ClaimDocuments", "Policies"],
}
SKILLS = ["design", "explore", "code-review", "dev-log", "spec-check", "ai-journal"]
SKELETON = ["Triggers", "Procedure", "Output", "Handoff", "Guardrails"]
CREATED_ON_REQUEST = {".claude/temp/review-rule-coverage.md"}
JOURNAL_KEYS = {
    "PROMPT": (["purpose", "prompt", "outcome", "note"], {"outcome": {"kept", "changed", "dropped"}}),
    "CORRECTION": (["what", "caught-by", "fix", "lesson"],
                   {"caught-by": {"user", "code-review", "spec-check", "test", "build"}}),
    "SPLIT": (["item", "who", "note"], {"who": {"ai", "manual", "hybrid"}}),
    "ASSESSMENT": (["area", "helped", "note"], {"helped": {"high", "medium", "low"}}),
    "SESSION": (["name", "phase", "note"], {}),
    "SUMMARY": (["topic", "story", "decisions", "ai-errors", "human-vs-ai", "refs"], {}),
}
JOURNAL_VALUE_LIMITS = {"SUMMARY": 600}
JOURNAL_DEFAULT_LIMIT = 200
SHOULD_BLOCK = [
    "git switch main -f", "git switch main --force", "git -C . commit -m x", "git -c a=b push", "git stash",
    "git stash pop", "git branch -v -D old", "git branch --delete --force old", "git pull", "git commit -m x",
    "git push --force", "git add .", "git reset --hard", "git clean -fd", "bash -c 'git commit'",
    "/usr/bin/git push", "git status && git push", "FOO=1 git push", "git reflog expire --all", "git gc",
    "env git push", "git remote add o u", "git apply p.patch",
]
SHOULD_PASS = [
    "git switch main", "git switch feature-fix", "git switch -c feature/x", "git stash list", "git stash show",
    "git branch -a", "git log --oneline", "git diff HEAD", "git status", "git fetch", "git remote -v",
    "git config --get user.name", "dotnet build backend", "npm --prefix frontend test",
]

findings = []
ran = 0


def line_limit():
    m = re.search(r"\*\*Line length:\*\* (\d+) characters", read(".claude/CLAUDE.md") or "")
    if m:
        return int(m.group(1))
    fail("code", "CLAUDE.md 'Code style' states no line length")
    return 10**6


def fail(check, detail):
    findings.append(f"FAIL {check}: {detail}")


def read(rel):
    path = ROOT / rel
    return path.read_text(encoding="utf-8") if path.exists() else None


def tracked_files():
    for path in ROOT.rglob("*"):
        if not path.is_file() or any(part in SKIP_DIRS for part in path.relative_to(ROOT).parts):
            continue
        rel = path.relative_to(ROOT).as_posix()
        if rel.endswith(".local.md") or rel.startswith("docs/") and rel.endswith(".docx"):
            continue
        yield rel, path


def check_ascii():
    for rel, path in tracked_files():
        if path.suffix in TEXT_SUFFIXES or path.name in {".gitignore", ".editorconfig"}:
            text = path.read_text(encoding="utf-8", errors="replace")
            bad = sorted({c for c in text if ord(c) > 127})
            if bad:
                fail("ascii", f"{rel} has non-ASCII characters {bad}")


def check_citations():
    claude = read(".claude/CLAUDE.md") or ""
    heads = {re.sub(r"`", "", m.group(1)).strip() for m in re.finditer(r"^#{1,4} (.+)$", claude, re.M)}
    labels = {m.group(1).strip().rstrip(":") for m in re.finditer(r"\*\*([^*]+)\*\*", claude)}
    for rel, path in tracked_files():
        if path.suffix != ".md":
            continue
        for m in re.finditer(r"(?:CLAUDE\.md |per |see |\()'([^']{3,80})'", path.read_text(encoding="utf-8")):
            cite = re.sub(r"`", "", m.group(1))
            in_heads = any(cite == h or h.startswith(cite) or cite.startswith(h) for h in heads)
            in_labels = any(cite.rstrip(":").startswith(x) or x.startswith(cite.rstrip(":")) for x in labels)
            if not (in_heads or in_labels):
                fail("citation", f"{rel}: '{cite}' is not a heading or label in CLAUDE.md")


def check_architecture_citations():
    doc = read("docs/ARCHITECTURE.md")
    if doc is None:
        return fail("citation", "docs/ARCHITECTURE.md is missing")
    heads = {re.sub(r"`", "", m.group(1)).strip() for m in re.finditer(r"^#{1,4} (.+)$", doc, re.M)}
    for rel, path in tracked_files():
        if path.suffix != ".md":
            continue
        text = path.read_text(encoding="utf-8")
        for m in re.finditer(r"ARCHITECTURE\.md`?\s+(?:under\s+)?'([^']{3,80})'", text):
            if m.group(1) not in heads:
                fail("citation", f"{rel}: '{m.group(1)}' is not a heading in docs/ARCHITECTURE.md")


def check_headings():
    files = [".claude/CLAUDE.md", "docs/ARCHITECTURE.md", "docs/AI-WORKFLOW.md"]
    for rel in files:
        text = read(rel)
        if text is None:
            continue
        fenced = False
        lines = text.splitlines()
        for number, line in enumerate(lines, 1):
            if line.startswith("```"):
                fenced = not fenced
            if not fenced and re.match(r"^#{1,6} ", line) and number < len(lines) and lines[number].strip():
                fail("headings", f"{rel}:{number} has no blank line after the heading")


def pattern_regex(rule):
    inner = rule[len("Bash("):-1]
    if inner.endswith(":*"):
        inner = inner[:-2] + " *"
    full = re.compile("^" + ".*".join(re.escape(p) for p in inner.split("*")) + "$")
    bare = None
    if inner.endswith(" *") and inner.count("*") == 1:
        bare = re.compile("^" + re.escape(inner[:-2]) + "$")
    return lambda command: bool(full.match(command) or (bare and bare.match(command)))


def check_settings():
    raw = read(".claude/settings.json")
    if raw is None:
        return fail("settings", ".claude/settings.json is missing")
    try:
        perms = json.loads(raw)["permissions"]
    except (ValueError, KeyError) as error:
        return fail("settings", f"invalid JSON or no permissions block ({error})")
    for key in ("allow", "ask", "deny"):
        for rule in perms.get(key, []):
            if not re.match(r"^(Bash\(.+\)|PowerShell)$", rule):
                fail("settings", f"{key} entry has an unexpected shape: {rule}")
    matchers = [(r, pattern_regex(r)) for r in perms.get("deny", []) if r.startswith("Bash(")]

    def blocked(command):
        parts = [p.strip() for p in re.split(r"&&|\|\||;|\||&|\n|\$\(|\)", command) if p.strip()]
        stripped = [re.sub(r"^(?:[A-Za-z_][A-Za-z0-9_]*=\S*\s+)+", "", p) for p in parts]
        return any(f(c) for c in parts + stripped for _, f in matchers)

    for command in SHOULD_BLOCK:
        if not blocked(command):
            fail("deny-selftest", f"not blocked but should be: {command}")
    for command in SHOULD_PASS:
        if blocked(command):
            fail("deny-selftest", f"blocked but should pass: {command}")
    if "PowerShell" not in perms.get("deny", []):
        fail("settings", "the PowerShell tool is not denied (CLAUDE.md 'Shell & git' says it is)")


def parse_register():
    text = read(REGISTER)
    if text is None:
        return None
    return [line for line in text.splitlines() if re.match(r"^[SA]:", line)]


def check_register(rules):
    if rules is None:
        return
    ids, flags = {}, {}
    for line in rules:
        fields = [f.strip() for f in line.split(" | ")]
        if len(fields) < 4:
            fail("register", f"line has fewer than 4 fields: {line[:40]}")
            continue
        rule_id, area = fields[0], fields[1]
        if rule_id in ids:
            fail("register", f"duplicate ID {rule_id}")
        ids[rule_id] = line
        if area not in AREAS:
            fail("register", f"{rule_id} has unknown area '{area}'")
        if len(fields[2].split()) > 20:
            fail("register", f"{rule_id} paraphrase is longer than 20 words")
        tail = fields[4] if len(fields) > 4 else ""
        flags[rule_id] = re.findall(r"conflict:([^,\s|]+)", tail)
    for rule_id, others in flags.items():
        for other in others:
            if other not in ids:
                fail("register", f"{rule_id} conflicts with unknown ID {other}")
            elif rule_id not in flags.get(other, []):
                fail("register", f"conflict {rule_id} -> {other} is not flagged on both sides")


def check_quoted_counts(rules):
    doc = read("docs/AI-WORKFLOW.md")
    if rules is None or doc is None:
        return
    total, spec = len(rules), sum(r.startswith("S:") for r in rules)
    conflicts = sum(bool(re.search(r"conflict:", r.split(" | ")[-1])) for r in rules)
    m = re.search(r"(\d+) one-line rules \((\d+) from the spec, (\d+) from the assessment\)", doc)
    if m and (int(m.group(1)), int(m.group(2)), int(m.group(3))) != (total, spec, total - spec):
        fail("counts", f"AI-WORKFLOW.md quotes {m.groups()} but the register has {(total, spec, total - spec)}")
    m = re.search(r"`conflict:` flag \((\d+) rules\)", doc)
    if m and int(m.group(1)) != conflicts:
        fail("counts", f"AI-WORKFLOW.md quotes {m.group(1)} conflict rules but the register has {conflicts}")


def check_skills():
    claude = read(".claude/CLAUDE.md") or ""
    triggers = re.search(r"### Skill triggers(.*?)(?=\n### )", claude, re.S)
    listed = set(re.findall(r"^- \*\*([a-z-]+):\*\*", triggers.group(1), re.M)) if triggers else set()
    if listed != set(SKILLS):
        fail("skills", f"CLAUDE.md 'Skill triggers' lists {sorted(listed)}, expected {sorted(SKILLS)}")
    for name in SKILLS:
        text = read(f".claude/skills/{name}/SKILL.md")
        if text is None:
            fail("skills", f"{name}: SKILL.md is missing")
            continue
        front = re.match(r"^---\n(.*?)\n---\n", text, re.S)
        if not front or f"name: {name}" not in front.group(1) or "description:" not in front.group(1):
            fail("skills", f"{name}: front matter lacks name or description")
        sections = re.findall(r"^## (.+)$", text, re.M)
        order = [s for s in SKELETON if s in sections]
        if sections != order or sections[:1] != ["Triggers"] or sections[-1:] != ["Guardrails"]:
            fail("skills", f"{name}: sections {sections} do not follow {SKELETON}")
        if not re.search(r"^# [A-Z]", text, re.M):
            fail("skills", f"{name}: no title heading")
        for m in re.finditer(r"^(#{1,4} .+)$\n(?!\n)", text, re.M):
            fail("skills", f"{name}: no blank line after heading '{m.group(1)}'")
    doc = read("docs/AI-WORKFLOW.md") or ""
    m = re.search(r"project skills: ([a-z, -]+)\.", doc)
    if m and set(s.strip() for s in m.group(1).split(",")) != set(SKILLS):
        fail("skills", "docs/AI-WORKFLOW.md skill list differs from the skills on disk")


def check_paths():
    for rel, path in tracked_files():
        if path.suffix != ".md":
            continue
        for m in re.finditer(r"`((?:\.claude|docs|backend)/[^`\s]+)`", path.read_text(encoding="utf-8")):
            target = m.group(1)
            if any(ch in target for ch in "<*{") or target.endswith(".local.md") or target.endswith("/"):
                continue
            if target not in CREATED_ON_REQUEST and not (ROOT / target).exists():
                fail("paths", f"{rel} mentions {target}, which does not exist")


def check_journal():
    text = read(".claude/temp/notes-ai-journal.md")
    if text is None:
        return fail("journal", "notes-ai-journal.md is missing")
    if "\nENTRIES" not in text:
        return fail("journal", "no ENTRIES block")
    body = text.split("\nENTRIES", 1)[1]
    blocks = [b for b in re.split(r"\n\s*\n", body.strip("\n")) if b.strip()]
    counters = {}
    for block in blocks:
        lines = block.splitlines()
        head = re.match(
            r"^## (PROMPT|CORRECTION|SPLIT|ASSESSMENT|SESSION|SUMMARY)-(\d{3}) \| (p[0-5] [a-z]+) \| (\d{4}-\d{2}-\d{2})$",
            lines[0])
        if not head:
            fail("journal", f"bad entry header: {lines[0][:60]}")
            continue
        kind, number = head.group(1), int(head.group(2))
        if number <= counters.get(kind, 0):
            fail("journal", f"{kind}-{number:03d} is out of sequence (must be above {counters.get(kind, 0):03d})")
        counters[kind] = number
        keys, allowed = JOURNAL_KEYS[kind]
        pairs = [re.match(r"^([a-z-]+): (.+)$", line) for line in lines[1:]]
        if any(p is None for p in pairs) or [p.group(1) for p in pairs] != keys:
            fail("journal", f"{kind}-{number:03d} keys differ from {keys}")
            continue
        limit = JOURNAL_VALUE_LIMITS.get(kind, JOURNAL_DEFAULT_LIMIT)
        for key, value in ((p.group(1), p.group(2)) for p in pairs):
            if len(value) > limit:
                fail("journal", f"{kind}-{number:03d} value of '{key}' is longer than {limit} characters")
            if key in allowed and value not in allowed[key]:
                fail("journal", f"{kind}-{number:03d} '{key}' must be one of {sorted(allowed[key])}")


def check_parking_lot():
    text = read(".claude/temp/notes-parking-lot.md")
    if text is None:
        return fail("parking-lot", "notes-parking-lot.md is missing")
    if not text.endswith("\n"):
        fail("parking-lot", "file does not end with a newline (the next append would join lines)")
    if "\nITEMS" not in text:
        return fail("parking-lot", "no ITEMS block")
    for line in text.split("\nITEMS", 1)[1].splitlines():
        if line.strip() and not re.match(r"^(TOPIC|NEXT|PROPOSAL): \S.*$", line):
            fail("parking-lot", f"untagged or malformed item: {line[:60]}")


def cs_files(base):
    root = ROOT / base
    if not root.exists():
        return
    for path in sorted(root.rglob("*.cs")):
        parts = path.relative_to(ROOT).parts
        if any(part in SKIP_DIRS or part == "Migrations" for part in parts):
            continue
        yield path.relative_to(root).as_posix(), path.read_text(encoding="utf-8", errors="replace")


def namespace_of(text):
    m = re.search(r"^namespace ([\w.]+);", text, re.M)
    return m.group(1) if m else ""


STRING_LITERAL = re.compile(r'@?"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\'')


def has_comment(line):
    code = STRING_LITERAL.sub('""', line)
    return bool(re.search(r"(?<!/)//(?!/)", code) or "/*" in code)


def check_line_rules(base, rel, text, max_line):
    lines = text.splitlines()
    for number, line in enumerate(lines, 1):
        if len(line) > max_line:
            fail("code", f"{base}/{rel}:{number} is longer than {max_line} characters")
        if re.match(r"^\s+return\b", line) and number > 1:
            before = lines[number - 2].strip()
            if before and not before.endswith("{") and not before.startswith("//"):
                fail("code", f"{base}/{rel}:{number} needs a blank line before return")
        if has_comment(line):
            fail("code", f"{base}/{rel}:{number} is a comment (CLAUDE.md 'No explanatory comments')")


def check_using_order(base, rel, text):
    block = []
    for line in text.splitlines():
        if not line.startswith("using "):
            break
        block.append(line)
    key = lambda l: (not l.startswith("using System"), l.rstrip(";").lower().replace(".", "\0"))
    if block != sorted(block, key=key):
        fail("code", f"{base}/{rel}: usings are not in order (System first, then alphabetical)")


def check_names(rel, text):
    for name in BOOL_MEMBER.findall(text):
        if name not in BOOL_EXEMPT and not rel.endswith("Options.cs") and not BOOL_PREFIX.match(name):
            fail("naming", f"backend/src/{rel}: bool member {name} must start with Is, Has, Can or Should")
    for name in INTERFACE.findall(text):
        if not re.match(r"^I[A-Z]", name):
            fail("naming", f"backend/src/{rel}: interface {name} must be named I<Name>")


def expected_namespace(project, rel):
    folder = Path(rel).parent.as_posix()
    return ".".join([project] + ([] if folder == "." else folder.split("/")))


def check_namespaces(base, project, files):
    for rel, text in files.items():
        namespace = namespace_of(text)
        if namespace and namespace != expected_namespace(project, rel):
            fail("layout", f"{base}/{project}/{rel} has namespace {namespace}, expected {expected_namespace(project, rel)}")


def check_test_mirror(project, tests, sources):
    for rel, text in tests.items():
        name = Path(rel).name
        if rel.startswith("Support/") or not name.endswith("Tests.cs") or name in TEST_WITHOUT_SOURCE:
            continue
        folder = Path(rel).parent.as_posix()
        twin = (folder + "/" if folder != "." else "") + name[: -len("Tests.cs")] + ".cs"
        if twin not in sources:
            fail("layout", f"backend/tests/{project}.Tests/{rel} has no source file in {project} to mirror ({twin})")
        for test_name in TEST_METHOD.findall(text):
            if "_" not in test_name:
                fail("naming", f"backend/tests/{project}.Tests/{rel}: test {test_name} must be named <Subject>_<Condition>_<Result>")


def check_domain(files):
    for rel, text in files.items():
        for layer, forbidden in LAYER_FORBIDS.items():
            if rel.startswith(layer + "/"):
                for use in re.findall(r"^using ClaimsModule\.Domain\.([\w.]+);", text, re.M):
                    if any(use == f or use.startswith(f + ".") for f in forbidden):
                        fail("layering", f"{DOMAIN}/{rel} uses {use}, which a {layer} file must not use")
        stem = Path(rel).stem
        parts = rel.split("/")
        is_aggregate = parts[0] in AGGREGATE_FOLDERS
        if is_aggregate and len(parts) > 3 and parts[2] not in ENTITY_SUBFOLDERS:
            fail("layout", f"{DOMAIN}/{rel}: an entity folder holds only {sorted(ENTITY_SUBFOLDERS)}")
        if is_aggregate and "Flows" in parts[:-1] and not stem.endswith(("Operation", "Transition")):
            fail("naming", f"{DOMAIN}/{rel}: a file in Flows/ is named <Verb><Thing>Operation or ...Transition")
        if is_aggregate and "Inputs" in parts[:-1] and not stem.endswith("Input"):
            fail("naming", f"{DOMAIN}/{rel}: a file in Inputs/ is named <Verb><Thing>Input")
        if is_aggregate and "Events" in parts[:-1] and not stem.endswith("Event"):
            fail("naming", f"{DOMAIN}/{rel}: a file in Events/ is named <Thing><PastVerb>Event")
        if is_aggregate and re.search(r"/Rules/[^/]+\.cs$", rel):
            if not re.match(r"^(Is|Has)[A-Z]", stem):
                fail("naming", f"{DOMAIN}/{rel}: a rule class must start with Is or Has")
            if "const string Code" not in text:
                fail("naming", f"{DOMAIN}/{rel}: a rule class must expose a Code constant")


def check_application_names(rel):
    parts = rel.split("/")
    if parts[0] != "ClaimsModule.Application" or len(parts) < 3:
        return
    stem = Path(rel).stem
    folders = parts[1:-1]
    expected = {"Dtos": ("Dto", "MappingProfile"), "Requests": ("Request",), "Commands": ("Command",)}
    for folder, suffixes in expected.items():
        if folder in folders and not stem.endswith(suffixes):
            fail("naming", f"backend/src/{rel}: a file in {folder}/ is named ...{' or ...'.join(suffixes)} (S:DATA-Naming)")
    if "Queries" in folders and not re.match(r"^(Get|List)[A-Z]\w*Query$", stem):
        fail("naming", f"backend/src/{rel}: a file in Queries/ is named GetNounQuery or ListNounsQuery (S:DATA-Naming)")


GUID_LITERAL = re.compile(r"[\"'][0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}[\"']")


def check_hard_guids(rel, text):
    for number, line in enumerate(text.splitlines(), 1):
        if GUID_LITERAL.search(line):
            fail("code", f"backend/src/{rel}:{number} has a hard-coded GUID (S:DATA-NoHardGuids)")


def check_code_layout():
    max_line = line_limit()
    for base in ("backend/src", "backend/tests"):
        for rel, text in cs_files(base):
            check_line_rules(base, rel, text, max_line)
            check_using_order(base, rel, text)
    for rel, text in cs_files("backend/src"):
        check_names(rel, text)
        check_hard_guids(rel, text)
        check_application_names(rel)
    src_root = ROOT / "backend/src"
    for project_dir in sorted(src_root.glob("ClaimsModule.*")) if src_root.exists() else []:
        project = project_dir.name
        sources = dict(cs_files(f"backend/src/{project}"))
        check_namespaces("backend/src", project, sources)
        tests = dict(cs_files(f"backend/tests/{project}.Tests"))
        check_namespaces("backend/tests", f"{project}.Tests", tests)
        check_test_mirror(project, tests, sources)
        if project == "ClaimsModule.Domain":
            check_domain(sources)


def check_overlap(window=8):
    words = []
    for rel in LOCAL_DOCS:
        text = read(rel)
        if text:
            words += re.findall(r"[a-z0-9']+", text.lower())
    if not words:
        return
    grams = {tuple(words[i:i + window]) for i in range(len(words) - window + 1)}
    for rel, path in tracked_files():
        if path.suffix not in TEXT_SUFFIXES or MESSAGE_ALLOWED.match(rel):
            continue
        tokens = re.findall(r"[a-z0-9']+", path.read_text(encoding="utf-8", errors="replace").lower())
        hits = sum(tuple(tokens[i:i + window]) in grams for i in range(len(tokens) - window + 1))
        if hits:
            fail("confidentiality", f"{rel} shares {hits} run(s) of {window}+ words with the local task documents")


def main():
    global ran
    rules = parse_register()
    for run in (check_ascii, check_citations, check_architecture_citations, check_headings, check_settings,
                lambda: check_register(rules), lambda: check_quoted_counts(rules), check_skills, check_paths,
                check_journal, check_parking_lot, check_code_layout, check_overlap):
        ran += 1
        run()
    for line in findings:
        print(line)
    skipped = "" if rules is not None else " (register not present: register checks skipped)"
    print(f"check_workflow: {ran} checks, {len(findings)} findings{skipped}")
    return 1 if findings else 0


if __name__ == "__main__":
    sys.exit(main())

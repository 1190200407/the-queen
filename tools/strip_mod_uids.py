import re
from pathlib import Path

mod_root = Path(__file__).resolve().parents[1] / "TheQueen"
patterns = ("*.tres", "*.tscn")
header_uid_re = re.compile(
    r'^(?P<prefix>\[(?:gd_scene|gd_resource)[^\]]*?) uid="uid://[^"]+"(?P<suffix>.*\])$',
    re.M,
)
ext_uid_re = re.compile(
    r'(\[ext_resource\b[^\]]*?) uid="uid://[^"]+"(\s+path="res://(?:TheQueen|Scripts)/[^"]+")'
)

changed_files: list[str] = []
for pattern in patterns:
    for path in mod_root.rglob(pattern):
        text = path.read_text(encoding="utf-8")
        new_text = header_uid_re.sub(r"\g<prefix>\g<suffix>", text)
        new_text = ext_uid_re.sub(r"\1\2", new_text)
        if new_text != text:
            path.write_text(new_text, encoding="utf-8", newline="\n")
            changed_files.append(str(path.relative_to(mod_root.parent)))

print(f"Updated {len(changed_files)} files:")
for file in sorted(changed_files):
    print(f"  {file}")

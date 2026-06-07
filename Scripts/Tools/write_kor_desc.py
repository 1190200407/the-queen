# -*- coding: utf-8 -*-
import json
from pathlib import Path

TRANSLATIONS = {}

def main():
    eng = json.load(open(Path(__file__).resolve().parents[2] / "TheQueen/localization/eng/cards.json", encoding="utf-8-sig"))
    other = [k for k in eng if not k.endswith(".title")]
    missing = [k for k in other if k not in TRANSLATIONS]
    if missing:
        raise SystemExit(f"Missing {len(missing)}: {missing[:5]}")
    out = Path(__file__).with_name("kor_cards_descriptions.json")
    out.write_text(json.dumps(TRANSLATIONS, ensure_ascii=False, indent=4) + "\n", encoding="utf-8")
    print(len(TRANSLATIONS), "keys ->", out)

if __name__ == "__main__":
    main()

"""One-shot generator: writes kor/cards.json from eng + embedded translations."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ENG = ROOT / "TheQueen/localization/eng/cards.json"
DESC = Path(__file__).with_name("kor_cards_descriptions.json")
KOR_OUT = ROOT / "TheQueen/localization/kor/cards.json"
TITLES_PY = Path(__file__).with_name("build_kor_cards.py")

def load_titles():
    ns = {}
    exec(TITLES_PY.read_text(encoding="utf-8"), ns)
    return ns["TITLES"]

def main():
    eng = json.loads(ENG.read_text(encoding="utf-8-sig"))
    desc = json.loads(DESC.read_text(encoding="utf-8"))
    titles = load_titles()

    kor = {}
    for key in eng:
        if key.endswith(".title"):
            kor[key] = titles[key]
        else:
            if key not in desc:
                raise KeyError(f"Missing description: {key}")
            kor[key] = desc[key]

    assert len(kor) == 444
    assert set(kor) == set(eng)
    KOR_OUT.parent.mkdir(parents=True, exist_ok=True)
    KOR_OUT.write_text(json.dumps(kor, ensure_ascii=False, indent=4) + "\n", encoding="utf-8")
    print(f"OK: {KOR_OUT} ({len(kor)} keys)")

if __name__ == "__main__":
    main()

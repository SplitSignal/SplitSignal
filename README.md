# Split Signal — Collaborative Escape Room

CSCE A401 Software Engineering — Ben Browning, Thatcher King, Reginald Sheakley

A two-player cooperative escape room. One player explores a 3D Unity room;
their partner uses a web companion app. A local server connects them, holds
session state, and relays updates in both directions. Neither player can
finish alone.

## Repository layout

```
unity/    Unity client — 3D room, player movement, interaction    (Ben)
server/   Game server, session handling, database, API            (Thatcher)
web/      Browser companion app                                   (Reggie)
docs/     Architecture notes and the API contract
```

## Requirements

| Part    | Needs                                                    |
| ------- | -------------------------------------------------------- |
| unity/  | Unity **6000.2.12f1** — this exact version               |
| server/ | TBD                                                      |
| web/    | TBD                                                      |
| all     | Git and **Git LFS** (`git lfs install` once per machine) |

## First-time setup

```bash
git clone https://github.com/lizard856/SplitSignal.git
cd SplitSignal
git lfs pull
```

Open the Unity project by pointing Unity Hub at the **`unity/`** folder,
not the repository root.

### Unity SmartMerge (Unity devs only, once per machine)

Lets Git merge scene and prefab files instead of mangling them:

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver '/Applications/Unity/Hub/Editor/6000.2.12f1/Unity.app/Contents/Tools/UnityYAMLMerge merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
git config merge.unityyamlmerge.recursive binary
```

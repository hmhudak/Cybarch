# Cybarch (teaching MVP)

## Prihlásenie / seed
V `Development` sa databáza vytvorí automaticky (`EnsureCreated`) a zasadia sa testovací používatelia:

- `creator` / `Passw0rd`
- `jano` / `Passw0rd`
- `mato` / `Passw0rd`

Zároveň vznikne demo Universe a demo Game.

> Poznámka: pre reálny projekt prejdite na EF Core migrations (`dotnet ef migrations add ...`).

---

## Pravidlá rolí v hre (Game)

Aplikácia rozlišuje členstvo na dvoch úrovniach:

1. **UniverseMember** – user je člen Universe (vidí Universe, jeho hry a členov).
2. **GameMember** – user je účastník konkrétnej hry a má rolu:
   - **DM** (Dungeon Master)
   - **Player**

Dôležité: byť UniverseMember **neznamená** automaticky byť Player/DM v hrách.

### Zobrazenie Game detailu

- Ak user **nie je** GameMember: zobrazí sa hláška **„Nie si hráč tejto hry.“**
- Universe creator má navyše tlačidlo **God mode**, ktorým sa vie pripojiť ako DM.

### Kto môže meniť roly

- **Make player**: môže iba DM (pridá Universe membera do Game ako Player)
- **Make DM**: môže iba DM (povýši Playera na DM)
- **Degrade to player**:
  - bežný DM môže degradovať **iba sám seba**
  - Universe creator so zapnutým **God mode** môže degradovať aj iných DM
  - nikdy nie je dovolené degradovať **posledného DM**

### Odstránenie z hry / odchod z hry

- **Remove**: DM môže odstrániť Playera z hry
- **Leave game**: Player môže odísť sám
- Pri odstránení/odchode sa automaticky zrušia všetky **CharacterAssignments** daného usera v rámci tejto hry.

---

## Assign players (CharacterAssignments)

- V detaile hry sa pri každej postave zobrazuje `Assigned: ...`.
- DM má pri tomto texte tlačidlo **+**, ktoré otvorí modal so zoznamom Players v hre.
- Checkboxy v modale určujú, kto je priradený k postave.

UI poznámka:
- Ak je zoznam priradených hráčov dlhý, blok je horizontálne **scrollable**.

---

## Rulesets (kontajnerový prístup)

Špecifiká pravidiel (rasy, classy, subclasy, default HP/Mana) sú izolované v `Services/Rulesets`.

- `IRulesetDefinition` definuje API pravidiel
- `Drd16Ruleset` je prvý konkrétny ruleset
- `RulesetRegistry` je jednoduchý registrátor

Pridanie nového rulesetu (napr. DnD 5E) znamená:

1. vytvoriť novú triedu implementujúcu `IRulesetDefinition`
2. zaregistrovať ju v `Program.cs` do DI
3. Universe môže používať nový `RulesetKey`


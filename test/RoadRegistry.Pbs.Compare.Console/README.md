# PBS compare

Vergelijkt het PBS-read model (SQL Server) met de Marten-store (PostgreSQL) om één vraag te beantwoorden:

> heeft de PBS-projectie alle events verwerkt?

Niet door waarde-per-waarde te auditen, maar door per entiteit drie dingen te controleren:

1. **bestaat wat moet bestaan** — elke niet-verwijderde entiteit in Marten heeft een rij in PBS;
2. **is de state de laatste** — de velden die bij vrijwel elke wijziging veranderen, liggen gelijk;
3. **is weg wat weg moet** — wat in Marten verwijderd is, staat niet meer in PBS.

## Waarom de topology-projectie als referentie

De Marten-kant wordt gelezen uit `projections.networktopology_*`, niet uit de snapshot-documenten. Die projectie is
**inline**: ze wordt geschreven in dezelfde transactie die de events wegschrijft en kan dus per definitie niet
achterlopen op de event store. Precies wat je als referentiepunt wil. Ze bewaart bovendien alles wat hier vergeleken
wordt in gewone kolommen (`id`, `is_removed`, `status`, de knoop-ids, de geometrie), dus er hangt niets af van een
json-propertynaam die morgen anders heet.

## Wat er vergeleken wordt

| entiteit | PBS-tabel | naast bestaan/verwijderd ook |
|---|---|---|
| wegsegment | `Wegsegmenten` | begin- en eindknoop, status, aantal punten, lengte |
| wegknoop | `WegknopenData` | x/y van het punt |
| ongelijkgrondse kruising | `OngelijkgrondseKruisingen` | boven- en onderliggend wegsegment |
| gelijkgrondse kruising | `GelijkgrondseKruisingen` | de twee wegsegmenten |

Daarvóór wordt de projectiestand getoond: de hoogste eventsequentie in Marten tegenover de positie(s) in
`[RoadRegistryPbs].[ProjectionStates]`. **Lees dat eerst.** Loopt PBS achter, dan is een deel van de bevindingen
gewoon achterstand en geen fout; de samenvatting zegt dat er ook bij.

## Wat er niet vergeleken wordt

Bewust buiten scope, omdat dit een dekkingscheck is en geen audit:

- de waarden in de attribuuttabellen (morfologie, wegcategorie, toegang, verharding, straatnaam, wegbeheerder,
  verkeersrichtingen) en hun VANPOS/TOTPOS;
- de Europese en nationale wegnummers;
- de inhoud van `AfgeleideWegsegmenten` (alleen onrechtstreeks: een segment dat ontbreekt, ontbreekt hier ook);
- `CREATIE` / `VERSIE`, want die komen uit event-provenance;
- labels, want die zijn een functie van de code.

Wil je wél een inhoudelijke vergelijking, dan is `Wegenregister/SQL/pbs-vs-editor-verificatie.sql` daarvoor het
instrument (PBS tegenover de EditorContext, mét V1→V2-mapping).

## Draaien

```bash
dotnet run --project test/RoadRegistry.Pbs.Compare.Console -- \
  --pbs "Data Source=...;Initial Catalog=road-registry;..." \
  --marten "Host=...;Database=...;Username=...;Password=..."
```

Connectiestrings kunnen ook uit `PBS_CONNECTION` en `MARTEN_CONNECTION` komen, zodat ze niet in je shell-history
belanden. Je hebt netwerktoegang nodig tot beide databanken van de omgeving die je controleert — op tst doorgaans
enkel via bastion/VPN.

Opties:

| optie | standaard | betekenis |
|---|---|---|
| `--out <pad>` | `pbs-compare-findings.csv` | waar de details landen |
| `--tolerance <meter>` | `0.05` | vlakke tolerantie op de geometrielengte; er komt 0,5 cm per vertex bij, want PBS rondt elke coördinaat af op cm |
| `--max-findings <n>` | `1000` | maximum aantal detailrijen per check; de aantallen in de samenvatting blijven volledig |

De tool leest alleen. Aan de SQL Server-kant staat de sessie op `read uncommitted`, zodat ze de projectiehosts die ze
controleert niet in de weg loopt. Ctrl+C breekt netjes af en print alsnog de samenvatting, met een notitie dat de
cijfers onvolledig zijn.

Exitcodes: `0` niets gevonden, `2` bevindingen, `1` verkeerde argumenten.

## Twee dingen om in gedachten te houden

- **Niet onafhankelijk.** PBS wordt uit diezelfde Marten-stream geprojecteerd. Een fout die het aggregaat én de
  projectie delen — een verkeerd event, een verkeerde migratie-interpretatie — valt hier weg. Daarvoor is de
  vergelijking met de EditorContext nodig, die een andere event store en een ander codepad heeft.
- **Momentopname van twee databanken die blijven bewegen.** De twee kanten worden na elkaar gelezen, niet in één
  consistent punt in de tijd. Op een omgeving waar op dat moment geprojecteerd wordt, zijn een paar bevindingen aan de
  rand normaal; draai het nog eens en kijk of dezelfde ids terugkomen.

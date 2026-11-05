# PBS compare

Vergelijkt het PBS-read model (SQL Server) met de Marten-store (PostgreSQL) om één vraag te beantwoorden:

> heeft de PBS-projectie alle events verwerkt?

Niet door waarde-per-waarde te auditen, maar door per entiteit drie dingen te controleren:

1. **bestaat wat moet bestaan** — elke niet-verwijderde entiteit in Marten heeft een rij in PBS;
2. **is de state de laatste** — de velden die bij vrijwel elke wijziging veranderen, liggen gelijk;
3. **is weg wat weg moet** — wat in Marten verwijderd is, staat niet meer in PBS.

## Waar de referentie uit komt

Twee verschillende bronnen, elk voor waar ze gezag over heeft.

**Moet de entiteit bestaan — uit de events.** Per stream geldt het laatste event dat de entiteit aanmaakt of
verwijdert; alles daartussen is een wijziging en verschuift het bestaan niet. `Comparers.Existences` houdt die twee
lijsten per entiteitssoort bij, in Martens snake_case-aliassen. Met `--eventtypes` print je wat de store écht aan
types bevat; dat is de manier om die lijsten na een nieuw event te controleren.

Een event dat in geen van beide lijsten staat, verschuift niets - en een stream zonder enig bestaansevent geldt als
bestaand. Dat is waarom `road_segment_was_merged` en `road_segment_was_split` in geen van beide lijsten staan: ze
worden aan de stream van het weggesplitste of samengevoegde segment gehangen, direct achter zijn
`..._was_retired_because_of_...`, en zouden die verwijdering dus ongedaan maken. Op de stream van een segment dat
eruit voortkomt zijn ze het eerste event, en daar is "bestaat" ook het juiste antwoord.

Niet uit `isRemoved` van het aggregate-document dus. Normaal zeggen die twee hetzelfde — de snapshot wordt inline
geschreven, na elk event van de stream — maar het is de stream die niet overgeslagen kán zijn, en elk verschil
tussen de twee is op zich al het melden waard. De samenvatting rapporteert dat verschil apart, als notitie: het zijn
geen bevindingen over PBS, maar ze zeggen hoe ver het document zelf is afgeweken van zijn eigen events.

**Hoe de entiteit er uitziet — uit de aggregate-documenten.** De state wordt gelezen uit
`eventstore.mt_doc_roadsegment`, `_roadnode`, `_gradeseparatedjunction` en `_gradejunction`. De snapshot wordt
**inline** geschreven, in dezelfde transactie die de events wegschrijft, en kan dus per definitie niet achterlopen op
de event store. Alles wat hier vergeleken wordt (`road_segment_id`, `status`, de knoop-ids, de geometrie) staat in
kolommen die Postgres uit de document-json genereert — toegevoegd door
`20261103120000_add_topology_columns_to_aggregate_documents` en bij elke upsert van Marten meteen mee bijgewerkt.

De `projections.networktopology_*`-tabellen waar deze tool dit vroeger uit las, worden sinds `1a813da62` niet meer
geprojecteerd. Draait de tool tegen een omgeving waar die migratie nog niet gepasseerd is, dan faalt hij met
`column "is_removed" does not exist` — dat is geen bug in de tool maar een databank die nog bijgewerkt moet worden.

De maand waarin een bevinding gebucket wordt, komt óók uit de event-stream: een document draagt geen eigen
event-timestamp, en `mt_last_modified` zegt wanneer de snapshot geschreven is, niet wanneer de wijziging gebeurde.

**Status wordt enkel vergeleken voor een gemigreerd wegsegment.** Het document van een segment dat nog uit het
legacy-wegenregister komt draagt geen echte V2-status: `MartenMigrationProjection` zet daar een placeholder uit de
tekenmethode (ingeschetst → `Gepland`, al de rest → `Gerealiseerd`) en laat de werkelijke status in het legacy-event
dat ze wegschrijft. PBS projecteert die werkelijke status. Die twee naast elkaar leggen zegt dus niets over PBS — het
markeerde 97% van alle ingeschetste segmenten voor dit overgeslagen werd.

## Wat er vergeleken wordt

| entiteit | PBS-tabel | naast bestaan/verwijderd ook |
|---|---|---|
| wegsegment | `Wegsegmenten` | begin- en eindknoop, status (enkel gemigreerd, zie hieronder), aantal punten, lengte |
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

Maak eenmalig `appsettings.local.json` naast `appsettings.json`. Dat bestand is gitignored (`appsettings.*.json`),
dus daar mogen echte credentials in. De namen zijn dezelfde als in de hosts, dus je kan de waarden daar letterlijk
overnemen:

```json
{
  "ConnectionStrings": {
    "PbsProjections": "Data Source=...;Initial Catalog=road-registry;...",
    "Marten": "Host=...;Database=...;Username=...;Password=..."
  }
}
```

Daarna draait F5 uit de debugger zonder argumenten. Van de commandolijn:

```bash
dotnet run --project test/RoadRegistry.Pbs.Compare.Console
```

De configuratie wordt in deze volgorde gelezen, waarbij de laatste wint:

1. `appsettings.json` — staat in git, laat de connectiestrings daar leeg;
2. `appsettings.{environment}.json` — optioneel (`DOTNET_ENVIRONMENT`, standaard `development`);
3. `appsettings.local.json` — gitignored, jouw credentials;
4. omgevingsvariabelen — `ConnectionStrings__PbsProjections` / `ConnectionStrings__Marten`, of de kortere
   `PBS_CONNECTION` / `MARTEN_CONNECTION`;
5. commandolijnargumenten — `--pbs` en `--marten`.

De json-bestanden worden bij het bouwen naar de outputmap gekopieerd en daar ook gelezen, zodat de debugger en
`dotnet run` — die in verschillende mappen starten — hetzelfde zien.

Je hebt netwerktoegang nodig tot beide databanken van de omgeving die je controleert — op tst doorgaans enkel via
bastion/VPN.

Opties:

| optie | standaard | betekenis |
|---|---|---|
| `--out <pad>` | `pbs-compare-findings.csv` | waar de details landen |
| `--tolerance <meter>` | `0.05` | vlakke tolerantie op de geometrielengte; er komt 0,5 cm per vertex bij, want PBS rondt elke coördinaat af op cm |
| `--max-findings <n>` | `1000` | maximum aantal detailrijen per check; de aantallen in de samenvatting blijven volledig |

Daarnaast zes read-only diagnosestanden, die elk één vraag beantwoorden en daarna stoppen:

| optie | wat het print |
|---|---|
| `--eventtypes` | elk eventtype in de store met zijn aantal — waarmee je de lijsten in `Existences` controleert |
| `--diagnose` | events zonder correlation id per type (die worden door `RoadNetworkChangesProjection` genegeerd) |
| `--stream <key,key>` | de eventgeschiedenis van een stream, bv. `RoadSegment-74369` |
| `--topology <id,id>` | de gegenereerde topologie-kolommen van het document achter een wegsegment-id |
| `--doc <id,id>` | `isRemoved` en status uit `mt_doc_roadsegment` - het aggregaat zelf, dus het oordeel van het domein |
| `--editor <id,id>` | of de EditorContext het segment nog heeft, met zijn methode en status |

Eén waarschuwing bij `--editor`: op tst bevat `[RoadRegistryEditor].[RoadSegment]` ongeveer 1.095.000 rijen tegen
ongeveer 853.000 in PBS, wat neerkomt op elk segment dat er ooit was. Die tabel onderscheidt daar dus niet tussen
bestaand en verwijderd, en is op die omgeving geen bruikbare getuige over bestaan. Controleer het totaal dat de optie
print vóór je er een conclusie op bouwt.

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

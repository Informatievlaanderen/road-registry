module DownloadForEditor exposing (Msg(..), init, main, subscriptions, update, view)

import Alert
import Browser
import Bytes exposing (Bytes)
import Json.Encode as Encode
import Json.Decode exposing (string)
import File.Download
import Filesize
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, a, div, h1, h2, h3, li, main_, section, span, text, ul, button, textarea, form)
import Html.Attributes exposing (class, classList, id, style, selected)
import Html.Events exposing (onClick)
import Http
import HttpBytes
import Html.Attributes exposing (placeholder)
import Html.Attributes exposing (rows)
import Html.Attributes exposing (cols)
import Html exposing (input)
import Html.Attributes exposing (type_)
import Html.Attributes exposing (value)
import Html exposing (label)
import FontAwesome exposing (check)
import Html.Attributes exposing (checked)
import Html.Events exposing (onInput)
import Html exposing (select)
import Html exposing (option)
import Browser.Navigation exposing (load)


main : Program Flags Model Msg
main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }

type alias Flags =
    { endpoint : String
    , oldEndpoint : String
    , apikey : String
    }

type alias DownloadModel =
    { title : String
    , url : String
    , downloading : Bool
    , progressing : Bool
    , progress : String
    , count: Int
    , apikey: String
    }

type alias WizardModel = 
    { step : WizardStep
    , previousStep : WizardStep
    , customWkt : String
    , enableBuffer : Bool
    , nisCode : String
    , description : String
    , municipalities : List Municipality
    }

type alias Municipality = 
    { nisCode : String
    , municipalityName : String}

type WizardStep = Step1 | Step2_Municipality | Step2_Contour | Step3_Municipality | Step3_Contour


type alias Model =
    { header : HeaderModel
    , download : DownloadModel
    , alert : Alert.Model
    , wizard : WizardModel
    , flags : Flags
    }

init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { header = Header.init |> Header.downloadForEditorBecameActive
      , flags = flags
      , download =
            { title = "Register dump"
            , url =
                if String.endsWith "/" flags.oldEndpoint then
                    String.concat [ flags.oldEndpoint, "v1/download/for-editor" ]

                else
                    String.concat [ flags.oldEndpoint, "/v1/download/for-editor" ]
            , downloading = False
            , progressing = False
            , progress = ""
            , count = 0
            , apikey = flags.apikey
            }
      , alert = Alert.init ()
      , wizard = 
        { step = Step1
        , previousStep = Step1
        , customWkt = ""
        , nisCode = ""
        , description = ""
        , enableBuffer = True
        , municipalities = [
                { nisCode = "41002", municipalityName = "Aalst"}
                , { nisCode = "44084", municipalityName = "Aalter"}
                , { nisCode = "81001", municipalityName = "Aarlen"}
                , { nisCode = "24001", municipalityName = "Aarschot"}
                , { nisCode = "11001", municipalityName = "Aartselaar"}
                , { nisCode = "51004", municipalityName = "Aat"}
                , { nisCode = "23105", municipalityName = "Affligem"}
                , { nisCode = "73001", municipalityName = "Alken"}
                , { nisCode = "38002", municipalityName = "Alveringem"}
                , { nisCode = "63001", municipalityName = "Amel"}
                , { nisCode = "21001", municipalityName = "Anderlecht"}
                , { nisCode = "11002", municipalityName = "Antwerpen"}
                , { nisCode = "34002", municipalityName = "Anzegem"}
                , { nisCode = "37020", municipalityName = "Ardooie"}
                , { nisCode = "13001", municipalityName = "Arendonk"}
                , { nisCode = "71002", municipalityName = "As"}
                , { nisCode = "23002", municipalityName = "Asse"}
                , { nisCode = "43002", municipalityName = "Assenede"}
                , { nisCode = "34003", municipalityName = "Avelgem"}
                , { nisCode = "13002", municipalityName = "Baarle-Hertog"}
                , { nisCode = "13003", municipalityName = "Balen"}
                , { nisCode = "82003", municipalityName = "Bastenaken"}
                , { nisCode = "31003", municipalityName = "Beernem"}
                , { nisCode = "13004", municipalityName = "Beerse"}
                , { nisCode = "23003", municipalityName = "Beersel"}
                , { nisCode = "24007", municipalityName = "Begijnendijk"}
                , { nisCode = "24008", municipalityName = "Bekkevoort"}
                , { nisCode = "53053", municipalityName = "Bergen"}
                , { nisCode = "71004", municipalityName = "Beringen"}
                , { nisCode = "12002", municipalityName = "Berlaar"}
                , { nisCode = "42003", municipalityName = "Berlare"}
                , { nisCode = "24009", municipalityName = "Bertem"}
                , { nisCode = "25005", municipalityName = "Bevekom"}
                , { nisCode = "23009", municipalityName = "Bever"}
                , { nisCode = "46003", municipalityName = "Beveren"}
                , { nisCode = "24011", municipalityName = "Bierbeek"}
                , { nisCode = "73006", municipalityName = "Bilzen"}
                , { nisCode = "62011", municipalityName = "Bitsingen"}
                , { nisCode = "31004", municipalityName = "Blankenberge"}
                , { nisCode = "72003", municipalityName = "Bocholt"}
                , { nisCode = "11004", municipalityName = "Boechout"}
                , { nisCode = "12005", municipalityName = "Bonheiden"}
                , { nisCode = "11005", municipalityName = "Boom"}
                , { nisCode = "24014", municipalityName = "Boortmeerbeek"}
                , { nisCode = "73009", municipalityName = "Borgloon"}
                , { nisCode = "64074", municipalityName = "Borgworm"}
                , { nisCode = "12007", municipalityName = "Bornem"}
                , { nisCode = "11007", municipalityName = "Borsbeek"}
                , { nisCode = "24016", municipalityName = "Boutersem"}
                , { nisCode = "45059", municipalityName = "Brakel"}
                , { nisCode = "11008", municipalityName = "Brasschaat"}
                , { nisCode = "11009", municipalityName = "Brecht"}
                , { nisCode = "35002", municipalityName = "Bredene"}
                , { nisCode = "72004", municipalityName = "Bree"}
                , { nisCode = "31005", municipalityName = "Brugge"}
                , { nisCode = "21004", municipalityName = "Brussel"}
                , { nisCode = "42004", municipalityName = "Buggenhout"}
                , { nisCode = "63012", municipalityName = "Büllingen"}
                , { nisCode = "63013", municipalityName = "Bütgenbach"}
                , { nisCode = "31006", municipalityName = "Damme"}
                , { nisCode = "35029", municipalityName = "De Haan"}
                , { nisCode = "38008", municipalityName = "De Panne"}
                , { nisCode = "44012", municipalityName = "De Pinte"}
                , { nisCode = "34009", municipalityName = "Deerlijk"}
                , { nisCode = "44083", municipalityName = "Deinze"}
                , { nisCode = "41011", municipalityName = "Denderleeuw"}
                , { nisCode = "42006", municipalityName = "Dendermonde"}
                , { nisCode = "37002", municipalityName = "Dentergem"}
                , { nisCode = "13006", municipalityName = "Dessel"}
                , { nisCode = "44013", municipalityName = "Destelbergen"}
                , { nisCode = "71011", municipalityName = "Diepenbeek"}
                , { nisCode = "24020", municipalityName = "Diest"}
                , { nisCode = "32003", municipalityName = "Diksmuide"}
                , { nisCode = "23016", municipalityName = "Dilbeek"}
                , { nisCode = "72041", municipalityName = "Dilsen-Stokkem"}
                , { nisCode = "57081", municipalityName = "Doornik"}
                , { nisCode = "23098", municipalityName = "Drogenbos"}
                , { nisCode = "12009", municipalityName = "Duffel"}
                , { nisCode = "11013", municipalityName = "Edegem"}
                , { nisCode = "55010", municipalityName = "Edingen"}
                , { nisCode = "43005", municipalityName = "Eeklo"}
                , { nisCode = "25014", municipalityName = "Eigenbrakel"}
                , { nisCode = "21009", municipalityName = "Elsene"}
                , { nisCode = "51017", municipalityName = "Elzele"}
                , { nisCode = "41082", municipalityName = "Erpe-Mere"}
                , { nisCode = "11016", municipalityName = "Essen"}
                , { nisCode = "21005", municipalityName = "Etterbeek"}
                , { nisCode = "21006", municipalityName = "Evere"}
                , { nisCode = "44019", municipalityName = "Evergem"}
                , { nisCode = "23023", municipalityName = "Galmaarden"}
                , { nisCode = "21008", municipalityName = "Ganshoren"}
                , { nisCode = "44020", municipalityName = "Gavere"}
                , { nisCode = "13008", municipalityName = "Geel"}
                , { nisCode = "24028", municipalityName = "Geetbets"}
                , { nisCode = "25048", municipalityName = "Geldenaken"}
                , { nisCode = "25031", municipalityName = "Genepiën"}
                , { nisCode = "71016", municipalityName = "Genk"}
                , { nisCode = "44021", municipalityName = "Gent"}
                , { nisCode = "41018", municipalityName = "Geraardsbergen"}
                , { nisCode = "71017", municipalityName = "Gingelom"}
                , { nisCode = "35005", municipalityName = "Gistel"}
                , { nisCode = "24137", municipalityName = "Glabbeek"}
                , { nisCode = "23024", municipalityName = "Gooik"}
                , { nisCode = "25037", municipalityName = "Graven"}
                , { nisCode = "23025", municipalityName = "Grimbergen"}
                , { nisCode = "13010", municipalityName = "Grobbendonk"}
                , { nisCode = "24033", municipalityName = "Haacht"}
                , { nisCode = "41024", municipalityName = "Haaltert"}
                , { nisCode = "71020", municipalityName = "Halen"}
                , { nisCode = "23027", municipalityName = "Halle"}
                , { nisCode = "71069", municipalityName = "Ham"}
                , { nisCode = "42008", municipalityName = "Hamme"}
                , { nisCode = "72037", municipalityName = "Hamont-Achel"}
                , { nisCode = "64034", municipalityName = "Hannuit"}
                , { nisCode = "34013", municipalityName = "Harelbeke"}
                , { nisCode = "71022", municipalityName = "Hasselt"}
                , { nisCode = "72038", municipalityName = "Hechtel-Eksel"}
                , { nisCode = "73022", municipalityName = "Heers"}
                , { nisCode = "12014", municipalityName = "Heist-op-den-Berg"}
                , { nisCode = "11018", municipalityName = "Hemiksem"}
                , { nisCode = "24038", municipalityName = "Herent"}
                , { nisCode = "13011", municipalityName = "Herentals"}
                , { nisCode = "13012", municipalityName = "Herenthout"}
                , { nisCode = "71024", municipalityName = "Herk-de-Stad"}
                , { nisCode = "23032", municipalityName = "Herne"}
                , { nisCode = "13013", municipalityName = "Herselt"}
                , { nisCode = "73028", municipalityName = "Herstappe"}
                , { nisCode = "41027", municipalityName = "Herzele"}
                , { nisCode = "71070", municipalityName = "Heusden-Zolder"}
                , { nisCode = "33039", municipalityName = "Heuvelland"}
                , { nisCode = "24041", municipalityName = "Hoegaarden"}
                , { nisCode = "61031", municipalityName = "Hoei"}
                , { nisCode = "23033", municipalityName = "Hoeilaart"}
                , { nisCode = "73032", municipalityName = "Hoeselt"}
                , { nisCode = "24043", municipalityName = "Holsbeek"}
                , { nisCode = "36006", municipalityName = "Hooglede"}
                , { nisCode = "13014", municipalityName = "Hoogstraten"}
                , { nisCode = "45062", municipalityName = "Horebeke"}
                , { nisCode = "72039", municipalityName = "Houthalen-Helchteren"}
                , { nisCode = "32006", municipalityName = "Houthulst"}
                , { nisCode = "11021", municipalityName = "Hove"}
                , { nisCode = "24045", municipalityName = "Huldenberg"}
                , { nisCode = "13016", municipalityName = "Hulshout"}
                , { nisCode = "35006", municipalityName = "Ichtegem"}
                , { nisCode = "33011", municipalityName = "Ieper"}
                , { nisCode = "36007", municipalityName = "Ingelmunster"}
                , { nisCode = "25044", municipalityName = "Itter"}
                , { nisCode = "36008", municipalityName = "Izegem"}
                , { nisCode = "31012", municipalityName = "Jabbeke"}
                , { nisCode = "21010", municipalityName = "Jette"}
                , { nisCode = "53044", municipalityName = "Jurbeke"}
                , { nisCode = "11022", municipalityName = "Kalmthout"}
                , { nisCode = "23038", municipalityName = "Kampenhout"}
                , { nisCode = "11023", municipalityName = "Kapellen"}
                , { nisCode = "23039", municipalityName = "Kapelle-op-den-Bos"}
                , { nisCode = "43007", municipalityName = "Kaprijke"}
                , { nisCode = "25015", municipalityName = "Kasteelbrakel"}
                , { nisCode = "13017", municipalityName = "Kasterlee"}
                , { nisCode = "24048", municipalityName = "Keerbergen"}
                , { nisCode = "63040", municipalityName = "Kelmis"}
                , { nisCode = "72018", municipalityName = "Kinrooi"}
                , { nisCode = "45060", municipalityName = "Kluisbergen"}
                , { nisCode = "31043", municipalityName = "Knokke-Heist"}
                , { nisCode = "32010", municipalityName = "Koekelare"}
                , { nisCode = "21011", municipalityName = "Koekelberg"}
                , { nisCode = "38014", municipalityName = "Koksijde"}
                , { nisCode = "54010", municipalityName = "Komen-Waasten"}
                , { nisCode = "11024", municipalityName = "Kontich"}
                , { nisCode = "32011", municipalityName = "Kortemark"}
                , { nisCode = "24054", municipalityName = "Kortenaken"}
                , { nisCode = "24055", municipalityName = "Kortenberg"}
                , { nisCode = "73040", municipalityName = "Kortessem"}
                , { nisCode = "34022", municipalityName = "Kortrijk"}
                , { nisCode = "23099", municipalityName = "Kraainem"}
                , { nisCode = "46013", municipalityName = "Kruibeke"}
                , { nisCode = "45068", municipalityName = "Kruisem"}
                , { nisCode = "34023", municipalityName = "Kuurne"}
                , { nisCode = "13053", municipalityName = "Laakdal"}
                , { nisCode = "42010", municipalityName = "Laarne"}
                , { nisCode = "73042", municipalityName = "Lanaken"}
                , { nisCode = "24059", municipalityName = "Landen"}
                , { nisCode = "33040", municipalityName = "Langemark-Poelkapelle"}
                , { nisCode = "42011", municipalityName = "Lebbeke"}
                , { nisCode = "41034", municipalityName = "Lede"}
                , { nisCode = "36010", municipalityName = "Ledegem"}
                , { nisCode = "34025", municipalityName = "Lendelede"}
                , { nisCode = "23104", municipalityName = "Lennik"}
                , { nisCode = "71034", municipalityName = "Leopoldsburg"}
                , { nisCode = "55023", municipalityName = "Lessen"}
                , { nisCode = "24062", municipalityName = "Leuven"}
                , { nisCode = "36011", municipalityName = "Lichtervelde"}
                , { nisCode = "23044", municipalityName = "Liedekerke"}
                , { nisCode = "12021", municipalityName = "Lier"}
                , { nisCode = "45063", municipalityName = "Lierde"}
                , { nisCode = "44085", municipalityName = "Lievegem"}
                , { nisCode = "64047", municipalityName = "Lijsem"}
                , { nisCode = "13019", municipalityName = "Lille"}
                , { nisCode = "63046", municipalityName = "Limburg"}
                , { nisCode = "23100", municipalityName = "Linkebeek"}
                , { nisCode = "11025", municipalityName = "Lint"}
                , { nisCode = "24133", municipalityName = "Linter"}
                , { nisCode = "44034", municipalityName = "Lochristi"}
                , { nisCode = "46014", municipalityName = "Lokeren"}
                , { nisCode = "72020", municipalityName = "Lommel"}
                , { nisCode = "23045", municipalityName = "Londerzeel"}
                , { nisCode = "32030", municipalityName = "Lo-Reninge"}
                , { nisCode = "24066", municipalityName = "Lubbeek"}
                , { nisCode = "62063", municipalityName = "Luik"}
                , { nisCode = "71037", municipalityName = "Lummen"}
                , { nisCode = "45064", municipalityName = "Maarkedal"}
                , { nisCode = "72021", municipalityName = "Maaseik"}
                , { nisCode = "73107", municipalityName = "Maasmechelen"}
                , { nisCode = "23047", municipalityName = "Machelen"}
                , { nisCode = "43010", municipalityName = "Maldegem"}
                , { nisCode = "11057", municipalityName = "Malle"}
                , { nisCode = "63049", municipalityName = "Malmedy"}
                , { nisCode = "12025", municipalityName = "Mechelen"}
                , { nisCode = "13021", municipalityName = "Meerhout"}
                , { nisCode = "23050", municipalityName = "Meise"}
                , { nisCode = "44040", municipalityName = "Melle"}
                , { nisCode = "34027", municipalityName = "Menen"}
                , { nisCode = "23052", municipalityName = "Merchtem"}
                , { nisCode = "44043", municipalityName = "Merelbeke"}
                , { nisCode = "13023", municipalityName = "Merksplas"}
                , { nisCode = "33016", municipalityName = "Mesen"}
                , { nisCode = "37007", municipalityName = "Meulebeke"}
                , { nisCode = "35011", municipalityName = "Middelkerke"}
                , { nisCode = "44045", municipalityName = "Moerbeke"}
                , { nisCode = "54007", municipalityName = "Moeskroen"}
                , { nisCode = "13025", municipalityName = "Mol"}
                , { nisCode = "36012", municipalityName = "Moorslede"}
                , { nisCode = "11029", municipalityName = "Mortsel"}
                , { nisCode = "92094", municipalityName = "Namen"}
                , { nisCode = "44048", municipalityName = "Nazareth"}
                , { nisCode = "11030", municipalityName = "Niel"}
                , { nisCode = "71045", municipalityName = "Nieuwerkerken"}
                , { nisCode = "38016", municipalityName = "Nieuwpoort"}
                , { nisCode = "12026", municipalityName = "Nijlen"}
                , { nisCode = "25072", municipalityName = "Nijvel"}
                , { nisCode = "41048", municipalityName = "Ninove"}
                , { nisCode = "64056", municipalityName = "Oerle"}
                , { nisCode = "13029", municipalityName = "Olen"}
                , { nisCode = "35013", municipalityName = "Oostende"}
                , { nisCode = "44052", municipalityName = "Oosterzele"}
                , { nisCode = "31022", municipalityName = "Oostkamp"}
                , { nisCode = "37010", municipalityName = "Oostrozebeke"}
                , { nisCode = "23060", municipalityName = "Opwijk"}
                , { nisCode = "55039", municipalityName = "Opzullik"}
                , { nisCode = "45035", municipalityName = "Oudenaarde"}
                , { nisCode = "35014", municipalityName = "Oudenburg"}
                , { nisCode = "21002", municipalityName = "Oudergem"}
                , { nisCode = "24086", municipalityName = "Oud-Heverlee"}
                , { nisCode = "72042", municipalityName = "Oudsbergen"}
                , { nisCode = "13031", municipalityName = "Oud-Turnhout"}
                , { nisCode = "23062", municipalityName = "Overijse"}
                , { nisCode = "72030", municipalityName = "Peer"}
                , { nisCode = "72043", municipalityName = "Pelt"}
                , { nisCode = "23064", municipalityName = "Pepingen"}
                , { nisCode = "25084", municipalityName = "Perwijs"}
                , { nisCode = "37011", municipalityName = "Pittem"}
                , { nisCode = "33021", municipalityName = "Poperinge"}
                , { nisCode = "12029", municipalityName = "Putte"}
                , { nisCode = "12041", municipalityName = "Puurs-Sint-Amands"}
                , { nisCode = "11035", municipalityName = "Ranst"}
                , { nisCode = "13035", municipalityName = "Ravels"}
                , { nisCode = "13036", municipalityName = "Retie"}
                , { nisCode = "73066", municipalityName = "Riemst"}
                , { nisCode = "13037", municipalityName = "Rijkevorsel"}
                , { nisCode = "36015", municipalityName = "Roeselare"}
                , { nisCode = "45041", municipalityName = "Ronse"}
                , { nisCode = "23097", municipalityName = "Roosdaal"}
                , { nisCode = "24094", municipalityName = "Rotselaar"}
                , { nisCode = "37012", municipalityName = "Ruiselede"}
                , { nisCode = "11037", municipalityName = "Rumst"}
                , { nisCode = "55004", municipalityName = "'s Gravenbrakel"}
                , { nisCode = "63067", municipalityName = "Sankt Vith"}
                , { nisCode = "21015", municipalityName = "Schaarbeek"}
                , { nisCode = "11038", municipalityName = "Schelle"}
                , { nisCode = "24134", municipalityName = "Scherpenheuvel-Zichem"}
                , { nisCode = "11039", municipalityName = "Schilde"}
                , { nisCode = "11040", municipalityName = "Schoten"}
                , { nisCode = "21003", municipalityName = "Sint-Agatha-Berchem"}
                , { nisCode = "23101", municipalityName = "Sint-Genesius-Rode"}
                , { nisCode = "21013", municipalityName = "Sint-Gillis"}
                , { nisCode = "46020", municipalityName = "Sint-Gillis-Waas"}
                , { nisCode = "21012", municipalityName = "Sint-Jans-Molenbeek"}
                , { nisCode = "21014", municipalityName = "Sint-Joost-ten-Node"}
                , { nisCode = "12035", municipalityName = "Sint-Katelijne-Waver"}
                , { nisCode = "21018", municipalityName = "Sint-Lambrechts-Woluwe"}
                , { nisCode = "43014", municipalityName = "Sint-Laureins"}
                , { nisCode = "41063", municipalityName = "Sint-Lievens-Houtem"}
                , { nisCode = "44064", municipalityName = "Sint-Martens-Latem"}
                , { nisCode = "46021", municipalityName = "Sint-Niklaas"}
                , { nisCode = "23077", municipalityName = "Sint-Pieters-Leeuw"}
                , { nisCode = "21019", municipalityName = "Sint-Pieters-Woluwe"}
                , { nisCode = "71053", municipalityName = "Sint-Truiden"}
                , { nisCode = "34043", municipalityName = "Spiere-Helkijn"}
                , { nisCode = "11044", municipalityName = "Stabroek"}
                , { nisCode = "36019", municipalityName = "Staden"}
                , { nisCode = "23081", municipalityName = "Steenokkerzeel"}
                , { nisCode = "46024", municipalityName = "Stekene"}
                , { nisCode = "46025", municipalityName = "Temse"}
                , { nisCode = "25050", municipalityName = "Terhulpen"}
                , { nisCode = "23086", municipalityName = "Ternat"}
                , { nisCode = "24104", municipalityName = "Tervuren"}
                , { nisCode = "71057", municipalityName = "Tessenderlo"}
                , { nisCode = "37015", municipalityName = "Tielt"}
                , { nisCode = "24135", municipalityName = "Tielt-Winge"}
                , { nisCode = "24107", municipalityName = "Tienen"}
                , { nisCode = "73083", municipalityName = "Tongeren"}
                , { nisCode = "31033", municipalityName = "Torhout"}
                , { nisCode = "24109", municipalityName = "Tremelo"}
                , { nisCode = "25105", municipalityName = "Tubeke"}
                , { nisCode = "13040", municipalityName = "Turnhout"}
                , { nisCode = "21016", municipalityName = "Ukkel"}
                , { nisCode = "38025", municipalityName = "Veurne"}
                , { nisCode = "23088", municipalityName = "Vilvoorde"}
                , { nisCode = "33041", municipalityName = "Vleteren"}
                , { nisCode = "51019", municipalityName = "Vloesberg"}
                , { nisCode = "73109", municipalityName = "Voeren"}
                , { nisCode = "13044", municipalityName = "Vorselaar"}
                , { nisCode = "21007", municipalityName = "Vorst"}
                , { nisCode = "13046", municipalityName = "Vosselaar"}
                , { nisCode = "42023", municipalityName = "Waasmunster"}
                , { nisCode = "44073", municipalityName = "Wachtebeke"}
                , { nisCode = "34040", municipalityName = "Waregem"}
                , { nisCode = "21017", municipalityName = "Watermaal-Bosvoorde"}
                , { nisCode = "25112", municipalityName = "Waver"}
                , { nisCode = "63080", municipalityName = "Weismes"}
                , { nisCode = "73098", municipalityName = "Wellen"}
                , { nisCode = "23102", municipalityName = "Wemmel"}
                , { nisCode = "33029", municipalityName = "Wervik"}
                , { nisCode = "13049", municipalityName = "Westerlo"}
                , { nisCode = "42025", municipalityName = "Wetteren"}
                , { nisCode = "34041", municipalityName = "Wevelgem"}
                , { nisCode = "23103", municipalityName = "Wezembeek-Oppem"}
                , { nisCode = "62108", municipalityName = "Wezet"}
                , { nisCode = "42026", municipalityName = "Wichelen"}
                , { nisCode = "37017", municipalityName = "Wielsbeke"}
                , { nisCode = "11050", municipalityName = "Wijnegem"}
                , { nisCode = "12040", municipalityName = "Willebroek"}
                , { nisCode = "37018", municipalityName = "Wingene"}
                , { nisCode = "11052", municipalityName = "Wommelgem"}
                , { nisCode = "45061", municipalityName = "Wortegem-Petegem"}
                , { nisCode = "11053", municipalityName = "Wuustwezel"}
                , { nisCode = "11054", municipalityName = "Zandhoven"}
                , { nisCode = "23094", municipalityName = "Zaventem"}
                , { nisCode = "31040", municipalityName = "Zedelgem"}
                , { nisCode = "42028", municipalityName = "Zele"}
                , { nisCode = "43018", municipalityName = "Zelzate"}
                , { nisCode = "23096", municipalityName = "Zemst"}
                , { nisCode = "55040", municipalityName = "Zinnik"}
                , { nisCode = "11055", municipalityName = "Zoersel"}
                , { nisCode = "71066", municipalityName = "Zonhoven"}
                , { nisCode = "33037", municipalityName = "Zonnebeke"}
                , { nisCode = "41081", municipalityName = "Zottegem"}
                , { nisCode = "24130", municipalityName = "Zoutleeuw"}
                , { nisCode = "31042", municipalityName = "Zuienkerke"}
                , { nisCode = "44081", municipalityName = "Zulte"}
                , { nisCode = "71067", municipalityName = "Zutendaal"}
                , { nisCode = "45065", municipalityName = "Zwalm"}
                , { nisCode = "34042", municipalityName = "Zwevegem"}
                , { nisCode = "11056", municipalityName = "Zwijndrecht"}
        ]
        }
      }
    , Cmd.none
    )

encodeContourRequest : Model -> Encode.Value
encodeContourRequest model =
    let
        bufferInMeters = if model.wizard.enableBuffer then 100 else 0

    in
        Encode.object 
        [
            ("contour", Encode.string model.wizard.customWkt)
            , ("buffer", Encode.int bufferInMeters)
        ]

encodeNisCodeRequest : Model -> Encode.Value
encodeNisCodeRequest model =
    let
        bufferInMeters = if model.wizard.enableBuffer then 100 else 0

    in
    Encode.object 
    [
        ("nisCode", Encode.string model.wizard.nisCode)
        , ("buffer", Encode.int bufferInMeters)
    ]

type Msg
    = DownloadFile
    | GotDownloadProgress Http.Progress
    | FileDownloaded (Result Http.Error Bytes)
    | GotAlertMessage Alert.Message
    | GoToStep WizardStep
    | ToggleBuffer
    | ChangeCustomWkt String
    | ChangeMunicipality String
    | ChangeDescription String
    | ExecuteContourRequest
    | ExecuteNisRequest
    | RequestExecuted (Result Http.Error (String))


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        DownloadFile ->
            let
                oldDownload =
                    model.download

                newDownload =
                    { oldDownload | downloading = True, count = oldDownload.count + 1 }
            in
            ( { model | download = newDownload, alert = Alert.hide model.alert }
            , Cmd.batch
                [
                  Http.cancel ("download-" ++ String.fromInt oldDownload.count)
                  , Http.request
                    { method = "GET"
                    , headers = [ ]
                    , url = model.download.url
                    , body = Http.emptyBody
                    , expect = HttpBytes.expect FileDownloaded
                    , timeout = Nothing
                    , tracker = Just ("download-" ++ String.fromInt newDownload.count)
                    }
                ]
            )

        GotAlertMessage alertMessage ->
            let
                ( alertModel, alertCommand ) =
                    Alert.update alertMessage model.alert
            in
            ( { model | alert = alertModel }
            , Cmd.map GotAlertMessage alertCommand
            )

        GotDownloadProgress progress ->
            case progress of
                Http.Sending _ ->
                    ( model, Cmd.none )

                Http.Receiving r ->
                    let
                        oldDownload =
                            model.download

                        newDownload =
                            { oldDownload | progressing = True, progress = Filesize.format r.received }
                    in
                    ( { model | download = newDownload }
                    , Cmd.none
                    )

        GoToStep step -> 
                let 
                    oldWizard = 
                        model.wizard

                    newWizard =
                        { oldWizard | previousStep = oldWizard.step, step = step }
                in
                    ( { model | wizard = newWizard }
                            , Cmd.none
                            )

        ChangeCustomWkt newCustomWkt -> 
                let 
                    oldWizard = 
                        model.wizard

                    newWizard =
                        { oldWizard | customWkt = newCustomWkt }
                in
                    ( { model | wizard = newWizard }, Cmd.none )

        ChangeMunicipality newNisCode -> 
                let 
                    oldWizard = 
                        model.wizard

                    newWizard =
                        { oldWizard | nisCode = newNisCode }
                in
                    ( { model | wizard = newWizard }, Cmd.none )

        ChangeDescription newDescription -> 
                let 
                    oldWizard = 
                        model.wizard

                    newWizard =
                        { oldWizard | description = newDescription }
                in
                    ( { model | wizard = newWizard }, Cmd.none )

        ToggleBuffer -> 
                let 
                    oldWizard = 
                        model.wizard

                    newWizard =
                        { oldWizard | enableBuffer = not oldWizard.enableBuffer }
                in
                    ( { model | wizard = newWizard } , Cmd.none )

        ExecuteContourRequest -> 
                let
                    url =
                        if String.endsWith "/" model.flags.endpoint then
                            String.concat [ model.flags.endpoint, "v1/wegen/extract/downloadaanvragen/percontour" ]

                        else
                            String.concat [ model.flags.endpoint, "/v1/wegen/extract/downloadaanvragen/percontour" ]
                in
                ( model
            , Http.request
                { method = "POST"
                , headers = [ Http.header "x-api-key" model.flags.apikey ]
                , url = url
                , body = Http.jsonBody (encodeContourRequest model) 
                , expect = Http.expectJson RequestExecuted (string)
                , timeout = Nothing
                , tracker = Nothing
                }
            )

        ExecuteNisRequest -> 
                let
                    url =
                        if String.endsWith "/" model.flags.endpoint then
                            String.concat [ model.flags.endpoint, "v1/wegen/extract/downloadaanvragen/perniscode" ]

                        else
                            String.concat [ model.flags.endpoint, "/v1/wegen/extract/downloadaanvragen/perniscode" ]
                in
                ( model
            , Http.request
                { method = "POST"
                , headers = [ Http.header "x-api-key" model.flags.apikey ]
                , url = url
                , body = Http.jsonBody (encodeNisCodeRequest model) 
                , expect = Http.expectJson RequestExecuted (string)
                , timeout = Nothing
                , tracker = Nothing
                }
            )

        RequestExecuted _-> 
            ( model, load "/activity.html")
        FileDownloaded result ->
            case result of
                Ok bytes ->
                    let
                        oldDownload =
                            model.download

                        newDownload =
                            { oldDownload | downloading = False, progressing = False, progress = "" }
                    in
                    ( { model | download = newDownload, alert = Alert.hide model.alert }
                    , File.Download.bytes "download.zip" "application/zip" bytes
                    )

                Err error ->
                    let
                        oldDownload =
                            model.download

                        newDownload =
                            { oldDownload | downloading = False, progressing = False, progress = "" }
                    in
                    case error of
                        Http.BadUrl _ ->
                            ( { model | download = newDownload, alert = Alert.showError model.alert "Er was een probleem bij het downloaden - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | download = newDownload, alert = Alert.showError model.alert "Er was een probleem bij het downloaden - de operatie nam teveel tijd in beslag." }
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | download = newDownload, alert = Alert.showError model.alert "Er was een probleem bij het downloaden - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                503 ->
                                    ( { model | download = newDownload, alert = Alert.showError model.alert "Downloaden is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
                                    , Cmd.none
                                    )

                                _ ->
                                    ( { model | download = newDownload, alert = Alert.showError model.alert "Er was een probleem bij het downloaden - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody _ ->
                            ( { model | download = newDownload, alert = Alert.showError model.alert "Er was een probleem bij het downloaden - dit kan duiden op een probleem met de website." }
                            , Cmd.none
                            )


viewMain : Model -> Html Msg
viewMain model =
    main_ [ id "main" ]
        [ Alert.view model.alert |> Html.map GotAlertMessage
        -- , viewDownload model.download
        , viewWizard model.wizard
        ]

viewWizard : WizardModel -> Html Msg
viewWizard model = 
    section [ class "region" ]
    [ div
        [ classList [ ( "layout", True ), ( "layout--wide", True ), ( "vl-typography", True)] ]
        [ div []
            [
                 h2 [class "h2"] [ text "Wizard extract downloaden"]] 
                , text "Volg de stappen hieronder om een extract van het Wegenregister te downloaden."
                , if model.step == Step1 then
                    div []
                    [
                        h3 [class "h3"] [ text "Stap 1: gemeentecontour of eigen contour?"]
                        , div [class "u-spacer--large"] 
                        [
                            text "Wenst u een extract ter grootte van een gemeente, of een extract ter grootte van een willekeurige contour in WKT-formaat?"
                        ]
                        , div [class "button-group"] 
                        [
                            button [class "button button--large", onClick (GoToStep Step2_Municipality)] [text "Gemeentecontour"]
                            , button [class "button button--large", onClick (GoToStep Step2_Contour)] [text "Eigen contour"]
                        ]
                    ]
                else if model.step == Step2_Municipality then
                    div []
                    [
                        h3 [class "h3"] [ text "Stap 2: Details van de contour"]
                        , div [class "u-spacer--large"] 
                        [
                            text "Vul hieronder de gemeente in waarvoor u de contour wenst op te halen"
                            , div [] 
                            [
                                select [class "select", onInput ChangeMunicipality] <|
                                List.map
                                    (\municipality -> 
                                        option [ value (municipality.nisCode), selected (municipality.nisCode == model.nisCode) ] [ text (municipality.municipalityName) ])
                                    model.municipalities
                            ]
                        ]
                        , div [class "u-spacer--large"] 
                        [
                            text "Wenst u een bufferzone van 100m toe te voegen aan de contour?"
                            , div []
                            [
                                label [] [
                                    input [type_ "checkbox", onClick ToggleBuffer, checked model.enableBuffer] []
                                    , text "Voeg buffer toe"
                                ]
                                
                            ]
                        ]
                        , div [class "button-group"] 
                        [
                            button [class "button button--large", onClick (GoToStep Step1)] [text "Vorige"]
                            , button [class "button button--large", onClick (GoToStep Step3_Municipality)] [text "Volgende"]
                        ]
                    ]
                else if model.step == Step2_Contour then
                    div []
                    [
                        h3 [class "h3"] [ text "Stap 2: Details van de contour"]
                        , div [class "u-spacer--large"] 
                        [
                            text "Geef een contour op (in WKT-formaat, coördinatensysteem Lambert 1972) waarvoor u het extract wenst op te halen:"
                            , form [class "form"] 
                            [
                                textarea [rows 5, cols 40, value model.customWkt, onInput ChangeCustomWkt] []
                            ]
                        ]
                        , div [class "u-spacer--large"] 
                        [
                            text "Wenst u een bufferzone van 100m toe te voegen aan de contour?"
                            , div []
                            [
                                label [] [
                                    input [type_ "checkbox", onClick ToggleBuffer, checked model.enableBuffer] []
                                    , text "Voeg buffer toe"
                                ]
                                
                            ]
                        ]
                        , div [class "button-group"] 
                        [
                            button [class "button button--large", onClick (GoToStep Step1)] [text "Vorige"]
                            , button [class "button button--large", onClick (GoToStep Step3_Contour)] [text "Volgende"]
                        ]
                    ]
                else if model.step == Step3_Municipality then
                    div []
                    [
                        h3 [class "h3"] [ text "Stap 3: Beschrijving van het extract"]
                        , div [class "u-spacer--large"] 
                        [
                            text "Geef een beschrijving op van het extract."
                            , form [class "form"] 
                            [
                                textarea [rows 5, cols 40, value model.description, onInput ChangeDescription] []
                            ]
                        ]
                        , div [class "button-group"] 
                        [
                            button [class "button button--large", onClick (GoToStep model.previousStep)] [text "Vorige"]
                            , button [class "button button--large", onClick ExecuteNisRequest] [text "Extract aanvragen"]
                        ]
                    ]
                else
                    div []
                    [
                        h3 [class "h3"] [ text "Stap 3: Beschrijving van het extract"]
                        , div [class "u-spacer--large"] 
                        [
                            text "Geef een beschrijving op van het extract."
                            , form [class "form"] 
                            [
                                textarea [rows 5, cols 40, value model.description, onInput ChangeDescription] []
                            ]
                        ]
                        , div [class "button-group"] 
                        [
                            button [class "button button--large", onClick (GoToStep model.previousStep)] [text "Vorige"]
                            , button [class "button button--large", onClick ExecuteContourRequest] [text "Extract aanvragen"]
                        ]
                    ]
        ]
    ]

viewDownload : DownloadModel -> Html Msg
viewDownload model =
    section [ class "region" ]
        [ div
            [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
            [ div []
                [ h1 [ class "h2 cta-title__title" ]
                    [ text "Downloaden" ]
                , ul [ class "grid grid--is-stacked js-equal-height-container u-spacer", style "clear" "both" ]
                    [ li [ class "col--4-12 col--6-12--m col--12-12--xs" ]
                        [ a
                            [ classList [ ( "not-allowed", False ), ( "doormat", True ), ( "doormat--graphic", True ), ( "js-equal-height", True ), ( "paragraph--type--doormat-graphic", True ), ( "paragraph--view-mode--default", True ) ]
                            , onClick DownloadFile
                            ]
                            [ div [ class "doormat__graphic-wrapper" ]
                                []
                            , h2 [ class "doormat__title" ]
                                [ span [] [ text model.title ] ]
                            , text "Download het volledige wegenregister als zip‑bestand"
                            , if model.downloading then
                                div [ class "download-progress" ]
                                    (if model.progressing then
                                        [ span [ class "progress" ]
                                            [ text (String.concat [ model.progress, " ontvangen" ]) ]
                                        , div [ class "loader" ]
                                            []
                                        ]

                                     else
                                        [ div [ class "loader" ] []
                                        ]
                                    )

                              else
                                text ""
                            ]
                        ]
                    ]
                ]
            ]
        ]


view : Model -> Html Msg
view model =
    div [ class "page" ]
        [ Header.viewBanner ()
        , Header.viewHeader model.header
        , viewMain model
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub Msg
subscriptions model =
    Http.track ("download-" ++ String.fromInt model.download.count) GotDownloadProgress

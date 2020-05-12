module Information exposing (Msg(..), init, main, subscriptions, update, view)

import Alert exposing (AlertKind(..), AlertModel, AlertMsg(..), hideAlert, showError, viewAlert)
import Browser
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, div, main_, section, table, tbody, td, text, th, thead, tr)
import Html.Attributes exposing (class, classList, id)
import Http
import Json.Decode as D
import Time exposing (Posix, every)

main: Program String Model Msg
main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias RoadNetworkInfo =
    { organizationCount : Int
    , roadNodeCount : Int
    , roadSegmentCount : Int
    , gradeSeparatedJunctionCount : Int
    }


informationDecoder : D.Decoder RoadNetworkInfo
informationDecoder =
    D.map4 RoadNetworkInfo
        (D.field "organizationCount" D.int)
        (D.field "roadNodeCount" D.int)
        (D.field "roadSegmentCount" D.int)
        (D.field "gradeSeparatedJunctionCount" D.int)


type alias InformationModel =
    { title : String
    , url : String
    , organizationCount : String
    , roadNodeCount : String
    , roadSegmentCount : String
    , gradeSeparatedJunctionCount : String
    }


type alias Model =
    { header : HeaderModel
    , information : InformationModel
    , alert : AlertModel
    }


init : String -> ( Model, Cmd Msg )
init url =
    ( { header = Header.init |> Header.informationBecameActive
      , information =
            { title = "Register dump"
            , url = if String.endsWith "/" url then String.concat [ url, "v1/information" ] else String.concat [ url, "/v1/information" ]
            , organizationCount = ""
            , roadNodeCount = ""
            , roadSegmentCount = ""
            , gradeSeparatedJunctionCount = ""
            }
      , alert =
            { title = ""
            , kind = Error
            , visible = False
            , closeable = True
            , hasIcon = True
            }
      }
    , Http.get
        { url = if String.endsWith "/" url then String.concat [ url, "v1/information" ] else String.concat [ url, "/v1/information" ]
        , expect = Http.expectJson GotInformation informationDecoder
        }
    )


type Msg
    = DownloadInformation
    | GotInformation (Result Http.Error RoadNetworkInfo)
    | GotAlertMsg AlertMsg
    | Tick Posix


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        DownloadInformation ->
            ( model
            , Http.get
                { url = model.information.url
                , expect = Http.expectJson GotInformation informationDecoder
                }
            )

        Tick _ ->
            ( model
            , Http.get
                { url = model.information.url
                , expect = Http.expectJson GotInformation informationDecoder
                }
            )

        GotInformation result ->
            case result of
                Ok info ->
                    let
                        oldInformation =
                            model.information

                        newInformation =
                            { oldInformation
                                | organizationCount = String.fromInt info.organizationCount
                                , roadNodeCount = String.fromInt info.roadNodeCount
                                , roadSegmentCount = String.fromInt info.roadSegmentCount
                                , gradeSeparatedJunctionCount = String.fromInt info.gradeSeparatedJunctionCount
                            }
                    in
                    ( { model | information = newInformation, alert = hideAlert model.alert }
                    , Cmd.none
                    )

                Err error ->
                    case error of
                        Http.BadUrl _ ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het downloaden van de informatie - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het downloaden van de informatie - de operatie nam teveel tijd in beslag." }
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het downloaden van de informatie - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                _ ->
                                    ( { model | alert = showError model.alert "Er was een probleem bij het downloaden van de informatie - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody _ ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het downloaden van de informatie - dit kan duiden op een probleem met de website." }
                            , Cmd.none
                            )

        GotAlertMsg alertMsg ->
            case alertMsg of
                CloseAlert ->
                    ( { model | alert = hideAlert model.alert }
                    , Cmd.none
                    )

viewInformation : InformationModel -> Html Msg
viewInformation model =
    section [ class "region" ]
        [ div
            [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
            [ div [ class "u-table-overflow" ]
                [ table [ class "data-table data-table--lined" ]
                    [ thead []
                        [ tr []
                            [ th []
                                [ text "Gegeven" ]
                            , th []
                                [ text "Waarde" ]
                            ]
                        ]
                    , tbody []
                        [ tr []
                            [ td []
                                [ text "# organisaties" ]
                            , td []
                                [ text model.organizationCount ]
                            ]
                        , tr []
                            [ td []
                                [ text "# wegknopen" ]
                            , td []
                                [ text model.roadNodeCount ]
                            ]
                        , tr []
                            [ td []
                                [ text "# wegsegmenten" ]
                            , td []
                                [ text model.roadSegmentCount ]
                            ]
                        , tr []
                            [ td []
                                [ text "# ongelijkgrondse kruisingen" ]
                            , td []
                                [ text model.gradeSeparatedJunctionCount ]
                            ]
                        ]
                    ]
                ]
            ]
        ]


viewMain : Model -> Html Msg
viewMain model =
    main_ [ id "main" ]
        [ viewAlert model.alert |> Html.map GotAlertMsg
        , viewInformation model.information
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
subscriptions _ =
    every 5000 Tick

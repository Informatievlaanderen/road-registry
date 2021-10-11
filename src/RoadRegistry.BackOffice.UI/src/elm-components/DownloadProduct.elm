module DownloadProduct exposing (Msg(..), init, main, subscriptions, update, view)

import Alert
import Browser
import Bytes exposing (Bytes)
import File.Download
import Filesize
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, a, br, div, h1, h2, input, label, li, main_, section, span, text, ul)
import Html.Attributes exposing (class, classList, disabled, for, id, placeholder, style, type_)
import Html.Events exposing (onClick, onInput)
import Http
import HttpBytes
import Date


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
    , version : String
    , downloading : Bool
    , progressing : Bool
    , progress : String
    , count: Int
    , apikey: String
    }


type alias Model =
    { header : HeaderModel
    , download : DownloadModel
    , alert : Alert.Model
    }


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { header = Header.init |> Header.downloadProductBecameActive
      , download =
            { title = "Register download product"
            , url =
                if String.endsWith "/" flags.oldEndpoint then
                    String.concat [ flags.oldEndpoint, "v1/download/for-product" ]

                else
                    String.concat [ flags.oldEndpoint, "/v1/download/for-product" ]
            , version = ""
            , downloading = False
            , progressing = False
            , progress = ""
            , count = 0
            , apikey = flags.apikey
            }
      , alert = Alert.init ()
      }
    , Cmd.none
    )


type Msg
    = DownloadFile
    | ChangeVersion String
    | GotDownloadProgress Http.Progress
    | FileDownloaded (Result Http.Error Bytes)
    | GotAlertMessage Alert.Message

update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        DownloadFile ->
            case Date.fromIsoString model.download.version of
              Ok _ ->
                let
                  oldDownload =
                      model.download

                  newDownload =
                      { oldDownload | downloading = True, count = oldDownload.count + 1 }
                in
                  ( { model | download = newDownload, alert = Alert.hide model.alert }
                  , Cmd.batch
                    [ Http.cancel ("download-" ++ String.fromInt oldDownload.count)
                      , Http.request
                        { method = "GET"
                        , headers = [ ]
                        , url = String.concat [ model.download.url, "/", model.download.version ]
                        , body = Http.emptyBody
                        , expect = HttpBytes.expect FileDownloaded
                        , timeout = Nothing
                        , tracker = Just ("download-" ++ String.fromInt newDownload.count)
                        }
                    ]
                  )
              Err _ ->
                ( { model | alert = Alert.showError model.alert "Gelieve een geldige versie in het formaat jjjjmmdd (j=jaar m=maand d=dag) op te geven." }
                  , Cmd.none
                  )

        ChangeVersion version ->
            let
              oldDownload =
                model.download

              newDownload =
                { oldDownload | version = version }
            in
              ( { model | download = newDownload, alert = Alert.hide model.alert }, Cmd.none)

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
                    , File.Download.bytes (String.concat [ "wegenregister-", model.download.version , ".zip" ]) "application/zip" bytes
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
        , viewDownload model.download
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
                        [ label [ for "version", class "form__label" ] [ text "Versie"]
                          , input [ id "version", classList [ ("input-field", True), ("input-field--disabled", model.downloading), ("input-field--block", True) ], type_ "text", placeholder "", disabled model.downloading, onInput ChangeVersion ] [ ]
                          , br [] []
                          , a
                            [ classList [ ( "not-allowed", False ), ( "doormat", True ), ( "doormat--graphic", True ), ( "js-equal-height", True ), ( "paragraph--type--doormat-graphic", True ), ( "paragraph--view-mode--default", True ) ]
                            , onClick DownloadFile
                            ]
                            [ div [ class "doormat__graphic-wrapper" ]
                                []
                            , h2 [ class "doormat__title" ]
                                [ span [] [ text model.title ] ]
                            , text "Download het wegenregister product."
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

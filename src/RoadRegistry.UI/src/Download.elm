module Download exposing (Msg(..), init, main, subscriptions, update, view)

import Alert exposing (AlertKind(..), AlertModel, AlertMsg(..), hideAlert, showAlert, viewAlert)
import Browser
import Bytes exposing (Bytes)
import File.Download
import Footer
import Header exposing (HeaderAction, HeaderModel, TabAction)
import Html exposing (Html, a, div, h1, h2, li, main_, span, text, ul)
import Html.Attributes exposing (class, classList, id, style)
import Html.Events exposing (onClick)
import Http
import HttpBytes
import Filesize


main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias DownloadModel =
    { title : String, url : String, downloading : Bool, progressing : Bool, progress : String }


type alias Model =
    { header : HeaderModel, download : DownloadModel, alert : AlertModel }


init : String -> ( Model, Cmd Msg )
init url =
    ( { header =
            { headerActions =
                [ { title = "Operator", link = Nothing }
                , { title = "Afmelden", link = Nothing }
                ]
            , tabActions =
                [ { title = "Downloaden", link = "/download.html", active = True }
                , { title = "Opladen", link = "/upload.html", active = False }
                ]
            }
      , download =
            { title = "Register dump"
            , url = String.concat [ url, "/v1/download" ]
            , downloading = False
            , progressing = False
            , progress = ""
            }
      , alert =
            { title = ""
            , kind = Error
            , visible = False
            , closeable = True
            , hasIcon = True
            }
      }
    , Cmd.none
    )


type Msg
    = Download
    | GotDownloadProgress Http.Progress
    | Downloaded (Result Http.Error Bytes)
    | GotAlertMsg AlertMsg


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        Download ->
            let
                oldDownload =
                    model.download

                newDownload =
                    { oldDownload | downloading = True }
            in
            ( { model | download = newDownload, alert = hideAlert model.alert }
            , Http.request
                { method = "GET"
                , headers = []
                , url = model.download.url
                , body = Http.emptyBody
                , expect = HttpBytes.expect Downloaded
                , timeout = Nothing
                , tracker = Just "download"
                }
            )

        GotAlertMsg alertMsg ->
            case alertMsg of
                CloseAlert ->
                    ( { model | alert = hideAlert model.alert }
                    , Cmd.none
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

        Downloaded result ->
            case result of
                Ok bytes ->
                    let
                        oldDownload =
                            model.download

                        newDownload =
                            { oldDownload | downloading = False, progressing = False, progress = "" }
                    in
                    ( { model | download = newDownload, alert = hideAlert model.alert }
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
                            ( { model | download = newDownload, alert = showAlert model.alert "Er was een probleem bij het downloaden - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | download = newDownload, alert = showAlert model.alert "Er was een probleem bij het downloaden - de operatie nam teveel tijd in beslag."}
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | download = newDownload, alert = showAlert model.alert "Er was een probleem bij het downloaden - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                503 ->
                                    ( { model | download = newDownload, alert = showAlert model.alert "Downloaden is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
                                    , Cmd.none
                                    )

                                _ ->
                                    ( { model | download = newDownload, alert = showAlert model.alert "Er was een probleem bij het downloaden - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody _ ->
                            ( { model | download = newDownload, alert = showAlert model.alert "Er was een probleem bij het downloaden - dit kan duiden op een probleem met de website." }
                            , Cmd.none
                            )


viewDownload : DownloadModel -> Html Msg
viewDownload model =
    main_ [ id "main" ]
        [ div [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                [ div []
                    [ h1 [ class "h2 cta-title__title" ]
                        [ text "Downloaden" ]
                    , ul [ class "grid grid--is-stacked js-equal-height-container u-spacer", style "clear" "both" ]
                        [ li [ class "col--4-12 col--6-12--m col--12-12--xs" ]
                            [ a
                                [ classList [ ( "not-allowed", False ), ( "doormat", True ), ( "doormat--graphic", True ), ( "js-equal-height", True ), ( "paragraph--type--doormat-graphic", True ), ( "paragraph--view-mode--default", True ) ]
                                , onClick Download
                                ]
                                [ div [ class "doormat__graphic-wrapper" ]
                                    []
                                , h2 [ class "doormat__title" ]
                                    [ span [] [ text model.title ] ]
                                , text "Download het volledige wegenregister als zip‑bestand."
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
        ]


view : Model -> Html Msg
view model =
    div [ class "page" ]
        [ Header.viewBanner ()
        , Header.viewHeader model.header
        , viewAlert model.alert |> Html.map GotAlertMsg
        , viewDownload model.download
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub Msg
subscriptions _ =
    Http.track "download" GotDownloadProgress

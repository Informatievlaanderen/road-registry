module Upload exposing (Msg(..), init, main, subscriptions, update, view)

import Alert
import Browser
import File exposing (File)
import File.Select as Select
import Filesize
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, a, div, h1, h2, li, main_, section, span, text, ul)
import Html.Attributes exposing (class, classList, id, style)
import Html.Events exposing (onClick)
import Http


main : Program Flags Model Msg
main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias UploadModel =
    { title : String
    , url : String
    , uploading : Bool
    , progressing : Bool
    , progress : String
    , apikey : String
    }

type alias Flags =
    { endpoint : String
    , oldEndpoint : String
    , apikey : String
    }

type alias Model =
    { header : HeaderModel
    , upload : UploadModel
    , alert : Alert.Model
    }


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { header = Header.init |> Header.uploadBecameActive
      , upload =
            { title = "Feature compare"
            , url =
                if String.endsWith "/" flags.oldEndpoint then
                    String.concat [ flags.oldEndpoint , "v1/upload" ]

                else
                    String.concat [ flags.oldEndpoint, "/v1/upload" ]
            , uploading = False
            , progressing = False
            , progress = ""
            , apikey = flags.apikey
            }
      , alert = Alert.init ()
      }
    , Cmd.none
    )


type Msg
    = SelectFile
    | FileSelected File
    | GotUploadProgress Http.Progress
    | GotAlertMessage Alert.Message
    | FileUploaded (Result Http.Error ())


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        SelectFile ->
            ( { model | alert = Alert.hide model.alert }
            , Select.file [ "application/zip" ] FileSelected
            )

        FileSelected file ->
            let
                oldUpload =
                    model.upload

                newUpload =
                    { oldUpload | uploading = True }
            in
            ( { model | upload = newUpload }
            , Http.request
                { method = "POST"
                , headers = [ ]
                , url = model.upload.url
                , body = Http.multipartBody [ Http.filePart "archive" file ]
                , expect = Http.expectWhatever FileUploaded
                , timeout = Nothing
                , tracker = Just "upload"
                }
            )

        GotUploadProgress progress ->
            case progress of
                Http.Sending s ->
                    let
                        oldUpload =
                            model.upload

                        newUpload =
                            { oldUpload | progressing = True, progress = Filesize.format s.sent }
                    in
                    ( { model | upload = newUpload }
                    , Cmd.none
                    )

                Http.Receiving _ ->
                    ( model, Cmd.none )

        GotAlertMessage alertMessage ->
            let
                ( alertModel, alertCommand ) =
                    Alert.update alertMessage model.alert
            in
            ( { model | alert = alertModel }
            , Cmd.map GotAlertMessage alertCommand
            )

        FileUploaded result ->
            case result of
                Ok _ ->
                    let
                        oldUpload =
                            model.upload

                        newUpload =
                            { oldUpload | uploading = False, progressing = False, progress = "" }
                    in
                    ( { model | upload = newUpload, alert = Alert.showSuccess model.alert "Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen. U kan de vooruitgang volgen via Activiteit." }
                    , Cmd.none
                    )

                Err error ->
                    let
                        oldUpload =
                            model.upload

                        newUpload =
                            { oldUpload | uploading = False, progressing = False, progress = "" }
                    in
                    case error of
                        Http.BadUrl _ ->
                            ( { model | upload = newUpload, alert = Alert.showError model.alert "Er was een probleem bij het opladen - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | upload = newUpload, alert = Alert.showError model.alert "Er was een probleem bij het opladen - de operatie nam teveel tijd in beslag." }
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | upload = newUpload, alert = Alert.showError model.alert "Er was een probleem bij het opladen - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                503 ->
                                    ( { model | upload = newUpload, alert = Alert.showError model.alert "Opladen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
                                    , Cmd.none
                                    )

                                415 ->
                                    ( { model | upload = newUpload, alert = Alert.showError model.alert "Opladen is enkel mogelijk op basis van zip bestanden. Probeer het opnieuw met een correct bestand." }
                                    , Cmd.none
                                    )

                                _ ->
                                    ( { model | upload = newUpload, alert = Alert.showError model.alert "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody _ ->
                            ( { model | upload = newUpload, alert = Alert.showError model.alert "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website." }
                            , Cmd.none
                            )


viewUpload : UploadModel -> Html Msg
viewUpload model =
    section [ class "region" ]
        [ div
            [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
            [ div []
                [ h1 [ class "h2 cta-title__title" ]
                    [ text "Opladen" ]
                , ul [ class "grid grid--is-stacked js-equal-height-container u-spacer", style "clear" "both" ]
                    [ li [ class "col--4-12 col--6-12--m col--12-12--xs" ]
                        [ a
                            [ classList [ ( "not-allowed", False ), ( "doormat", True ), ( "doormat--graphic", True ), ( "js-equal-height", True ), ( "paragraph--type--doormat-graphic", True ), ( "paragraph--view-mode--default", True ) ]
                            , onClick SelectFile
                            ]
                            [ div [ class "doormat__graphic-wrapper" ]
                                []
                            , h2 [ class "doormat__title" ]
                                [ span [] [ text model.title ] ]
                            , text "Selecteer het zip‑bestand met de op te laden verschillen."
                            , if model.uploading then
                                div [ class "download-progress" ]
                                    (if model.progressing then
                                        [ span [ class "progress" ]
                                            [ text (String.concat [ model.progress, " verzonden" ]) ]
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


viewMain : Model -> Html Msg
viewMain model =
    main_ [ id "main" ]
        [ Alert.view model.alert |> Html.map GotAlertMessage
        , viewUpload model.upload
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
    Http.track "upload" GotUploadProgress

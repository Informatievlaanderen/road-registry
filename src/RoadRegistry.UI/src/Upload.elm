module Upload exposing (Msg(..), init, main, subscriptions, update, view)

import Alert exposing (AlertKind(..), AlertModel, AlertMsg(..), hideAlert, showError, showSuccess, viewAlert)
import Browser
import File exposing (File)
import File.Select as Select
import Filesize
import Footer
import Header exposing (HeaderAction, HeaderModel, TabAction)
import Html exposing (Html, a, button, div, h1, h2, header, i, input, label, li, main_, nav, small, span, text, ul)
import Html.Attributes exposing (accept, attribute, class, classList, for, href, id, name, style, target, type_)
import Html.Events exposing (onClick)
import Http


main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias UploadModel =
    { title : String, url : String, uploading : Bool, progressing : Bool, progress : String }


type alias Model =
    { header : HeaderModel, upload : UploadModel, alert : AlertModel }


init : String -> ( Model, Cmd Msg )
init url =
    ( { header =
            { headerActions =
                [ { title = "Operator", link = Nothing }
                , { title = "Afmelden", link = Nothing }
                ]
            , tabActions =
                [ { title = "Downloaden", link = "/download.html", active = False }
                , { title = "Opladen", link = "/upload.html", active = True }
                ]
            }
      , upload =
            { title = "Feature compare"
            , url = String.concat [ url, "/v1/upload" ]
            , uploading = False
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
    = SelectFile
    | FileSelected File
    | GotUploadProgress Http.Progress
    | GotAlertMsg AlertMsg
    | FileUploaded (Result Http.Error ())


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        SelectFile ->
            ( { model | alert = hideAlert model.alert }
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
                , headers = []
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

        GotAlertMsg alertMsg ->
            case alertMsg of
                CloseAlert ->
                    ( { model | alert = hideAlert model.alert }
                    , Cmd.none
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
                    ( { model | upload = newUpload, alert = showSuccess model.alert "Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen." }
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
                            ( { model | upload = newUpload, alert = showError model.alert "Er was een probleem bij het opladen - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | upload = newUpload, alert = showError model.alert "Er was een probleem bij het opladen - de operatie nam teveel tijd in beslag." }
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | upload = newUpload, alert = showError model.alert "Er was een probleem bij het opladen - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                503 ->
                                    ( { model | upload = newUpload, alert = showError model.alert "Opladen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
                                    , Cmd.none
                                    )

                                415 ->
                                    ( { model | upload = newUpload, alert = showError model.alert "Opladen is enkel mogelijk op basis van zip bestanden. Probeer het opnieuw met een correct bestand." }
                                    , Cmd.none
                                    )

                                _ ->
                                    ( { model | upload = newUpload, alert = showError model.alert "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody _ ->
                            ( { model | upload = newUpload, alert = showError model.alert "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website." }
                            , Cmd.none
                            )


viewUpload : UploadModel -> Html Msg
viewUpload model =
    main_ [ id "main" ]
        [ div [ class "region" ]
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
                , div
                    [ class "upload js-upload"
                    , attribute "data-upload-t-close" "Sluiten"
                    , attribute "data-upload-t-drop" "Laat de bijlage hier los om op te laden"
                    , attribute "data-upload-allow-drop" ""
                    ]
                    [ div
                        [ class "upload__element" ]
                        [ input
                            [ class "upload__element__input"
                            , type_ "file"
                            , name "files[1]"
                            , id "files[1]"
                            , attribute "data-upload-multiple-caption" "{count} bestand(en) geselecteerd"
                            , attribute "data-upload-error-message-filesize" "Het bestand mag max :maxFsz zijn."
                            , attribute "data-upload-error-message-maxfiles" "Je kan maximum :maxfl bestand(en) opladen."
                            , attribute "data-upload-max-size" "10485760"
                            , attribute "data-upload-max-files" "1"
                            , accept ".zip"
                            ]
                            []
                        , label
                            [ class "upload__element__label", for "files[1]" ]
                            [ i [ class "vi vi-paperclip" ]
                                []
                            , span []
                                [ text "Bijlage toevoegen" ]
                            , small []
                                [ text "Sleep de bijlage naar hier om toe te voegen" ]
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
        , viewUpload model.upload
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub Msg
subscriptions _ =
    Http.track "upload" GotUploadProgress

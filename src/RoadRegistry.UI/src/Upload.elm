module Upload exposing (Msg(..), init, main, subscriptions, update, view)

import Browser
import File exposing (File)
import File.Select as Select
import Footer
import Header exposing (HeaderAction, HeaderModel, TabAction)
import Html exposing (Html, a, button, div, h1, h2, header, li, main_, nav, span, text, ul)
import Html.Attributes exposing (attribute, class, classList, href, id, style, target)
import Html.Events exposing (onClick)
import Http

main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias UploadModel =
    { title : String, url : String, uploading : Bool, progressing : Bool, progress : Int }


type alias Model =
    { header : HeaderModel, upload : UploadModel }


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
            , progress = 0
            }
      }
    , Cmd.none
    )


type Msg
    = SelectFile
    | FileSelected File
    | GotProgress Http.Progress
    | FileUploaded (Result Http.Error ())


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        SelectFile ->
            ( model, Select.file ["application/zip"] FileSelected )
        FileSelected file ->
            ( model, Cmd.none )
        GotProgress progress ->
            ( model, Cmd.none )
        FileUploaded result ->
            ( model, Cmd.none )

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
                                , text "Selecteer het zip‑bestand met de verschillen."
                                , if model.uploading then
                                    div [ class "download-progress" ]
                                        (if model.progressing then
                                            [ span [ class "progress" ]
                                                [ text (String.concat [ String.fromInt model.progress, " byte(s) verzonden" ]) ]
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
        , viewUpload model.upload
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none

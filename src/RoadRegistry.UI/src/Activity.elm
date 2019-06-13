module Activity exposing (Msg(..), init, main, subscriptions, update, view)

import Alert exposing (AlertKind(..), AlertModel, AlertMsg(..), hideAlert, showError, viewAlert)
import Browser
import Bytes exposing (Bytes)
import Footer
import Header exposing (HeaderAction, HeaderModel, TabAction)
import Html exposing (Html, a, br, div, h1, h2, h3, i, li, main_, section, span, text, ul)
import Html.Attributes exposing (class, classList, href, id, style)
import Html.Attributes.Aria exposing (ariaHidden)
import Html.Events exposing (onClick)
import Http
import HttpBytes
import Json.Decode as Decode
import Json.Decode.Extra exposing(when)
import Time exposing(Posix, every)

main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias FileProblems = { file: String, problems: List String }

type ActivityListEntryContent
    = BeganRoadNetworkImport
    | CompletedRoadNetworkImport
    | RoadNetworkChangesArchiveAccepted { archive : String, problems : FileProblems }
    | RoadNetworkChangesArchiveRejected { archive : String, problems : FileProblems }
    | RoadNetworkChangesArchiveUploaded { archive : String }


type alias ActivityListEntry =
    { id : String, title : String, day : String, month : String, expanded : Bool, disabled : Bool, detail : ActivityListEntryContent }


type alias ActivityModel =
    { title : String, url : String, entries : List ActivityListEntry }


type alias Model =
    { header : HeaderModel, activity : ActivityModel, alert : AlertModel }

decodeEntryContentType: Decode.Decoder String
decodeEntryContentType =
    Decode.field "type" Decode.string

is : a -> a -> Bool
is a b =
    a == b

decodeFileProblem: Decode.Decoder FileProblems
decodeFileProblem =
    Decode.map2 FileProblems
      (Decode.field "file" Decode.string)
      (Decode.field "problems" (Decode.list Decode.string))

decodeRoadNetworkChangesArchiveAccepted : Decode.Decoder ActivityListEntryContent
decodeRoadNetworkChangesArchiveAccepted =
    Decode.field "content"
      (
        Decode.map2 RoadNetworkChangesArchiveAccepted
          (Decode.field "archiveId" Decode.string)
          (Decode.field "problems" (Decode.list decodeFileProblem))
      )

decodeEntry : Decode.Decoder ActivityListEntry
decodeEntry =
    Decode.map7 ActivityListEntry
      (Decode.field "id" Decode.int |> Decode.map String.fromInt)
      (Decode.field "title" Decode.string)
      (Decode.succeed "14")
      (Decode.succeed "jun")
      (Decode.succeed True)
      (Decode.succeed False)
      (Decode.oneOf [
        when decodeEntryContentType (is "BeganRoadNetworkImport") (Decode.succeed BeganRoadNetworkImport)
        , when decodeEntryContentType (is "CompletedRoadNetworkImport") (Decode.succeed CompletedRoadNetworkImport)
        , when decodeEntryContentType (is "RoadNetworkChangesArchiveAccepted") (Decode.succeed CompletedRoadNetworkImport)
        , when decodeEntryContentType (is "RoadNetworkChangesArchiveRejected") (Decode.succeed CompletedRoadNetworkImport)
        , when decodeEntryContentType (is "RoadNetworkChangesArchiveUploaded") (Decode.succeed CompletedRoadNetworkImport)
      ])

init : String -> ( Model, Cmd Msg )
init url =
    ( { header = Header.init |> Header.activityBecameActive
      , activity =
            { title = "Register dump"
            , url = String.concat [ url, "/v1/activity" ]
            , entries =
                [ { id = "4"
                  , title = "Oplading werd aanvaard"
                  , day = "05"
                  , month = "mei"
                  , expanded = False
                  , disabled = False
                  , detail =
                        RoadNetworkChangesArchiveRejected
                            { archive = ""
                            , warnings = []
                            , errors = []
                            }
                  }
                , { id = "3"
                  , title = "Oplading ontvangen"
                  , day = "05"
                  , month = "mei"
                  , expanded = False
                  , disabled = False
                  , detail = RoadNetworkChangesArchiveUploaded { archive = "" }
                  }
                , { id = "2"
                  , title = "Oplading werd niet aanvaard"
                  , day = "02"
                  , month = "mei"
                  , expanded = False
                  , disabled = False
                  , detail =
                        RoadNetworkChangesArchiveRejected
                            { archive = ""
                            , warnings = []
                            , errors =
                                [ "Het vereiste bestand WEGSEGMENT_ALL.SHP ontbreekt."
                                , "Het vereiste bestand WEGSEGMENT_ALL.DBF ontbreekt."
                                , "Het bestand WEGKNOOP_ALL.SHP bevat een record (325) dat geen punt is maar een polygoon."
                                ]
                            }
                  }
                , { id = "1"
                  , title = "Oplading ontvangen"
                  , day = "02"
                  , month = "mei"
                  , expanded = False
                  , disabled = False
                  , detail = RoadNetworkChangesArchiveUploaded { archive = "" }
                  }
                ]
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
    = ToggleExpandEntry String
    | GotAlertMsg AlertMsg
    | Tick Posix
    | GotActivity (Result Http.Error String)


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        ToggleExpandEntry toggleMsg ->
            ( model, Cmd.none )

        GotAlertMsg alertMsg ->
            case alertMsg of
                CloseAlert ->
                    ( { model | alert = hideAlert model.alert }
                    , Cmd.none
                    )
        Tick time ->
            ( model
            , Http.get { url = model.activity.url
                         , expect = Http.expectJson GotActivity
                         })

onClickNoBubble : msg -> Html.Attribute msg
onClickNoBubble message =
    Html.Events.custom "click" (Decode.succeed { message = message, stopPropagation = True, preventDefault = True })

viewActivityEntryContent : ActivityListEntryContent -> Html Msg
viewActivityEntryContent content =
    case content of
        BeganRoadNetworkImport ->
                    div [ class "step__content" ]
                        [ text "Archief: "
                        , a [ href "", class "link--icon link--icon--inline" ]
                            [ i [ class "vi vi-paperclip", ariaHidden True ]
                                []
                            , text "archief.zip"
                            ]
                        ]
        CompletedRoadNetworkImport ->
                    div [ class "step__content" ]
                        [ text "Archief: "
                        , a [ href "", class "link--icon link--icon--inline" ]
                            [ i [ class "vi vi-paperclip", ariaHidden True ]
                                []
                            , text "archief.zip"
                            ]
                        ]
        RoadNetworkChangesArchiveUploaded _ ->
            div [ class "step__content" ]
                [ text "Archief: "
                , a [ href "", class "link--icon link--icon--inline" ]
                    [ i [ class "vi vi-paperclip", ariaHidden True ]
                        []
                    , text "archief.zip"
                    ]
                ]

        RoadNetworkChangesArchiveRejected rejected ->
            div [ class "step__content" ]
                [ ul []
                    (List.map (\error -> li [] [ text error ]) rejected.errors)
                , ul []
                    (List.map (\warning -> li [] [ text warning ]) rejected.warnings)
                , br [] []
                , text "Archief: "
                , a [ href "", class "link--icon link--icon--inline" ]
                    [ i [ class "vi vi-paperclip", ariaHidden True ]
                        []
                    , text "archief.zip"
                    ]
                ]

        RoadNetworkChangesArchiveAccepted accepted ->
            div [ class "step__content" ]
                [ ul []
                    (List.map (\warning -> li [] [ text warning ]) accepted.warnings)
                ]


viewActivityEntry : ActivityListEntry -> Html Msg
viewActivityEntry entry =
    if entry.disabled then
        li
            [ classList [ ( "step", True ), ( "step--disabled", True ), ( "js-accordion", True ) ] ]
            [ div [ class "step__icon" ]
                [ text entry.day
                , span [ class "step__icon__sub" ] [ text entry.month ]
                ]
            , div [ class "step__wrapper" ]
                [ div [ class "step__header" ]
                    [ div [ class "step__header__titles" ]
                        [ h3 [ class "step__title" ]
                            [ text entry.title ]
                        ]
                    , div [ class "step__header__info" ]
                        [ i [ class "step__accordion-toggle" ]
                            []
                        ]
                    ]
                ]
            ]

    else
        li
            [ classList [ ( "step", True ), ( "js-accordion", True ) ] ]
            [ div [ class "step__icon" ]
                [ text entry.day
                , span [ class "step__icon__sub" ] [ text entry.month ]
                ]
            , div [ class "step__wrapper" ]
                [ a [ href "#", class "step__header js-accordion__toggle" ]
                    [ div [ class "step__header__titles" ]
                        [ h3 [ class "step__title" ]
                            [ text entry.title ]
                        ]
                    , div [ class "step__header__info" ]
                        [ i [ class "vi vi-paperclip vi-u-s" ]
                            []
                        , i [ class "step__accordion-toggle" ]
                            []
                        ]
                    ]
                , div
                    [ class "step__content-wrapper" ]
                    [ viewActivityEntryContent entry.detail
                    ]
                ]
            ]


viewActivityTitle : ActivityModel -> Html Msg
viewActivityTitle model =
        section [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                [ div []
                    [ h1 [ class "h2 cta-title__title" ]
                        [ text "Activiteit" ]
                    ]
                ]
            ]

viewActivity : ActivityModel -> Html Msg
viewActivity model =
        section [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                [ ul
                    [ class "steps steps--timeline" ]
                    (List.map viewActivityEntry model.entries)
                ]
            ]

viewMain : Model -> Html Msg
viewMain model =
    main_ [ id "main" ]
        [
          viewAlert model.alert |> Html.map GotAlertMsg
        , viewActivityTitle model.activity
        , viewActivity model.activity
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
    every (60 * 1000) Tick

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


main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type ActivityListEntryDetail
    = RoadNetworkChangesArchiveAccepted { archive : String, warnings : List String }
    | RoadNetworkChangesArchiveRejected { archive : String, errors : List String, warnings : List String }
    | RoadNetworkChangesArchiveUploaded { archive : String }


type alias ActivityListEntry =
    { id : String, title : String, day : String, month : String, expanded : Bool, disabled : Bool, detail : ActivityListEntryDetail }


type alias ActivityModel =
    { title : String, url : String, entries : List ActivityListEntry }


type alias Model =
    { header : HeaderModel, activity : ActivityModel, alert : AlertModel }


init : String -> ( Model, Cmd Msg )
init url =
    ( { header = Header.init |> Header.activityBecameActive
      , activity =
            { title = "Register dump"
            , url = String.concat [ url, "/v1/activity" ]
            , entries =
                [ { id = "2"
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


viewActivityEntryDetail : ActivityListEntryDetail -> Html Msg
viewActivityEntryDetail detail =
    case detail of
        RoadNetworkChangesArchiveUploaded uploaded ->
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
                    [ viewActivityEntryDetail entry.detail
                    ]
                ]
            ]


viewActivity : ActivityModel -> Html Msg
viewActivity model =
    main_ [ id "main" ]
        [ section [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                [ div []
                    [ h1 [ class "h2 cta-title__title" ]
                        [ text "Activiteit" ]
                    ]
                ]
            ]
        , section [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                [ ul
                    [ class "steps steps--timeline" ]
                    (List.map viewActivityEntry model.entries)
                ]
            ]
        ]


view : Model -> Html Msg
view model =
    div [ class "page" ]
        [ Header.viewBanner ()
        , Header.viewHeader model.header
        , viewAlert model.alert |> Html.map GotAlertMsg
        , viewActivity model.activity
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub Msg
subscriptions _ =
    Sub.none

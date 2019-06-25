module Activity exposing (Msg(..), init, main, subscriptions, update, view)

import Alert exposing (AlertKind(..), AlertModel, AlertMsg(..), hideAlert, showError, viewAlert)
import Browser
import Footer
import Header exposing (HeaderAction, HeaderModel, TabAction)
import Html exposing (Html, a, br, div, h1, h2, h3, i, li, main_, section, span, text, ul)
import Html.Attributes exposing (class, classList, href, id, style)
import Html.Attributes.Aria exposing (ariaHidden)
import Html.Events
import Http
import Json.Decode as Decode
import Json.Decode.Extra exposing (when)
import Time exposing (Posix, every)
import Iso8601


main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias FileProblems =
    { file : String, problems : List String }


type ChangeFeedEntryContent
    = BeganRoadNetworkImport
    | CompletedRoadNetworkImport
    | RoadNetworkChangesArchiveAccepted { archive : String, problems : List FileProblems }
    | RoadNetworkChangesArchiveRejected { archive : String, problems : List FileProblems }
    | RoadNetworkChangesArchiveUploaded { archive : String }


type alias ChangeFeedEntry =
    { id : String, title : String, day : String, month: String, timeOfDay: String, content : ChangeFeedEntryContent }


type alias ActivityModel =
    { url : String, entries : List ChangeFeedEntry }


type alias Model =
    { header : HeaderModel, activity : ActivityModel, alert : AlertModel }


decodeEntryContentType : Decode.Decoder String
decodeEntryContentType =
    Decode.field "type" Decode.string


is : a -> a -> Bool
is a b =
    a == b


decodeFileProblem : Decode.Decoder FileProblems
decodeFileProblem =
    Decode.map2 FileProblems
        (Decode.field "file" Decode.string)
        (Decode.field "problems" (Decode.list Decode.string))


decodeRoadNetworkChangesArchiveUploaded : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveUploaded =
    Decode.field "content"
        (Decode.map
            (\archive -> RoadNetworkChangesArchiveUploaded { archive = archive })
            (Decode.field "archiveId" Decode.string)
        )


decodeRoadNetworkChangesArchiveAccepted : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveAccepted =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesArchiveAccepted { archive = archive, problems = problems })
            (Decode.field "archiveId" Decode.string)
            (Decode.field "files" (Decode.list decodeFileProblem))
        )


decodeRoadNetworkChangesArchiveRejected : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveRejected =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesArchiveRejected { archive = archive, problems = problems })
            (Decode.field "archiveId" Decode.string)
            (Decode.field "files" (Decode.list decodeFileProblem))
        )


decodeEntry : Decode.Decoder ChangeFeedEntry
decodeEntry =
    Decode.map6 ChangeFeedEntry
        (Decode.field "id" Decode.int |> Decode.map String.fromInt)
        (Decode.field "title" Decode.string)
        (Decode.field "day" Decode.string)
        (Decode.field "month" Decode.string)
        (Decode.field "timeOfDay" Decode.string)
        (Decode.oneOf
            [ when decodeEntryContentType (is "BeganRoadNetworkImport") (Decode.succeed BeganRoadNetworkImport)
            , when decodeEntryContentType (is "CompletedRoadNetworkImport") (Decode.succeed CompletedRoadNetworkImport)
            , when decodeEntryContentType (is "RoadNetworkChangesArchiveAccepted") decodeRoadNetworkChangesArchiveAccepted
            , when decodeEntryContentType (is "RoadNetworkChangesArchiveRejected") decodeRoadNetworkChangesArchiveRejected
            , when decodeEntryContentType (is "RoadNetworkChangesArchiveUploaded") decodeRoadNetworkChangesArchiveUploaded
            ]
        )


decodeResponse : Decode.Decoder (List ChangeFeedEntry)
decodeResponse =
    Decode.field "entries" (Decode.list decodeEntry)


init : String -> ( Model, Cmd Msg )
init url =
    ( { header = Header.init |> Header.activityBecameActive
      , activity =
            { url = String.concat [ url, "/v1/changefeed" ]
            , entries = []
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
        { url = String.concat [ url, "/v1/changefeed" ]
        , expect = Http.expectJson GotActivity decodeResponse
        }
    )


type Msg
    = ToggleExpandEntry String
    | GotAlertMsg AlertMsg
    | Tick Posix
    | GotActivity (Result Http.Error (List ChangeFeedEntry))


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
            , Http.get
                { url = model.activity.url
                , expect = Http.expectJson GotActivity decodeResponse
                }
            )

        GotActivity result ->
            case result of
                Ok entries ->
                    let
                        oldActivity =
                            model.activity

                        newActivity =
                            { oldActivity | entries = entries }
                    in
                    ( { model | activity = newActivity }, Cmd.none )

                Err error ->
                    case error of
                        Http.BadUrl _ ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het opvragen van de activiteiten - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het opvragen van de activiteiten - de operatie nam teveel tijd in beslag." }
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het opvragen van de activiteiten - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                503 ->
                                    ( { model | alert = showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
                                    , Cmd.none
                                    )

                                _ ->
                                    ( { model | alert = showError model.alert "Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody _ ->
                            ( { model | alert = showError model.alert "Er was een probleem bij het interpreteren van de activiteiten - dit kan duiden op een probleem met de website." }
                            , Cmd.none
                            )


onClickNoBubble : msg -> Html.Attribute msg
onClickNoBubble message =
    Html.Events.custom "click" (Decode.succeed { message = message, stopPropagation = True, preventDefault = True })


viewActivityEntryContent : ChangeFeedEntryContent -> Html Msg
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
                    (List.map (\problem -> li [] [ text problem.file ]) rejected.problems)
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
                    (List.map (\problem -> li [] [ text problem.file ]) accepted.problems)
                ]


viewActivityEntry : ChangeFeedEntry -> Html Msg
viewActivityEntry entry =
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
                  [ viewActivityEntryContent entry.content
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
        [ viewAlert model.alert |> Html.map GotAlertMsg
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
    every 5000 Tick

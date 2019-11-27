module Activity exposing (Msg(..), init, main, subscriptions, update, view)

import Alert as Alert
import Browser
import FontAwesome as FA
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, a, br, div, h1, h3, i, li, main_, section, span, text, ul)
import Html.Attributes exposing (class, classList, href, id, style)
import Html.Attributes.Aria exposing (ariaHidden)
import Http
import Json.Decode as Decode
import Json.Decode.Extra exposing (when)
import Time exposing (Posix, every)


main : Program String Model Msg
main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type ProblemSeverity
    = Warning
    | Error


type alias FileProblem =
    { problem : String
    , severity : ProblemSeverity
    }


type alias FileProblems =
    { file : String
    , problems : List FileProblem
    }


type alias ChangeProblem =
    { problem : String
    , severity : ProblemSeverity
    }


type alias ChangeProblems =
    { change : String
    , problems : List ChangeProblem
    }


type alias Archive =
    { id : String
    , available : Bool
    , filename : String
    }


type ChangeFeedEntryContent
    = BeganRoadNetworkImport
    | CompletedRoadNetworkImport
    | RoadNetworkChangesArchiveAccepted { archive : Archive, problems : List FileProblems }
    | RoadNetworkChangesArchiveRejected { archive : Archive, problems : List FileProblems }
    | RoadNetworkChangesArchiveUploaded { archive : Archive }
    | RoadNetworkChangesBasedOnArchiveRejected { archive : Archive, problems : List ChangeProblems }


type alias ChangeFeedEntry =
    { id : String
    , title : String
    , day : String
    , month : String
    , timeOfDay : String
    , content : ChangeFeedEntryContent
    }


type alias ActivityModel =
    { url : String
    , entries : List ChangeFeedEntry
    }


type alias Model =
    { header : HeaderModel
    , activity : ActivityModel
    , alert : Alert.AlertModel
    }


decodeEntryContentType : Decode.Decoder String
decodeEntryContentType =
    Decode.field "type" Decode.string


is : a -> a -> Bool
is a b =
    a == b


decodeFileProblem : Decode.Decoder FileProblem
decodeFileProblem =
    Decode.map2 FileProblem
        (Decode.field "text" Decode.string)
        (Decode.field "severity" Decode.string
            |> Decode.andThen
                (\value ->
                    case value of
                        "Warning" ->
                            Decode.succeed Warning

                        "Error" ->
                            Decode.succeed Error

                        other ->
                            Decode.fail <| "Unknown severity: " ++ other
                )
        )


decodeChangeProblem : Decode.Decoder ChangeProblem
decodeChangeProblem =
    Decode.map2 ChangeProblem
        (Decode.field "text" Decode.string)
        (Decode.field "severity" Decode.string
            |> Decode.andThen
                (\value ->
                    case value of
                        "Warning" ->
                            Decode.succeed Warning

                        "Error" ->
                            Decode.succeed Error

                        other ->
                            Decode.fail <| "Unknown severity: " ++ other
                )
        )


decodeFileProblems : Decode.Decoder FileProblems
decodeFileProblems =
    Decode.map2 FileProblems
        (Decode.field "file" Decode.string)
        (Decode.field "problems" (Decode.list decodeFileProblem))


decodeChangeProblems : Decode.Decoder ChangeProblems
decodeChangeProblems =
    Decode.map2 ChangeProblems
        (Decode.field "change" Decode.string)
        (Decode.field "problems" (Decode.list decodeChangeProblem))


decodeArchive : Decode.Decoder Archive
decodeArchive =
    Decode.map3 Archive
        (Decode.field "id" Decode.string)
        (Decode.field "available" Decode.bool)
        (Decode.field "filename" Decode.string)


decodeRoadNetworkChangesArchiveUploaded : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveUploaded =
    Decode.field "content"
        (Decode.map
            (\archive -> RoadNetworkChangesArchiveUploaded { archive = archive })
            (Decode.field "archive" decodeArchive)
        )


decodeRoadNetworkChangesArchiveAccepted : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveAccepted =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesArchiveAccepted { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "files" (Decode.list decodeFileProblems))
        )


decodeRoadNetworkChangesArchiveRejected : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveRejected =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesArchiveRejected { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "files" (Decode.list decodeFileProblems))
        )


decodeRoadNetworkChangesBasedOnArchiveRejected : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesBasedOnArchiveRejected =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesBasedOnArchiveRejected { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "changes" (Decode.list decodeChangeProblems))
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
            , when decodeEntryContentType (is "RoadNetworkChangesBasedOnArchiveRejected") decodeRoadNetworkChangesBasedOnArchiveRejected
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
            , kind = Alert.Error
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
    | GotAlertMsg Alert.AlertMsg
    | Tick Posix
    | GotActivity (Result Http.Error (List ChangeFeedEntry))


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        ToggleExpandEntry _ ->
            ( model, Cmd.none )

        GotAlertMsg alertMsg ->
            case alertMsg of
                Alert.CloseAlert ->
                    ( { model | alert = Alert.hideAlert model.alert }
                    , Cmd.none
                    )

        Tick _ ->
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
                            ( { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - de url blijkt foutief te zijn." }
                            , Cmd.none
                            )

                        Http.Timeout ->
                            ( { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - de operatie nam teveel tijd in beslag." }
                            , Cmd.none
                            )

                        Http.NetworkError ->
                            ( { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - een netwerk fout ligt aan de basis." }
                            , Cmd.none
                            )

                        Http.BadStatus statusCode ->
                            case statusCode of
                                503 ->
                                    ( { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
                                    , Cmd.none
                                    )

                                _ ->
                                    ( { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website." }
                                    , Cmd.none
                                    )

                        Http.BadBody bodyProblem ->
                            ( { model | alert = Alert.showError model.alert ("Er was een probleem bij het interpreteren van de activiteiten - dit kan duiden op een probleem met de website." ++ bodyProblem) }
                            , Cmd.none
                            )


viewActivityEntryContent : ChangeFeedEntryContent -> Html Msg
viewActivityEntryContent content =
    case content of
        BeganRoadNetworkImport ->
            div [ class "step__content" ]
                []

        CompletedRoadNetworkImport ->
            div [ class "step__content" ]
                []

        RoadNetworkChangesArchiveUploaded uploaded ->
            div [ class "step__content" ]
                [ text "Archief: "
                , a [ href "", class "link--icon link--icon--inline" ]
                    [ i [ class "vi vi-paperclip", ariaHidden True ]
                        []
                    , text uploaded.archive.filename
                    ]
                ]

        RoadNetworkChangesArchiveRejected rejected ->
            div [ class "step__content" ]
                [ ul
                    []
                    (List.map
                        (\fileProblems ->
                            li
                                [ style "padding-top" "5px" ]
                                [ span [ style "font-weight" "bold" ] [ text fileProblems.file ]
                                , ul
                                    []
                                    (List.map
                                        (\problem ->
                                            case problem.severity of
                                                Warning ->
                                                    li
                                                        []
                                                        [ span [ style "color" "#ffc515" ] [ FA.icon FA.exclamationTriangle ]
                                                        , text "\u{00A0}"
                                                        , text problem.problem
                                                        ]

                                                Error ->
                                                    li
                                                        []
                                                        [ span [ style "color" "#db3434" ] [ FA.icon FA.exclamationTriangle ]
                                                        , text "\u{00A0}"
                                                        , text problem.problem
                                                        ]
                                        )
                                        fileProblems.problems
                                    )
                                ]
                        )
                        rejected.problems
                    )
                , br [] []
                , text "Archief: "
                , a [ href "", class "link--icon link--icon--inline" ]
                    [ i [ class "vi vi-paperclip", ariaHidden True ]
                        []
                    , text rejected.archive.filename
                    ]
                ]

        RoadNetworkChangesArchiveAccepted accepted ->
            div [ class "step__content" ]
                [ ul []
                    (List.map (\problem -> li [] [ text problem.file ]) accepted.problems)
                ]

        RoadNetworkChangesBasedOnArchiveRejected rejected ->
            div [ class "step__content" ]
                [ ul
                    []
                    (List.map
                        (\changeProblems ->
                            li
                                [ style "padding-top" "5px" ]
                                [ span [ style "font-weight" "bold" ] [ text changeProblems.change ]
                                , ul
                                    []
                                    (List.map
                                        (\problem ->
                                            case problem.severity of
                                                Warning ->
                                                    li
                                                        []
                                                        [ span [ style "color" "#ffc515" ] [ FA.icon FA.exclamationTriangle ]
                                                        , text "\u{00A0}"
                                                        , text problem.problem
                                                        ]

                                                Error ->
                                                    li
                                                        []
                                                        [ span [ style "color" "#db3434" ] [ FA.icon FA.exclamationTriangle ]
                                                        , text "\u{00A0}"
                                                        , text problem.problem
                                                        ]
                                        )
                                        changeProblems.problems
                                    )
                                ]
                        )
                        rejected.problems
                    )
                , br [] []
                , text "Archief: "
                , a [ href "", class "link--icon link--icon--inline" ]
                    [ i [ class "vi vi-paperclip", ariaHidden True ]
                        []
                    , text rejected.archive.filename
                    ]
                ]


viewActivityEntry : ChangeFeedEntry -> Html Msg
viewActivityEntry entry =
    li
        [ classList [ ( "step", True ), ( "js-accordion", True ) ] ]
        [ div [ class "step__icon" ]
            [ text entry.day
            , span [ class "step__icon__sub" ] [ text entry.month ]
            , span [ class "step__icon__sub" ] [ text entry.timeOfDay ]
            ]
        , div [ class "step__wrapper" ]
            [ a [ href "#", class "step__header js-accordion__toggle" ]
                [ div [ class "step__header__titles" ]
                    [ h3 [ class "step__title" ]
                        [ text entry.title ]
                    ]
                , case entry.content of
                    BeganRoadNetworkImport ->
                        div [ class "step__header__info" ]
                            []

                    CompletedRoadNetworkImport ->
                        div [ class "step__header__info" ]
                            []

                    RoadNetworkChangesArchiveAccepted _ ->
                        div [ class "step__header__info" ]
                            [ i [ class "vi vi-paperclip vi-u-s" ]
                                []
                            , i [ class "step__accordion-toggle" ]
                                []
                            ]

                    RoadNetworkChangesArchiveRejected _ ->
                        div [ class "step__header__info" ]
                            [ i [ class "vi vi-paperclip vi-u-s" ]
                                []
                            , i [ class "step__accordion-toggle" ]
                                []
                            ]

                    RoadNetworkChangesArchiveUploaded _ ->
                        div [ class "step__header__info" ]
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
viewActivityTitle _ =
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
        [ Alert.viewAlert model.alert |> Html.map GotAlertMsg
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

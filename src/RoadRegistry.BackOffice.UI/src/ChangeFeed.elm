module ChangeFeed exposing (..)

import Alert
import FontAwesome as FA
import Html exposing (Html, a, br, div, h1, h3, i, li, section, span, text, ul)
import Html.Attributes exposing (class, classList, href, style)
import Html.Attributes.Aria exposing (ariaHidden)
import Http
import Json.Decode as Decode
import Json.Decode.Extra exposing (when)

-- models

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
    | RoadNetworkChangesBasedOnArchiveAccepted { archive : Archive, problems : List ChangeProblems }
    | RoadNetworkChangesBasedOnArchiveRejected { archive : Archive, problems : List ChangeProblems }

type alias ChangeFeedEntriesResponse =
    { entries : List ChangeFeedEntry }

type alias ChangeFeedEntryContentResponse =
    { id : Int
    , content : ChangeFeedEntryContent }

type alias ChangeFeedEntry =
    { id : Int
    , title : String
    , day : String
    , month : String
    , timeOfDay : String
    , contentLink : String
    , content : Maybe ChangeFeedEntryContent
    }

type alias Model =
    { entries : List ChangeFeedEntry
    , feedUrl : String
    , archiveUrl : String
    , maxEntryCount: Int
    , alert : Alert.Model
    }

-- messaging

init : String -> ( Model, Cmd Message )
init url =
    let
      feedUrl = if String.endsWith "/" url then String.concat [ url, "v1/changefeed" ] else String.concat [ url, "/v1/changefeed" ]
      archiveUrl = if String.endsWith "/" url then String.concat [ url, "v1/upload/" ] else String.concat [ url, "/v1/upload/" ]

      model =
        { entries = []
        , feedUrl = feedUrl
        , archiveUrl = archiveUrl
        , maxEntryCount = 50
        , alert = Alert.init()
        }
    in
      ( model
        , Http.get
          { url = buildHeadUrl model
          , expect = Http.expectJson GotHead decodeChangeFeedEntriesResponse
          }
      )

getNextEntry : Model -> Maybe Int
getNextEntry model =
  List.maximum (List.map .id model.entries)

getPreviousEntry : Model -> Maybe Int
getPreviousEntry model =
  List.minimum (List.map .id model.entries)

type Message =
    GetHead
    | GetNext Int
    | GetPrevious Int
    | GetEntryContent Int
    | GotHead (Result Http.Error ChangeFeedEntriesResponse)
    | GotNext (Result Http.Error ChangeFeedEntriesResponse)
    | GotPrevious (Result Http.Error ChangeFeedEntriesResponse)
    | GotEntryContent (Result Http.Error ChangeFeedEntryContentResponse)
    | GotAlertMessage Alert.Message

buildHeadUrl: Model -> String
buildHeadUrl model = model.feedUrl ++ "/head?maxEntryCount=" ++ String.fromInt model.maxEntryCount

buildNextUrl: Int -> Model -> String
buildNextUrl entry model = model.feedUrl ++ "/next?afterEntry=" ++ String.fromInt entry  ++ "&maxEntryCount=" ++ String.fromInt model.maxEntryCount

buildPreviousUrl: Int -> Model -> String
buildPreviousUrl entry model = model.feedUrl ++ "/previous?beforeEntry=" ++ String.fromInt entry  ++ "&maxEntryCount=" ++ String.fromInt model.maxEntryCount

buildEntryContentUrl: Int -> Model -> String
buildEntryContentUrl entry model = model.feedUrl ++ "/entry/" ++ String.fromInt entry  ++ "/content"

type alias BadStatusTranslator = Model -> Int -> Model

translateBadHeadStatus: BadStatusTranslator
translateBadHeadStatus model statusCode =
  case statusCode of
    400 ->
      { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - niet alle vereiste parameters werden doorgegeven." }
    503 ->
      { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
    _ ->
      { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }

translateBadNextStatus: BadStatusTranslator
translateBadNextStatus model statusCode =
  case statusCode of
    400 ->
      { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - niet alle vereiste parameters werden doorgegeven." }
    503 ->
      { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
    _ ->
      { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }

translateBadPreviousStatus: BadStatusTranslator
translateBadPreviousStatus model statusCode =
  case statusCode of
    400 ->
      { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - niet alle vereiste parameters werden doorgegeven." }
    503 ->
      { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
    _ ->
      { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }

translateBadEntryContentStatus: BadStatusTranslator
translateBadEntryContentStatus model statusCode =
  case statusCode of
    404 ->
      { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteit - kon de activiteit niet vinden." }
    503 ->
      { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }
    _ ->
      { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteit - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }

translateHttpError: Model -> Http.Error -> BadStatusTranslator -> Model
translateHttpError model error badStatusTranslator =
  case error of
    Http.BadUrl _ ->
        { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - de url blijkt foutief te zijn." }

    Http.Timeout ->
        { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - de operatie nam teveel tijd in beslag." }

    Http.NetworkError ->
        { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - een netwerk fout ligt aan de basis." }

    Http.BadStatus statusCode ->
        badStatusTranslator model statusCode

    Http.BadBody bodyProblem ->
        { model | alert = Alert.showError model.alert ("Er was een probleem bij het interpreteren van de activiteiten - dit kan duiden op een probleem met de website (BodyProblem=" ++ bodyProblem ++ ").") }

descending : ChangeFeedEntry -> ChangeFeedEntry -> Order
descending left right =
    case compare left.id right.id of
      LT -> GT
      EQ -> EQ
      GT -> LT

update: Message -> Model -> (Model, Cmd Message)
update message model =
  case message of
    GetHead ->
      (model
      , Http.get { url = buildHeadUrl model, expect = Http.expectJson GotHead decodeChangeFeedEntriesResponse })

    GetNext afterEntry ->
      (model
      , Http.get { url = buildNextUrl afterEntry model, expect = Http.expectJson GotNext decodeChangeFeedEntriesResponse })


    GetPrevious beforeEntry ->
      (model
      , Http.get { url = buildPreviousUrl beforeEntry model, expect = Http.expectJson GotPrevious decodeChangeFeedEntriesResponse })


    GetEntryContent entry ->
      (model
      , Http.get { url = buildEntryContentUrl entry model, expect = Http.expectJson GotEntryContent decodeChangeFeedEntryContentResponse })


    GotHead result ->
      case result of
        Ok response ->
          ( { model | entries = List.sortWith descending response.entries }
          , Cmd.none )
        Err error ->
          ( translateHttpError model error translateBadHeadStatus, Cmd.none )

    GotNext result ->
      case result of
        Ok response ->
          ( { model | entries = List.sortWith descending (List.append response.entries model.entries) }
          , Cmd.none )
        Err error ->
          ( translateHttpError model error translateBadNextStatus, Cmd.none )

    GotPrevious result ->
      case result of
        Ok response ->
          ( { model | entries = List.sortWith descending (List.append model.entries response.entries) }
          , Cmd.none )
        Err error ->
          ( translateHttpError model error translateBadPreviousStatus, Cmd.none )

    GotEntryContent result ->
      case result of
        Ok response ->
          let
            entries =
              List.map (\entry ->
                if entry.id == response.id then
                  { entry | content = Just response.content }
                else
                  entry
              ) model.entries
          in
            ( { model | entries = entries }
            , Cmd.none )
        Err error ->
          ( translateHttpError model error translateBadPreviousStatus, Cmd.none )

    GotAlertMessage alertMessage ->
      let
        (alertModel, alertCommand) = Alert.update alertMessage model.alert
      in
        ( { model | alert = alertModel }
        , Cmd.map GotAlertMessage alertCommand )


-- view

viewArchiveLinkContent: String -> Archive -> Html Message
viewArchiveLinkContent url archive =
    if not archive.available then
      text archive.filename
    else
      a [ href (String.concat [ url, archive.id ]), class "link--icon link--icon--inline" ]
      [ i [ class "vi vi-paperclip", ariaHidden True ]
          []
      , text archive.filename
      ]

viewChangeFeedEntryContent : String -> ChangeFeedEntryContent -> Html Message
viewChangeFeedEntryContent url content =
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
                , viewArchiveLinkContent url uploaded.archive
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
                , viewArchiveLinkContent url rejected.archive
                ]

        RoadNetworkChangesArchiveAccepted accepted ->
            div [ class "step__content" ]
                [ ul []
                    (List.map (\problem -> li [] [ text problem.file ]) accepted.problems)
                ]

        RoadNetworkChangesBasedOnArchiveAccepted accepted ->
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
                                accepted.problems
                            )
                        , br [] []
                        , text "Archief: "
                        , viewArchiveLinkContent url accepted.archive
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
                , viewArchiveLinkContent url rejected.archive
                ]

viewChangeFeedEntry : String -> ChangeFeedEntry -> Html Message
viewChangeFeedEntry url entry =
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
                    Just content ->
                      case content of
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

                        RoadNetworkChangesBasedOnArchiveAccepted _ ->
                            div [ class "step__header__info" ]
                                [ i [ class "vi vi-paperclip vi-u-s" ]
                                    []
                                , i [ class "step__accordion-toggle" ]
                                    []
                                ]

                        RoadNetworkChangesBasedOnArchiveRejected _ ->
                            div [ class "step__header__info" ]
                                [ i [ class "vi vi-paperclip vi-u-s" ]
                                    []
                                , i [ class "step__accordion-toggle" ]
                                    []
                                ]
                    Nothing ->
                      text ""
                ]
            , div
                [ class "step__content-wrapper" ]
                [ case entry.content of
                    Just content -> viewChangeFeedEntryContent url content
                    Nothing -> text ""
                ]
            ]
        ]


viewChangeFeedTitle : Model -> Html Message
viewChangeFeedTitle _ =
    section [ class "region" ]
        [ div
            [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
            [ div []
                [ h1 [ class "h2 cta-title__title" ]
                    [ text "Activiteit" ]
                ]
            ]
        ]


view : Model -> Html Message
view model =
    section [ class "region" ]
        [ div
            [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
            [ ul
                [ class "steps steps--timeline" ]
                (List.map (viewChangeFeedEntry model.archiveUrl) model.entries)
            ]
        ]

-- decoders

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

decodeRoadNetworkChangesBasedOnArchiveAccepted : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesBasedOnArchiveAccepted =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesBasedOnArchiveAccepted { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "changes" (Decode.list decodeChangeProblems))
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
    Decode.map7 ChangeFeedEntry
        (Decode.field "id" Decode.int)
        (Decode.field "title" Decode.string)
        (Decode.field "day" Decode.string)
        (Decode.field "month" Decode.string)
        (Decode.field "timeOfDay" Decode.string)
        (Decode.field "contentLink" Decode.string)
        (Decode.succeed (Nothing))


decodeChangeFeedEntriesResponse : Decode.Decoder ChangeFeedEntriesResponse
decodeChangeFeedEntriesResponse =
    Decode.map ChangeFeedEntriesResponse
      (Decode.field "entries" (Decode.list decodeEntry))

decodeChangeFeedEntryContentResponse : Decode.Decoder ChangeFeedEntryContentResponse
decodeChangeFeedEntryContentResponse =
    Decode.map2 ChangeFeedEntryContentResponse
      (Decode.field "id" Decode.int)
      (Decode.oneOf
          [ when decodeEntryContentType (is "BeganRoadNetworkImport") (Decode.succeed BeganRoadNetworkImport)
          , when decodeEntryContentType (is "CompletedRoadNetworkImport") (Decode.succeed CompletedRoadNetworkImport)
          , when decodeEntryContentType (is "RoadNetworkChangesArchiveAccepted") decodeRoadNetworkChangesArchiveAccepted
          , when decodeEntryContentType (is "RoadNetworkChangesArchiveRejected") decodeRoadNetworkChangesArchiveRejected
          , when decodeEntryContentType (is "RoadNetworkChangesArchiveUploaded") decodeRoadNetworkChangesArchiveUploaded
          , when decodeEntryContentType (is "RoadNetworkChangesAccepted") decodeRoadNetworkChangesBasedOnArchiveAccepted
          , when decodeEntryContentType (is "RoadNetworkChangesRejected") decodeRoadNetworkChangesBasedOnArchiveRejected
          ]
      )


module ChangeFeed exposing (..)

import Alert
import FontAwesome as FA
import Html exposing (Html, a, br, div, h1, h3, i, li, section, span, text, ul)
import Html.Attributes exposing (class, classList, href, style)
import Html.Attributes.Aria exposing (ariaHidden)
import Html.Events exposing (onClick)
import Http
import Json.Decode as Decode
import Json.Decode.Extra exposing (when)
import List.Extra
import Html.Attributes exposing (disabled)


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

type alias Counters =
    { added: Int
    , modified: Int
    , removed: Int }

type alias Summary =
    { roadNodes: Counters
    , roadSegments: Counters
    , gradeSeparatedJunctions: Counters
    }

type ChangeFeedEntryContent
    = BeganRoadNetworkImport
    | CompletedRoadNetworkImport
    | RoadNetworkChangesArchiveAccepted { archive : Archive, problems : List FileProblems }
    | RoadNetworkChangesArchiveRejected { archive : Archive, problems : List FileProblems }
    | RoadNetworkChangesArchiveUploaded { archive : Archive }
    | RoadNetworkChangesBasedOnArchiveAccepted { archive : Archive, summary: Maybe Summary, problems : List ChangeProblems }
    | RoadNetworkChangesBasedOnArchiveRejected { archive : Archive, problems : List ChangeProblems }
    | RoadNetworkExtractGotRequested
    | RoadNetworkExtractDownloadBecameAvailable { archive : Archive }
    | RoadNetworkExtractChangesArchiveAccepted { archive : Archive, problems : List FileProblems }
    | RoadNetworkExtractChangesArchiveRejected { archive : Archive, problems : List FileProblems }
    | RoadNetworkExtractChangesArchiveUploaded { archive : Archive }


type alias ChangeFeedEntriesResponse =
    { entries : List ChangeFeedEntry }


type alias ChangeFeedEntryContentResponse =
    { id : Int
    , content : ChangeFeedEntryContent
    }


type ChangeFeedEntryState
    = Loaded
    | Loading
    | RequiresLoading


type alias ChangeFeedEntry =
    { id : Int
    , title : String
    , contentType : String
    , state : ChangeFeedEntryState
    , day : String
    , month : String
    , timeOfDay : String
    , content : Maybe ChangeFeedEntryContent
    }


type alias Model =
    { entries : List ChangeFeedEntry
    , loading : Bool
    , feedUrl : String
    , archiveUrl : String
    , extractUrl : String
    , maxEntryCount : Int
    , alert : Alert.Model
    , apikey: String
    }



-- messaging


init : Int -> String -> String -> String -> ( Model, Cmd Message )
init maxEntryCount url url1 apikey =
    let
        feedUrl =
            if String.endsWith "/" url then
                String.concat [ url, "v1/wegen/activiteit" ]

            else
                String.concat [ url, "/v1/wegen/activiteit" ]

        archiveUrl =
            if String.endsWith "/" url1 then
                String.concat [ url1, "v1/upload/" ]

            else
                String.concat [ url1, "/v1/upload/" ]

        extractUrl =
            if String.endsWith "/" url1 then
                String.concat [ url1, "v1/extracts/download/" ]

            else
                String.concat [ url1, "/v1/extracts/download/" ]

        model =
            { entries = []
            , loading = False
            , feedUrl = feedUrl
            , archiveUrl = archiveUrl
            , extractUrl = extractUrl
            , maxEntryCount = maxEntryCount
            , alert = Alert.init ()
            , apikey = apikey
            }
    in
    ( model
    , Http.request
        { method = "GET"
        , headers = [ Http.header "x-api-key" model.apikey ]
        , url = buildHeadUrl model
        , body = Http.emptyBody
        , expect = Http.expectJson GotHead decodeChangeFeedEntriesResponse
        , timeout = Nothing
        , tracker = Nothing
        }
    )


getNextEntry : Model -> Maybe Int
getNextEntry model =
    List.maximum (List.map .id model.entries)


getPreviousEntry : Model -> Maybe Int
getPreviousEntry model =
    List.minimum (List.map .id model.entries)


type Message
    = GetHead
    | GetNext Int
    | GetPrevious Int
    | ToggleEntryContent Int
    | GotHead (Result Http.Error ChangeFeedEntriesResponse)
    | GotNext (Result Http.Error ChangeFeedEntriesResponse)
    | GotPrevious (Result Http.Error ChangeFeedEntriesResponse)
    | GotEntryContent (Result Http.Error ChangeFeedEntryContentResponse)
    | GotAlertMessage Alert.Message


buildHeadUrl : Model -> String
buildHeadUrl model =
    model.feedUrl ++ "/begin?maxEntryCount=" ++ String.fromInt model.maxEntryCount


buildNextUrl : Int -> Model -> String
buildNextUrl entry model =
    model.feedUrl ++ "/volgende?afterEntry=" ++ String.fromInt entry ++ "&maxEntryCount=" ++ String.fromInt model.maxEntryCount


buildPreviousUrl : Int -> Model -> String
buildPreviousUrl entry model =
    model.feedUrl ++ "/vorige?beforeEntry=" ++ String.fromInt entry ++ "&maxEntryCount=" ++ String.fromInt model.maxEntryCount


buildEntryContentUrl : Int -> Model -> String
buildEntryContentUrl entry model =
    model.feedUrl ++ "/gebeurtenis/" ++ String.fromInt entry ++ "/inhoud"


type alias BadStatusTranslator =
    Model -> Int -> Model


translateBadHeadStatus : BadStatusTranslator
translateBadHeadStatus model statusCode =
    case statusCode of
        400 ->
            { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - niet alle vereiste parameters werden doorgegeven." }

        503 ->
            { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }

        _ ->
            { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }


translateBadNextStatus : BadStatusTranslator
translateBadNextStatus model statusCode =
    case statusCode of
        400 ->
            { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - niet alle vereiste parameters werden doorgegeven." }

        503 ->
            { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }

        _ ->
            { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }


translateBadPreviousStatus : BadStatusTranslator
translateBadPreviousStatus model statusCode =
    case statusCode of
        400 ->
            { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteiten - niet alle vereiste parameters werden doorgegeven." }

        503 ->
            { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }

        _ ->
            { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteiten - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }


translateBadEntryContentStatus : BadStatusTranslator
translateBadEntryContentStatus model statusCode =
    case statusCode of
        404 ->
            { model | alert = Alert.showError model.alert "Er was een probleem bij het opvragen van de activiteit - kon de activiteit niet vinden." }

        503 ->
            { model | alert = Alert.showError model.alert "Activiteiten opvragen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw." }

        _ ->
            { model | alert = Alert.showError model.alert ("Er was een probleem bij het opvragen van de activiteit - dit kan duiden op een probleem met de website (StatusCode=" ++ String.fromInt statusCode ++ ").") }


translateHttpError : Model -> Http.Error -> BadStatusTranslator -> Model
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
        LT ->
            GT

        EQ ->
            EQ

        GT ->
            LT


update : Message -> Model -> ( Model, Cmd Message )
update message model =
    case message of
        GetHead ->
            ( model
            , Http.request
              { method = "GET"
              , headers = [ Http.header "x-api-key" model.apikey ]
              , url = buildHeadUrl model
              , body = Http.emptyBody
              , expect = Http.expectJson GotHead decodeChangeFeedEntriesResponse
              , timeout = Nothing
              , tracker = Nothing
              }
            )

        GetNext afterEntry ->
            ( model
            , Http.request
              { method = "GET"
              , headers = [ Http.header "x-api-key" model.apikey ]
              , url = buildNextUrl afterEntry model
              , body = Http.emptyBody
              , expect = Http.expectJson GotNext decodeChangeFeedEntriesResponse
              , timeout = Nothing
              , tracker = Nothing
              }
            )

        GetPrevious beforeEntry ->
            ( { model | loading = True }
            , Http.request
              { method = "GET"
              , headers = [ Http.header "x-api-key" model.apikey ]
              , url = buildPreviousUrl beforeEntry model
              , body = Http.emptyBody
              , expect = Http.expectJson GotPrevious decodeChangeFeedEntriesResponse
              , timeout = Nothing
              , tracker = Nothing
              }
            )

        ToggleEntryContent entry ->
            case List.Extra.find (\candidate -> candidate.id == entry) model.entries of
                Just found ->
                    if found.contentType == "BeganRoadNetworkImport" || found.contentType == "CompletedRoadNetworkImport" then
                        ( model, Cmd.none )
                        -- nothing to toggle in these cases

                    else
                        case found.content of
                            Just _ ->
                                let
                                    entries =
                                        List.map
                                            (\candidate ->
                                                if candidate.id == entry then
                                                    { candidate | content = Nothing, state = RequiresLoading }

                                                else
                                                    candidate
                                            )
                                            model.entries
                                in
                                ( { model | entries = entries }
                                , Cmd.none
                                )

                            Nothing ->
                                let
                                    entries =
                                        List.map
                                            (\candidate ->
                                                if candidate.id == entry then
                                                    { candidate | state = Loading }

                                                else
                                                    candidate
                                            )
                                            model.entries
                                in
                                ( { model | entries = entries }
                                , Http.request
                                    { method = "GET"
                                    , headers = [ Http.header "x-api-key" model.apikey ]
                                    , url = buildEntryContentUrl entry model
                                    , body = Http.emptyBody
                                    , expect = Http.expectJson GotEntryContent decodeChangeFeedEntryContentResponse
                                    , timeout = Nothing
                                    , tracker = Nothing
                                    }
                                )

                Nothing ->
                    ( model, Cmd.none )

        GotHead result ->
            case result of
                Ok response ->
                    ( { model | entries = List.sortWith descending response.entries }
                    , Cmd.none
                    )

                Err error ->
                    ( translateHttpError model error translateBadHeadStatus, Cmd.none )

        GotNext result ->
            case result of
                Ok response ->
                    ( { model | entries = List.sortWith descending (List.append response.entries model.entries) }
                    , Cmd.none
                    )

                Err error ->
                    ( translateHttpError model error translateBadNextStatus, Cmd.none )

        GotPrevious result ->
            case result of
                Ok response ->
                    ( { model | loading = False, entries = List.sortWith descending (List.append model.entries response.entries) }
                    , Cmd.none
                    )

                Err error ->
                    let
                        m =
                            translateHttpError model error translateBadPreviousStatus
                    in
                    ( { m | loading = False }, Cmd.none )

        GotEntryContent result ->
            case result of
                Ok response ->
                    let
                        entries =
                            List.map
                                (\entry ->
                                    if entry.id == response.id then
                                        { entry | content = Just response.content, state = Loaded }

                                    else
                                        entry
                                )
                                model.entries
                    in
                    ( { model | entries = entries }
                    , Cmd.none
                    )

                Err error ->
                    ( translateHttpError model error translateBadPreviousStatus, Cmd.none )

        GotAlertMessage alertMessage ->
            let
                ( alertModel, alertCommand ) =
                    Alert.update alertMessage model.alert
            in
            ( { model | alert = alertModel }
            , Cmd.map GotAlertMessage alertCommand
            )



-- view


viewArchiveLinkContent : String -> Archive -> Html Message
viewArchiveLinkContent url archive =
    if not archive.available then
        -- WR-215 workaround
        -- text archive.filename
        a [ href (String.concat [ url, archive.id ]), class "link--icon link--icon--inline" ]
            [ i [ class "vi vi-paperclip", ariaHidden True ]
                []
            , text "Download"
            ]

    else
        a [ href (String.concat [ url, archive.id ]), class "link--icon link--icon--inline" ]
            [ i [ class "vi vi-paperclip", ariaHidden True ]
                []
            , text archive.filename
            ]

viewChangeProblemsList: List ChangeProblems -> Html Message
viewChangeProblemsList problems =
  ul
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
          problems
      )

viewFileProblemsList: List FileProblems -> Html Message
viewFileProblemsList problems =
  ul
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
          problems
      )

viewSummary: Maybe Summary -> Html Message
viewSummary mayBeSummary =
  case mayBeSummary of
    Just summary ->
      div
        [ class "grid"
        , style "background-color" "#e8ebee"
        , style "margin-bottom" "5px"
        , style "margin-left" "0px"
        , style "padding-bottom" "10px"]
        [
          div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.roadNodes.added) ]
            , div [ class "infotext__text"] [ text "Toegevoegde wegknopen" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.roadNodes.modified) ]
            , div [ class "infotext__text"] [ text "Gewijzigde wegknopen" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.roadNodes.removed) ]
            , div [ class "infotext__text"] [ text "Verwijderde wegknopen" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.roadSegments.added) ]
            , div [ class "infotext__text"] [ text "Toegevoegde wegsegmenten" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.roadSegments.modified) ]
            , div [ class "infotext__text"] [ text "Gewijzigde wegsegmenten" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.roadSegments.removed) ]
            , div [ class "infotext__text"] [ text "Verwijderde wegsegmenten" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.gradeSeparatedJunctions.added) ]
            , div [ class "infotext__text"] [ text "Toegevoegde ongelijkgrondse kruisingen" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.gradeSeparatedJunctions.modified) ]
            , div [ class "infotext__text"] [ text "Gewijzigde ongelijkgrondse kruisingen" ]
            ]
          ]
        , div [ class "col--4-12"]
          [
            div [ class "infotext"]
            [
              div [ class "infotext__value"] [ text (String.fromInt summary.gradeSeparatedJunctions.removed) ]
            , div [ class "infotext__text"] [ text "Verwijderde ongelijkgrondse kruisingen" ]
            ]
          ]
        ]
    Nothing ->
      text ""

viewChangeFeedEntryContent : String -> String-> ChangeFeedEntryContent -> Html Message
viewChangeFeedEntryContent url extractUrl content =
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
                [ viewFileProblemsList rejected.problems
                , br [] []
                , text "Archief: "
                , viewArchiveLinkContent url rejected.archive
                ]

        RoadNetworkChangesArchiveAccepted accepted ->
            div [ class "step__content" ]
                [ viewFileProblemsList accepted.problems
                , br [] []
                , text "Archief: "
                , viewArchiveLinkContent url accepted.archive
                ]

        RoadNetworkChangesBasedOnArchiveAccepted accepted ->
            div [ class "step__content" ]
                [
                  viewSummary accepted.summary
                , viewChangeProblemsList accepted.problems
                , br [] []
                , text "Archief: "
                , viewArchiveLinkContent url accepted.archive
                ]

        RoadNetworkChangesBasedOnArchiveRejected rejected ->
            div [ class "step__content" ]
                [ viewChangeProblemsList rejected.problems
                , br [] []
                , text "Archief: "
                , viewArchiveLinkContent url rejected.archive
                ]

        RoadNetworkExtractGotRequested ->
            div [ class "step__content" ]
                []

        RoadNetworkExtractDownloadBecameAvailable available ->
            div [ class "step__content" ]
                [ text "Archief: "
                , viewArchiveLinkContent extractUrl available.archive
                ]

        RoadNetworkExtractChangesArchiveUploaded uploaded ->
            div [ class "step__content" ]
                [ text "Archief: "
                , viewArchiveLinkContent url uploaded.archive
                ]

        RoadNetworkExtractChangesArchiveRejected rejected ->
            div [ class "step__content" ]
                [ viewFileProblemsList rejected.problems
                , br [] []
                , text "Archief: "
                , viewArchiveLinkContent url rejected.archive
                ]

        RoadNetworkExtractChangesArchiveAccepted accepted ->
            div [ class "step__content" ]
                [ viewFileProblemsList accepted.problems
                , br [] []
                , text "Archief: "
                , viewArchiveLinkContent url accepted.archive
                ]


onClickNoBubble : msg -> Html.Attribute msg
onClickNoBubble message =
    Html.Events.custom "click" (Decode.succeed { message = message, stopPropagation = True, preventDefault = True })


viewChangeFeedEntry : String -> String -> ChangeFeedEntry -> Html Message
viewChangeFeedEntry url extractUrl entry =
    li
        [ class "step" ]
        [ div [ class "step__icon" ]
            [ text entry.day
            , span [ class "step__icon__sub" ] [ text entry.month ]
            , span [ class "step__icon__sub" ] [ text entry.timeOfDay ]
            ]
        , div
            [ class "step__wrapper" ]
            [ a
                [ href ""
                , class "step__header"
                , onClickNoBubble (ToggleEntryContent entry.id)
                ]
                [ div [ class "step__header__titles" ]
                    [ h3 [ class "step__title" ]
                        [ text entry.title ]
                    ]
                , case entry.contentType of
                    "BeganRoadNetworkImport" ->
                        text ""

                    "CompletedRoadNetworkImport" ->
                        text ""

                    "RoadNetworkChangesArchiveAccepted" ->
                        div [ class "step__header__info", style "width" "30px" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkChangesArchiveRejected" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkChangesArchiveUploaded" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkChangesAccepted" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkChangesAccepted:v2" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkChangesRejected" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkExtractGotRequested" ->
                        text ""

                    "RoadNetworkExtractDownloadBecameAvailable" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkExtractChangesArchiveAccepted" ->
                        div [ class "step__header__info", style "width" "30px" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkExtractChangesArchiveRejected" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    "RoadNetworkExtractChangesArchiveUploaded" ->
                        div [ class "step__header__info" ]
                            [ i [ classList [ ( "vi", True ), ( "vi-plus", entry.state == RequiresLoading ), ( "vi-minus", entry.state == Loaded ) ] ]
                                []
                            , if entry.state == Loading then
                                div [ class "loader" ] []

                              else
                                text ""
                            ]

                    value ->
                        text ("Please fix handling an entry of type " ++ value)
                ]
            , div
                [ class "step__content-wrapper" ]
                [ case entry.content of
                    Just content ->
                        viewChangeFeedEntryContent url extractUrl content

                    Nothing ->
                        text ""
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
    let
        behavior =
            case getPreviousEntry model of
                Just entry ->
                    GetPrevious entry

                Nothing ->
                    GetHead
    in
    section [ class "region" ]
        [ div
            [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
            [ ul
                [ class "steps steps--timeline" ]
                (List.map (viewChangeFeedEntry model.archiveUrl model.extractUrl) model.entries)
            , div [ class "u-align-center" ]
                [ a [ class "button", href "", onClickNoBubble behavior, disabled model.loading ]
                    [ if model.loading then
                        text "Meer "

                      else
                        text "Meer ..."
                    , if model.loading then
                        div [ class "loader" ] []
                      else
                        text ""
                    ]
                ]
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

decodeRoadNetworkExtractDownloadBecameAvailable : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkExtractDownloadBecameAvailable =
    Decode.field "content"
        (Decode.map
            (\archive -> RoadNetworkExtractDownloadBecameAvailable { archive = archive })
            (Decode.field "archive" decodeArchive)
        )

decodeRoadNetworkExtractChangesArchiveUploaded : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkExtractChangesArchiveUploaded =
    Decode.field "content"
        (Decode.map
            (\archive -> RoadNetworkExtractChangesArchiveUploaded { archive = archive })
            (Decode.field "archive" decodeArchive)
        )

decodeRoadNetworkChangesArchiveUploaded : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveUploaded =
    Decode.field "content"
        (Decode.map
            (\archive -> RoadNetworkChangesArchiveUploaded { archive = archive })
            (Decode.field "archive" decodeArchive)
        )


decodeRoadNetworkExtractChangesArchiveAccepted : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkExtractChangesArchiveAccepted =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkExtractChangesArchiveAccepted { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "files" (Decode.list decodeFileProblems))
        )

decodeRoadNetworkChangesArchiveAccepted : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesArchiveAccepted =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesArchiveAccepted { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "files" (Decode.list decodeFileProblems))
        )

decodeRoadNetworkExtractChangesArchiveRejected : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkExtractChangesArchiveRejected =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkExtractChangesArchiveRejected { archive = archive, problems = problems })
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


decodeRoadNetworkChangesAccepted : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesAccepted =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesBasedOnArchiveAccepted { archive = archive, summary = Nothing, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "changes" (Decode.list decodeChangeProblems))
        )


decodeCounters : Decode.Decoder Counters
decodeCounters =
    Decode.map3 Counters
      (Decode.field "added" Decode.int)
      (Decode.field "modified" Decode.int)
      (Decode.field "removed" Decode.int)


decodeSummary : Decode.Decoder Summary
decodeSummary =
    Decode.map3 Summary
      (Decode.field "roadNodes" decodeCounters)
      (Decode.field "roadSegments" decodeCounters)
      (Decode.field "gradeSeparatedJunctions" decodeCounters)


decodeRoadNetworkChangesAcceptedV2 : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesAcceptedV2 =
    Decode.field "content"
        (Decode.map3
            (\archive summary problems -> RoadNetworkChangesBasedOnArchiveAccepted { archive = archive, summary = Just summary, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "summary" decodeSummary)
            (Decode.field "changes" (Decode.list decodeChangeProblems))
        )


decodeRoadNetworkChangesRejected : Decode.Decoder ChangeFeedEntryContent
decodeRoadNetworkChangesRejected =
    Decode.field "content"
        (Decode.map2
            (\archive problems -> RoadNetworkChangesBasedOnArchiveRejected { archive = archive, problems = problems })
            (Decode.field "archive" decodeArchive)
            (Decode.field "changes" (Decode.list decodeChangeProblems))
        )


decodeEntry : Decode.Decoder ChangeFeedEntry
decodeEntry =
    Decode.map8 ChangeFeedEntry
        (Decode.field "id" Decode.int)
        (Decode.field "title" Decode.string)
        (Decode.field "type" Decode.string)
        (Decode.succeed RequiresLoading)
        (Decode.field "day" Decode.string)
        (Decode.field "month" Decode.string)
        (Decode.field "timeOfDay" Decode.string)
        (Decode.succeed Nothing)


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
            , when decodeEntryContentType (is "RoadNetworkChangesAccepted") decodeRoadNetworkChangesAccepted
            , when decodeEntryContentType (is "RoadNetworkChangesAccepted:v2") decodeRoadNetworkChangesAcceptedV2
            , when decodeEntryContentType (is "RoadNetworkChangesRejected") decodeRoadNetworkChangesRejected
            , when decodeEntryContentType (is "RoadNetworkExtractGotRequested") (Decode.succeed RoadNetworkExtractGotRequested)
            , when decodeEntryContentType (is "RoadNetworkExtractDownloadBecameAvailable") decodeRoadNetworkExtractDownloadBecameAvailable
            , when decodeEntryContentType (is "RoadNetworkExtractChangesArchiveAccepted") decodeRoadNetworkExtractChangesArchiveAccepted
            , when decodeEntryContentType (is "RoadNetworkExtractChangesArchiveRejected") decodeRoadNetworkExtractChangesArchiveRejected
            , when decodeEntryContentType (is "RoadNetworkExtractChangesArchiveUploaded") decodeRoadNetworkExtractChangesArchiveUploaded
            ]
        )

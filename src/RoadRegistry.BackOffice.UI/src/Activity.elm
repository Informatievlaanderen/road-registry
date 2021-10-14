module Activity exposing (Msg(..), init, main, subscriptions, update, view)

import Alert
import Browser
import ChangeFeed
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, div, main_)
import Html.Attributes exposing (class, id)
import Time exposing (Posix, every)


main : Program Flags Model Msg
main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias Flags =
    { endpoint : String
    , oldEndpoint: String
    , apikey : String
    }


type alias Model =
    { header : HeaderModel
    , changeFeed : ChangeFeed.Model
    }


init : Flags -> ( Model, Cmd Msg )
init flags =
    let
        ( changeFeedModel, changeFeedCommand ) =
            ChangeFeed.init 25 flags.endpoint flags.oldEndpoint flags.apikey
    in
    ( { header = Header.init |> Header.activityBecameActive
      , changeFeed = changeFeedModel
      }
    , Cmd.map GotChangeFeedMessage changeFeedCommand
    )


type Msg
    = ToggleExpandEntry String
    | Tick Posix
    | GotChangeFeedMessage ChangeFeed.Message


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        ToggleExpandEntry _ ->
            ( model, Cmd.none )

        Tick _ ->
            case ChangeFeed.getNextEntry model.changeFeed of
                Just entry ->
                    let
                        changeFeedMessage =
                            ChangeFeed.GetNext entry

                        ( changeFeedModel, changeFeedCommand ) =
                            ChangeFeed.update changeFeedMessage model.changeFeed
                    in
                    ( { model | changeFeed = changeFeedModel }, Cmd.map GotChangeFeedMessage changeFeedCommand )

                Nothing ->
                    ( model, Cmd.none )

        GotChangeFeedMessage changeFeedMessage ->
            let
                ( changeFeedModel, changeFeedCommand ) =
                    ChangeFeed.update changeFeedMessage model.changeFeed
            in
            ( { model | changeFeed = changeFeedModel }
            , Cmd.map GotChangeFeedMessage changeFeedCommand
            )


viewMain : Model -> Html Msg
viewMain model =
    main_ [ id "main" ]
        [ Alert.view model.changeFeed.alert |> Html.map ChangeFeed.GotAlertMessage |> Html.map GotChangeFeedMessage
        , ChangeFeed.viewChangeFeedTitle model.changeFeed |> Html.map GotChangeFeedMessage
        , ChangeFeed.view model.changeFeed |> Html.map GotChangeFeedMessage
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

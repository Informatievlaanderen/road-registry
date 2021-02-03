module Main exposing (init, main, subscriptions, update, view)

import Browser
import Footer
import Header exposing (HeaderModel)
import Html exposing (Html, div, main_, section)
import Html.Attributes exposing (class, classList, id)


main : Program () Model ()
main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias Model =
    HeaderModel


init : () -> ( Model, Cmd () )
init () =
    ( Header.init |> Header.homeBecameActive
    , Cmd.none
    )


update : () -> Model -> ( Model, Cmd () )
update () model =
    ( model, Cmd.none )


viewEmpty : () -> Html ()
viewEmpty () =
    main_
        [ id "main" ]
        [ section
            [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                []
            ]
        ]


view : Model -> Html ()
view model =
    div [ class "page" ]
        [ Header.viewBanner ()
        , Header.viewHeader model
        , viewEmpty ()
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub ()
subscriptions _ =
    Sub.none

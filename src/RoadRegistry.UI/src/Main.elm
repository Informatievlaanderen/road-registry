module Main exposing (init, main, subscriptions, update, view)

import Browser
import File.Download as Download
import Footer
import Header exposing (HeaderAction, HeaderModel, TabAction)
import Html exposing (Html, a, button, div, h1, h2, header, li, main_, nav, section, span, text, ul)
import Html.Attributes exposing (attribute, class, classList, href, id, style, target)
import Html.Events exposing (onClick)


main =
    Browser.element { init = init, update = update, view = view, subscriptions = subscriptions }


type alias Model =
    { header : HeaderModel }


init : () -> ( Model, Cmd () )
init () =
    ( { header = Header.init |> Header.homeBecameActive }
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
        , Header.viewHeader model.header
        , viewEmpty ()
        , Footer.viewFooter ()
        ]


subscriptions : Model -> Sub ()
subscriptions model =
    Sub.none

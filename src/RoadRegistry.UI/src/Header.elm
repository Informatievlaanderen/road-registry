module Header exposing (HeaderAction, HeaderModel, TabAction, viewBanner, viewHeader)

import Html exposing (Html, a, div, h1, header, li, nav, span, text, ul)
import Html.Attributes exposing (attribute, class, classList, href, id, target)


type alias TabAction =
    { title : String, link : String, active : Bool }


type alias HeaderAction =
    { title : String, link : Maybe String }


type alias HeaderModel =
    { headerActions : List HeaderAction, tabActions : List TabAction }


viewHeaderAction : HeaderAction -> Html msg
viewHeaderAction action =
    case action.link of
        Just link ->
            li
                [ class "functional-header__action" ]
                [ a
                    [ href link ]
                    [ text action.title ]
                ]

        Nothing ->
            li 
                [ class "functional-header__action" ]
                [ text action.title ]


viewTabAction : TabAction -> Html msg
viewTabAction action =
    li
        [ classList [ ( "tab", True ), ( "tab--active", action.active ) ] ]
        [ a 
            [ class "tab__link", href action.link ]
            [ text action.title ]
        ]


viewBanner : () -> Html msg
viewBanner () =
    header
        [ id "vlaanderen-top" ]
        [ div
            [ id "vlaanderen-navigation" ]
            [ a
                [ id "vlaanderen-link", href "https://www.vlaanderen.be/nl", target "_self" ]
                [ div
                    [ id "vlaanderen-top-logo" ]
                    []
                , span
                    []
                    [ text "Vlaanderen" ]
                ]
            ]
        ]


viewHeader : HeaderModel -> Html msg
viewHeader model =
    header
        [ classList
            [ ( "functional-header", True )
            , ( "functional-header--has-actions", not (List.isEmpty model.headerActions) )
            ]
        ]
        [ div
            [ classList
                [ ( "layout", True )
                , ( "layout--wide", True )
                ]
            ]
            [ div
                [ class "functional-header__actions" ]
                [ ul [] (List.map viewHeaderAction model.headerActions) ]
            , div
                [ class "functional-header__content" ]
                [ h1
                    [ class "functional-header__title" ]
                    [ a [ class "functional-header__title", href "/" ]
                        [ text "WEGENREGISTER" ]
                    ]
                ]
            , div
                [ class "functional-header__sub" ]
                [ div
                    [ class "grid" ]
                    [ nav
                        [ class "col--9-12 col--8-12s col--1-1s", attribute "data-tabs-responsive-label" "Navigatie" ]
                        [ div
                            [ class "tabs__wrapper" ]
                            [ ul
                                [ class "tabs" ]
                                (List.map viewTabAction model.tabActions)
                            ]
                        ]
                    ]
                ]
            ]
        ]

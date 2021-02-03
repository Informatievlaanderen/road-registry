module Alert exposing (AlertKind(..), Message(..), Model, hide, init, showError, showSuccess, update, view)

import Html exposing (Html, a, div, section, span, text)
import Html.Attributes exposing (class, classList, href)
import Html.Attributes.Aria exposing (ariaHidden)
import Html.Events
import Json.Decode as Decode


type AlertKind
    = Error
    | Warning
    | Success
    | CallToAction


type alias Model =
    { title : String
    , kind : AlertKind
    , hasIcon : Bool
    , closeable : Bool
    , visible : Bool
    }


type Message
    = CloseAlert


init : () -> Model
init () =
    { title = ""
    , kind = Error
    , visible = False
    , closeable = True
    , hasIcon = True
    }


update : Message -> Model -> ( Model, Cmd Message )
update message model =
    case message of
        CloseAlert ->
            ( hide model, Cmd.none )


showError : Model -> String -> Model
showError model title =
    { model | title = title, kind = Error, visible = True }


showSuccess : Model -> String -> Model
showSuccess model title =
    { model | title = title, kind = Success, visible = True }


hide : Model -> Model
hide model =
    { model | visible = False }


onClickNoBubble : msg -> Html.Attribute msg
onClickNoBubble message =
    Html.Events.custom "click" (Decode.succeed { message = message, stopPropagation = True, preventDefault = True })


view : Model -> Html Message
view model =
    if model.visible then
        section [ class "region" ]
            [ div
                [ classList [ ( "layout", True ), ( "layout--wide", True ) ] ]
                [ div
                    [ classList
                        [ ( "alert", True )
                        , ( "alert--error"
                          , if model.kind == Error then
                                True

                            else
                                False
                          )
                        , ( "alert--warning"
                          , if model.kind == Warning then
                                True

                            else
                                False
                          )
                        , ( "alert--success"
                          , if model.kind == Success then
                                True

                            else
                                False
                          )
                        , ( "alert--cta"
                          , if model.kind == CallToAction then
                                True

                            else
                                False
                          )
                        , ( "alert--closable", model.closeable )
                        ]
                    ]
                    [ if model.closeable then
                        a
                            [ href ""
                            , classList [ ( "alert__close", True ), ( "link--icon--close", True ), ( "u-float-right", True ) ]
                            , onClickNoBubble CloseAlert
                            ]
                            [ span [ class "u-visually-hidden" ] [ text "Sluiten" ] ]

                      else
                        text ""
                    , if model.hasIcon then
                        div [ class "alert__icon", ariaHidden True ]
                            []

                      else
                        text ""
                    , div
                        [ class "alert__content" ]
                        [ div
                            [ class "alert__title" ]
                            [ text model.title ]
                        , div
                            [ class "alert__message" ]
                            [ div
                                [ class "typography" ]
                                []
                            ]
                        ]
                    ]
                ]
            ]

    else
        text ""

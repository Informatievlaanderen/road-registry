module Alert exposing (AlertKind(..), AlertModel, AlertMsg(..), hideAlert, showAlert, viewAlert)

import Html exposing (Html, a, div, span, text)
import Html.Attributes exposing (class, classList, href, name)
import Html.Attributes.Aria exposing (ariaHidden)
import Html.Events exposing (on, onClick)
import Json.Decode as Decode


type AlertKind
    = Error
    | Warning
    | Success
    | CallToAction


type alias AlertModel =
    { title : String, kind : AlertKind, hasIcon : Bool, closeable : Bool, visible : Bool }


type AlertMsg
    = CloseAlert


showAlert : AlertModel -> String -> AlertModel
showAlert model title =
    { model | title = title, visible = True }


hideAlert : AlertModel -> AlertModel
hideAlert model =
    { model | visible = True }


onClickNoBubble : msg -> Html.Attribute msg
onClickNoBubble message =
    Html.Events.custom "click" (Decode.succeed { message = message, stopPropagation = True, preventDefault = True })


viewAlert : AlertModel -> Html AlertMsg
viewAlert model =
    if model.visible then
        div
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

    else
        text ""

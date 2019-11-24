module HttpBytes exposing (expect)

import Bytes exposing (Bytes)
import Http


rawBytesFromResponse : Http.Response Bytes -> Result Http.Error Bytes
rawBytesFromResponse response =
    case response of
        Http.BadUrl_ url ->
            Err (Http.BadUrl url)

        Http.Timeout_ ->
            Err Http.Timeout

        Http.NetworkError_ ->
            Err Http.NetworkError

        Http.BadStatus_ metadata _ ->
            Err (Http.BadStatus metadata.statusCode)

        Http.GoodStatus_ _ body ->
            Ok body


expect : (Result Http.Error Bytes -> msg) -> Http.Expect msg
expect toMessage =
    Http.expectBytesResponse toMessage rawBytesFromResponse

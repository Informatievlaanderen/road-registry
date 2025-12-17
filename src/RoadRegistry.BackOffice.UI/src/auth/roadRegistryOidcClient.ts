import { OidcClient } from "oidc-client-ts";
import PublicApi from "@/services/public-api";

export default class RoadRegistryOidcClient {
  private static _client: OidcClient | undefined = undefined;

  static async initialize(): Promise<void> {
    if (RoadRegistryOidcClient._client) {
      return;
    }

    const info = await PublicApi.Security.getInfo();

    RoadRegistryOidcClient._client = new OidcClient({
      authority: info.authorizationEndpoint,
      metadata: {
        issuer: info.issuer,
        authorization_endpoint: info.authorizationEndpoint,
        userinfo_endpoint: info.userInfoEndPoint,
        end_session_endpoint: info.endSessionEndPoint,
        jwks_uri: info.jwksUri,
      },
      signingKeys: [{ x: "RS256" }],

      client_id: info.clientId,
      redirect_uri: info.authorizationRedirectUri,
      post_logout_redirect_uri: info.postLogoutRedirectUri,
      response_type: "code",
      scope: "openid profile vo dv_wegenregister",
      filterProtocolClaims: true,
      loadUserInfo: true,
      //query_status_response_type: "code",
    });
  }

  static get instance(): OidcClient {
    return RoadRegistryOidcClient._client as OidcClient;
  }
}

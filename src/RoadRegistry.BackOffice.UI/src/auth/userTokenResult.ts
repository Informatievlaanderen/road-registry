import { jwtDecode } from "jwt-decode";
import RoadRegistry from "@/types/road-registry";

const convertUserInfoToToken = (userInfo: RoadRegistry.UserInfo) => {
  return userInfo.claims.reduce((target: any, item: RoadRegistry.UserClaim) => {
    const targetValue = target[item.type];
    let itemValue: any = item.value;
    if (itemValue === "True") {
      itemValue = true;
    } else if (targetValue === "False") {
      itemValue = false;
    }
    if (targetValue === undefined) {
      target[item.type] = itemValue;
    } else if (Array.isArray(targetValue)) {
      target[item.type] = [...targetValue, itemValue];
    } else {
      target[item.type] = [targetValue, itemValue];
    }
    return target;
  }, {});
};

export default class UserTokenResult {
  constructor(decodedJwtToken?: any) {
    this.token = decodedJwtToken ?? {};
    if (decodedJwtToken) {
      console.log("token", decodedJwtToken);
    }
    this.contexts = this.parseContexts();
  }

  private readonly token: any;
  public readonly contexts: string[];

  get isExpired(): boolean {
    if (!this.token.exp) {
      return false;
    }
    return Date.now() >= this.token.exp * 1000;
  }

  get firstName(): string {
    return this.token.given_name ?? this.token["urn:be:vlaanderen:acm:voornaam"];
  }
  get lastName(): string {
    return this.token.family_name ?? this.token["urn:be:vlaanderen:acm:familienaam"];
  }
  get isInwinner(): boolean {
    return this.token.vo_doelgroepcode === 'EA';
  }

  parseContexts(): string[] {
    const dvWegenregister = this.token.dv_wegenregister ?? [];
    const dvWegenregisterClaims = (Array.isArray(dvWegenregister) ? dvWegenregister : [dvWegenregister]).filter(
      (x: string) => x.startsWith("DVWegenregister-")
    );
    return dvWegenregisterClaims.map((x: string) => x.split("-")[1].split(":")[0]);
  }

  static empty() {
    return new UserTokenResult();
  }

  static fromJwt(jwt: string) {
    if (!jwt) {
      return UserTokenResult.empty();
    }

    return new UserTokenResult(jwtDecode(jwt));
  }

  static fromUserInfo(userInfo: RoadRegistry.UserInfo) {
    return new UserTokenResult(convertUserInfoToToken(userInfo));
  }
}

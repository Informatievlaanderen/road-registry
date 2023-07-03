import jwtDecode from "jwt-decode";

export default class UserTokenResult {
  constructor(decodedJwtToken?: any) {
    this.token = decodedJwtToken ?? {};
    if (decodedJwtToken) {
      console.log('token', decodedJwtToken);
    }
  }

  private readonly token: any;

  get expiration(): number {
    return this.token.exp ?? 0;
  }

  get isExpired(): boolean {
    return Date.now() >= this.expiration * 1000;
  }

  get firstName(): string {
    return this.token.given_name ?? this.token['urn:be:vlaanderen:acm:voornaam'];
  }
  get lastName(): string {
    return this.token.family_name ?? this.token['urn:be:vlaanderen:acm:familienaam'];
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
}

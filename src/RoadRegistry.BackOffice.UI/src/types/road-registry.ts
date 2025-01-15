export namespace RoadRegistry {
  export interface GetHeadApiResponse {
    entries: ChangeFeedEntry[];
  }

  export interface ChangeFeedEntry {
    id: number;
    title: string;
    type: string;
    day: string;
    month: string;
    timeOfDay: string;
  }

  export interface ChangeFeedContent {
    id: number;
    type: string;
    content: any;
  }

  export interface RoadNetworkInformationResponse {
    completedImport: boolean;
    organizationCount: number;
    roadNodeCount: number;
    roadSegmentCount: number;
    roadSegmentEuropeanRoadAttributeCount: number;
    roadSegmentNumberedRoadAttributeCount: number;
    roadSegmentNationalRoadAttributeCount: number;
    roadSegmentLaneAttributeCount: number;
    roadSegmentWidthAttributeCount: number;
    roadSegmentSurfaceAttributeCount: number;
    gradeSeparatedJunctionCount: number;
  }

  export interface UploadExtractResponseBody {
    status: number;
    uploadId: string;
    changeRequestId: string;
  }

  export interface UploadPresignedUrlResponse {
    uploadUrl: string;
    uploadUrlFormData: any;
    ticketUrl: string;
  }

  export interface GetUploadDownloadPreSignedUrlResponse {
    downloadUrl: string;
    fileName: string;
  }

  export interface GetExtractDownloadPreSignedUrlResponse {
    downloadUrl: string;
  }

  export interface GetTicketResponse {
    status: string;
    ticketId: string;
    result: TicketResult;
  }
  export interface TicketResult {
    json: string;
  }

  export interface DownloadExtractRequest {
    requestId: string;
    contour: string;
  }
  export interface DownloadExtractResponse {
    downloadId: string;
  }

  export interface DownloadExtractByContourRequest {
    contour: string;
    description: string;
    isInformative: Boolean;
  }

  export interface DownloadExtractByFileRequest {
    files: File[];
    description: string;
    isInformative: Boolean;
  }

  export interface PerContourErrorResponse {
    validationErrors: PerContourValidationErrors;
  }

  export interface PerContourValidationErrors {
    contour: ContourValidationError[];
  }

  export interface ContourValidationError {
    code: string;
    reason: string;
  }

  export interface DownloadExtractByNisCodeRequest {
    nisCode: string;
    description: string;
    isInformative: Boolean;
  }

  export interface ListOverlappingExtractsByNisCodeRequest {
    nisCode: string;
  }
  export interface ListOverlappingExtractsByContourRequest {
    contour: string;
  }
  export interface ListOverlappingExtractsResponse {
    downloadIds: string[];
  }

  export interface ValidateWktResponse {
    area: number;
    areaMaximumSquareKilometers: number;
    isValid: boolean;
    isLargerThanMaximumArea: boolean;
  }

  export interface SecurityInfo {
    authority: string;
    issuer: string;
    authorizationEndpoint: string;
    authorizationRedirectUri: string;
    userInfoEndPoint: string;
    endSessionEndPoint: string;
    jwksUri: string;
    clientId: string;
    postLogoutRedirectUri: string;
  }

  export interface UserInfo {
    claims: UserClaim[];
  }

  export interface UserClaim {
    type: string;
    value: string;
  }

  export enum UserContext {
    Lezer = "lezer",
    Editeerder = "editeerder",
    Admin = "admin",
  }
}
export default RoadRegistry;

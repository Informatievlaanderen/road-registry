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

  export interface DownloadUploadResponse {
    downloadUrl: string;
    fileName: string;
  }

  export interface ExtractDownloadaanvraagResponse {
    downloadId: string;
  }

  export interface RequestExtractResponse {
    downloadId: string;
    ticketUrl: string;
  }

  export interface CloseExtractResponse {
    ticketUrl: string;
  }

  export interface GetTicketResponse {
    status: string;
    ticketId: string;
    result: TicketResult;
  }
  export interface TicketResult {
    json: string;
  }

  export interface ExtractDetails {
    downloadId: string;
    description: string;
    extractRequestId: string;
    isInformative: boolean;
    archiveId: string;
    ticketId: string;
    downloadAvailable: boolean;
    extractDownloadTimeoutOccurred: boolean;
  }
  export interface ExtractDetailsV2 {
    downloadId: string;
    beschrijving: string;
    externeId: string;
    informatief: boolean;
    aangevraagdOp: string;
    downloadStatus: string;
    gedownloadOp: string;
    uploadStatus: string;
    gesloten: boolean;
    ticketId: string;
  }
  export interface ExtractListResponse {
    items: ExtractListItem[];
    dataBeschikbaar: boolean;
  }
  export interface ExtractListItem {
    downloadId: string;
    beschrijving: string;
    aangevraagdOp: string;
    informatief: boolean;
    downloadStatus: string;
    uploadStatus: string;
    gesloten: boolean;
  }

  export interface DownloadExtractRequest {
    requestId: string;
    contour: string;
  }
  export interface DownloadExtractResponse {
    downloadUrl: string;
  }
  export interface DownloadExtractResponseBody {
    downloadId: string;
    isInformative: boolean;
  }

  export interface DownloadExtractByContourRequest {
    contour: string;
    description: string;
    isInformative: boolean;
  }
  export interface ExtractDownloadaanvraagPerContourBody {
    contour: string;
    beschrijving: string;
    informatief: boolean;
  }

  export interface DownloadExtractByFileRequest {
    files: File[];
    description: string;
    isInformative: boolean;
  }
  export interface ExtractDownloadaanvraagPerBestandBody {
    bestanden: File[];
    beschrijving: string;
    informatief: boolean;
  }

  export interface BadRequestResponse {
    validationErrors: any;
  }

  export interface ValidationError {
    code: string;
    reason: string;
  }

  export interface DownloadExtractByNisCodeRequest {
    nisCode: string;
    description: string;
    isInformative: boolean;
  }
  export interface ExtractDownloadaanvraagPerNisCodeBody {
    nisCode: string;
    beschrijving: string;
    informatief: boolean;
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

  export interface TicketDetails {
    status: string;
    metadata: TicketMetadata;
    result: TicketResult;
  }

  export interface TicketMetadata {
    action: string;
  }

  export interface TicketResult {
    json: string;
  }
}
export default RoadRegistry;

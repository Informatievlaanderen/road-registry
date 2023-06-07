export namespace RoadRegistry {

    export interface GetHeadApiResponse {
        entries: ChangeFeedEntry[]
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
        completedImport  : boolean;
        organizationCount  : number;
        roadNodeCount : number;
        roadSegmentCount : number;
        roadSegmentEuropeanRoadAttributeCount : number;
        roadSegmentNumberedRoadAttributeCount : number;
        roadSegmentNationalRoadAttributeCount : number;
        roadSegmentLaneAttributeCount  : number;
        roadSegmentWidthAttributeCount : number;
        roadSegmentSurfaceAttributeCount  : number;
        gradeSeparatedJunctionCount: number;
    }

    export interface UploadExtractResponseBody
    {
        uploadId: string;
    }

    export interface DownloadExtractRequest {
        requestId: string;
        contour: string;
    }
    export interface DownloadExtractResponse {
        downloadId : string;
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
        buffer: number;
        nisCode: string;
        description: string;
        isInformative: Boolean;
    }

    export interface ValidateWktResponse {
        area: number;
        areaMaximumSquareKilometers: number;
        isValid: boolean;
        isLargerThanMaximumArea: boolean;
    }
}
export default RoadRegistry;

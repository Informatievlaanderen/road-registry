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
        buffer: number;
        contour: string;
        description: string;
    }

    export interface DownloadExtractByFileRequest {
        buffer: number;
        files: File[];
        description: string;
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
    }
}
export default RoadRegistry;

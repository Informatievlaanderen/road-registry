import RoadRegistry from "./road-registry";

export namespace RoadRegistryExceptions {
    export class RequestExtractPerContourError extends Error {
        error: RoadRegistry.PerContourErrorResponse;
        constructor(error: RoadRegistry.PerContourErrorResponse) {
            super();
            this.error = error;
        }
    }
}

export default RoadRegistryExceptions;
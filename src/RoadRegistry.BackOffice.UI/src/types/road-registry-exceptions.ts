import RoadRegistry from "./road-registry";

export namespace RoadRegistryExceptions {
    export class BadRequestError extends Error {
        error: RoadRegistry.BadRequestResponse;
        constructor(error: RoadRegistry.BadRequestResponse) {
            super();
            this.error = error;
        }
    }
}

export default RoadRegistryExceptions;
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
}
export default RoadRegistry;

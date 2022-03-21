import apiClient from "./api-client";
import RoadRegistry from "@/types/road-registry";

export const PublicApi = {
    ChangeFeed: {
        getHead: async (maxEntryCount: number): Promise<RoadRegistry.GetHeadApiResponse> => {
            const path = "v1/wegen/activiteit/begin";
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { maxEntryCount })
            return response.data as RoadRegistry.GetHeadApiResponse;
        },
        getContent: async (id: number) => {
            const path = `v1/wegen/activiteit/gebeurtenis/${id}/inhoud`;
            const response = await apiClient.get<RoadRegistry.ChangeFeedContent>(path)
            return response.data;
        },
        getNext: async (afterEntry?: number, maxEntryCount?: number) => {
            const path = `v1/wegen/activiteit/volgende`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { afterEntry, maxEntryCount })
            return response.data;
        },
        getPrevious: async (beforeEntry?: number, maxEntryCount?: number) => {
            const path = `v1/wegen/activiteit/vorige`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { beforeEntry, maxEntryCount })
            return response.data;
        }
    },
    
}
export default PublicApi;
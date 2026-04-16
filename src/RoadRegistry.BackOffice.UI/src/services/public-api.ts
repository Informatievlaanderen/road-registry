import apiClient, { AxiosHttpApiClient, convertError } from "./api-client";
import RoadRegistry from "@/types/road-registry";
import Municipalities from "@/types/municipalities";
import axios from "axios";
import { trimEnd } from "lodash";
import { featureToggles, API_ENDPOINT } from "@/environment";
import BackOfficeApi from "./backoffice-api";
import { downloadFile } from "@/core/utils/file-utils";

const directApiEndpoint = trimEnd(API_ENDPOINT, "/");
const apiEndpoint = trimEnd(featureToggles.useDirectApiCalls ? directApiEndpoint : "/public", "/");
const useBackOfficeApi = process.env.NODE_ENV !== "production";

export const PublicApi = {
  ChangeFeed: {
    getHead: async (maxEntryCount: number, filter?: string): Promise<RoadRegistry.GetHeadApiResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getHead(maxEntryCount, filter);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/begin`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { maxEntryCount, filter });
      return response.data as RoadRegistry.GetHeadApiResponse;
    },
    getContent: async (id: number): Promise<RoadRegistry.ChangeFeedContent> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getContent(id);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/gebeurtenis/${id}/inhoud`;
      const response = await apiClient.get<RoadRegistry.ChangeFeedContent>(path);
      return response.data;
    },
    getNext: async (afterEntry?: number, maxEntryCount?: number, filter?: string) => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getNext(afterEntry, maxEntryCount, filter);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/volgende`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, {
        afterEntry,
        maxEntryCount,
        filter,
      });
      return response.data;
    },
    getPrevious: async (beforeEntry?: number, maxEntryCount?: number, filter?: string) => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getPrevious(beforeEntry, maxEntryCount, filter);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/vorige`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, {
        beforeEntry,
        maxEntryCount,
        filter,
      });
      return response.data;
    },
  },
  Extracts: {
    V2: {
      requestExtractByContour: async (
        downloadRequest: RoadRegistry.ExtractDownloadaanvraagPerContourBody
      ): Promise<RoadRegistry.RequestExtractResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.requestExtractByContour(downloadRequest);
        }

        try {
          const path = `${apiEndpoint}/v2/wegen/extracten/downloadaanvragen/percontour`;
          const response = await apiClient.post<RoadRegistry.ExtractDownloadaanvraagResponse>(path, downloadRequest);
          return {
            downloadId: response.data.downloadId,
            ticketUrl: response.headers.location,
          };
        } catch (exception) {
          throw convertError(exception);
        }
      },
      requestExtractByFile: async (
        downloadRequest: RoadRegistry.ExtractDownloadaanvraagPerBestandBody
      ): Promise<RoadRegistry.RequestExtractResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.requestExtractByFile(downloadRequest);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/downloadaanvragen/perbestand`;

        const data = new FormData();
        data.append("beschrijving", downloadRequest.beschrijving);
        data.append("informatief", downloadRequest.informatief.toString());
        downloadRequest.bestanden.forEach((file) => {
          data.append("bestanden", file, file.name);
        });

        try {
          const response = await apiClient.post<RoadRegistry.ExtractDownloadaanvraagResponse>(path, data);
          return {
            downloadId: response.data.downloadId,
            ticketUrl: response.headers.location,
          };
        } catch (exception) {
          throw convertError(exception);
        }
      },
      requestExtractByNisCode: async (
        downloadRequest: RoadRegistry.ExtractDownloadaanvraagPerNisCodeBody
      ): Promise<RoadRegistry.RequestExtractResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.requestExtractByNisCode(downloadRequest);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/downloadaanvragen/perniscode`;

        try {
          const response = await apiClient.post<RoadRegistry.ExtractDownloadaanvraagResponse>(path, downloadRequest);
          return {
            downloadId: response.data.downloadId,
            ticketUrl: response.headers.location,
          };
        } catch (exception) {
          throw convertError(exception);
        }
      },

      getOverlappingExtractsByNisCode: async (
        nisCode: string
      ): Promise<RoadRegistry.ListOverlappingExtractsResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.getOverlappingExtractsByNisCode(nisCode);
        }

        const request = {
          nisCode,
        } as RoadRegistry.ListOverlappingExtractsByNisCodeRequest;
        const path = `${apiEndpoint}/v2/wegen/extracten/overlapping/perniscode`;
        const response = await apiClient.post<RoadRegistry.ListOverlappingExtractsResponse>(path, request);
        return response.data;
      },
      getOverlappingExtractsByContour: async (
        contour: string
      ): Promise<RoadRegistry.ListOverlappingExtractsResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.getOverlappingExtractsByContour(contour);
        }

        const request = {
          contour,
        } as RoadRegistry.ListOverlappingExtractsByContourRequest;
        const path = `${apiEndpoint}/v2/wegen/extracten/overlapping/percontour`;
        const response = await apiClient.post<RoadRegistry.ListOverlappingExtractsResponse>(path, request);
        return response.data;
      },

      getList: async (eigenExtracten: boolean, page: number): Promise<RoadRegistry.ExtractListResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.getList(eigenExtracten, page);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten`;
        const response = await apiClient.get<RoadRegistry.ExtractListResponse>(path, {
          eigenExtracten: eigenExtracten,
          page: page || undefined,
        });
        return response.data;
      },
      getDetails: async (downloadId: string): Promise<RoadRegistry.ExtractDetailsV2> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.getDetails(downloadId);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/${downloadId}`;
        const response = await apiClient.get<RoadRegistry.ExtractDetailsV2>(path);
        return response.data;
      },
      downloadExtract: async (downloadId: string): Promise<void> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.downloadExtract(downloadId);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/${downloadId}/download`;
        const response = await apiClient.get<RoadRegistry.DownloadExtractResponse>(path);

        downloadFile(response.data.downloadUrl, response.data.fileName || `${downloadId}.zip`);
      },
      downloadUpload: async (downloadId: string): Promise<void> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.downloadUpload(downloadId);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/${downloadId}/upload`;
        const response = await apiClient.get<RoadRegistry.DownloadUploadResponse>(path);

        downloadFile(response.data.downloadUrl, response.data.fileName || `${downloadId}_upload.zip`);
      },
      upload: async (
        downloadId: string,
        file: Blob,
        filename: string
      ): Promise<RoadRegistry.UploadPresignedUrlResponse | null> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.upload(downloadId, file, filename);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/${downloadId}/upload`;
        const response = await apiClient.post<RoadRegistry.UploadPresignedUrlResponse>(path);

        const data = new FormData();
        if (response.data.uploadUrlFormData) {
          for (const key in response.data.uploadUrlFormData) {
            data.append(key, response.data.uploadUrlFormData[key]);
          }
        }
        data.append("file", file, filename);

        const awsHttp = axios.create();
        const uploadFileResponse = await awsHttp.post(response.data.uploadUrl, data);

        const status = uploadFileResponse.status as any;
        if (status !== 204) {
          return null;
        }

        return response.data;
      },
      close: async (downloadId: string): Promise<RoadRegistry.CloseExtractResponse> => {
        if (useBackOfficeApi) {
          return BackOfficeApi.Extracts.V2.close(downloadId);
        }

        const path = `${apiEndpoint}/v2/wegen/extracten/${downloadId}/sluit`;
        const response = await apiClient.post(path);
        return {
          ticketUrl: response.headers.location,
        };
      },
    },
  },
  Inwinning: {
    getNisCodes: async (): Promise<string[]> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Inwinning.getNisCodes();
      }

      const path = `${apiEndpoint}/v2/wegen/inwinning/niscodes`;
      const response = await apiClient.get<string[]>(path);
      return response.data;
    },
    requestExtract: async (
      downloadRequest: RoadRegistry.ExtractDownloadaanvraagPerNisCodeBody
    ): Promise<RoadRegistry.RequestExtractResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Inwinning.requestExtract(downloadRequest);
      }

      const path = `${apiEndpoint}/v2/wegen/inwinning/downloadaanvraag`;

      try {
        const response = await apiClient.post<RoadRegistry.ExtractDownloadaanvraagResponse>(path, downloadRequest);
        return {
          downloadId: response.data.downloadId,
          ticketUrl: response.headers.location,
        };
      } catch (exception) {
        throw convertError(exception);
      }
    },
    getList: async (): Promise<RoadRegistry.ExtractListResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Inwinning.getList();
      }

      const path = `${apiEndpoint}/v2/wegen/inwinning/extracten`;
      const response = await apiClient.get<RoadRegistry.ExtractListResponse>(path);
      return response.data;
    },
    upload: async (
      downloadId: string,
      file: Blob,
      filename: string
    ): Promise<RoadRegistry.UploadPresignedUrlResponse | null> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Inwinning.upload(downloadId, file, filename);
      }

      const path = `${apiEndpoint}/v2/wegen/inwinning/${downloadId}/upload`;
      const response = await apiClient.post<RoadRegistry.UploadPresignedUrlResponse>(path);

      const data = new FormData();
      if (response.data.uploadUrlFormData) {
        for (const key in response.data.uploadUrlFormData) {
          data.append(key, response.data.uploadUrlFormData[key]);
        }
      }
      data.append("file", file, filename);

      const awsHttp = axios.create();
      const uploadFileResponse = await awsHttp.post(response.data.uploadUrl, data);

      const status = uploadFileResponse.status as any;
      if (status !== 204) {
        return null;
      }

      return response.data;
    },
  },
  Information: {
    getInformation: async (): Promise<RoadRegistry.RoadNetworkInformationResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Information.getInformation();
      }

      const path = `${apiEndpoint}/v2/wegen/informatie`;
      const response = await apiClient.get<RoadRegistry.RoadNetworkInformationResponse>(path);
      return response.data;
    },
    postValidateWkt: async (wkt: string): Promise<RoadRegistry.ValidateWktResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Information.postValidateWkt(wkt);
      }

      const path = `${apiEndpoint}/v2/wegen/informatie/valideer-wkt`;
      const response = await apiClient.post(path, { contour: wkt });
      return response.data;
    },
  },
  Security: {
    getInfo: async (): Promise<RoadRegistry.SecurityInfo> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Security.getInfo();
      }

      const path = `${apiEndpoint}/v2/wegen/security/info`;
      const response = await apiClient.get<RoadRegistry.SecurityInfo>(path);
      return response.data;
    },
    getExchangeCode: async (
      code: string,
      verifier: string,
      redirectUri: string
    ): Promise<RoadRegistry.CodeExchangeResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Security.getExchangeCode(code, verifier, redirectUri);
      }

      const path = `${apiEndpoint}/v2/wegen/security/exchange?code=${code}&verifier=${verifier}&redirectUri=${redirectUri}`;
      const response = await apiClient.get<RoadRegistry.CodeExchangeResponse>(path);
      return response.data;
    },
    getAuthenticatedUser: async (): Promise<RoadRegistry.UserInfo> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Security.getAuthenticatedUser();
      }

      const apiClient = new AxiosHttpApiClient({
        noRedirectOnUnauthorized: true,
      });
      const path = `${apiEndpoint}/v2/wegen/security/user`;
      const response = await apiClient.get<RoadRegistry.UserInfo>(path);
      return response.data;
    },
  },
  Municipalities: {
    getAll: async (): Promise<Municipalities.Gemeenten[]> => {
      const municipalities = [] as Municipalities.Gemeenten[];

      const headers = { Accept: "application/ld+json" };
      const query = {
        offset: 0,
        limit: 500,
        status: "InGebruik",
        isFlemishRegion: true,
      };

      while (true) {
        const response = (
          await apiClient.get<Municipalities.GetMunicipalitiesAPIResponse>(
            `${apiEndpoint}/v2/gemeenten`,
            query,
            headers
          )
        ).data;
        if (response.gemeenten) {
          municipalities.push(...response.gemeenten);
        }

        if (!response.volgende) {
          break;
        }

        query.offset += query.limit;
      }

      return municipalities;
    },
  },
  Ticketing: {
    get: async (id: string): Promise<RoadRegistry.TicketDetails> => {
      const path = `${apiEndpoint}/v2/tickets/${id}`;
      const response = await apiClient.get<RoadRegistry.TicketDetails>(path, { t: new Date().getTime() });
      return response.data;
    },
  },
};
export default PublicApi;

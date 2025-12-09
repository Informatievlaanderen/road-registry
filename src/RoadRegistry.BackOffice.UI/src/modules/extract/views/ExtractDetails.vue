<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <wr-h2>Extract {{ isInformative ? " (informatief)" : "" }}</wr-h2>
          </vl-column>

          <vl-column>
            <div v-if="downloadAvailable === undefined">Het extract is nog niet beschikbaar, even geduld a.u.b.</div>

            <div v-if="downloadAvailable === false">
              <div v-if="extractStatus">
                <vl-alert :mod-error="extractStatus.error">
                  {{ extractStatus.message }}
                </vl-alert>
              </div>
              <div v-else>Het extract is nog niet beschikbaar, even geduld a.u.b.</div>
            </div>

            <div v-if="downloadAvailable === true">
              <div>{{ description }}</div>

              <div>
                <br />
                <span>
                  <vl-button v-if="isDownloading" mod-loading> Download extract... </vl-button>
                  <vl-button v-else @click="downloadExtract()"> Download extract </vl-button>
                </span>
                <span v-if="archiveId" style="margin-left: 1rem">
                  <vl-button v-if="isDownloading" mod-loading> Download upload... </vl-button>
                  <vl-button v-else @click="downloadUpload()"> Download upload </vl-button>
                </span>
                <div v-if="downloadError">
                  <br />
                  <vl-alert :mod-error="true">
                    {{ downloadError }}
                  </vl-alert>
                </div>
              </div>

              <br />
              <UploadComponent
                v-if="!isInformative && !uploadAndChangeAccepted"
                @upload-complete="handleUploadComplete"
              />

              <div v-if="ticketId && uploadStatus.text">
                <vl-alert
                  :title="uploadStatus.title"
                  :mod-success="uploadStatus.success"
                  :mod-warning="uploadStatus.warning"
                  :mod-error="uploadStatus.error"
                >
                  {{ uploadStatus.text }}
                </vl-alert>
              </div>

              <div v-if="fileProblems.length > 0">
                <div v-for="fileProblem in fileProblems" :key="fileProblem.file">
                  <br />
                  <h3 v-if="fileProblem.file && fileProblem.file !== 'ticketId'">
                    <strong>{{ fileProblem.file.toUpperCase() }}</strong>
                  </h3>

                  <ActivityProblems :problems="fileProblem.problems" />
                </div>
              </div>

              <div v-if="changes.length > 0">
                <ActivitySummary v-if="summary" :summary="summary" />
                <div v-for="change in changes" :key="change.change">
                  <h3>
                    <strong>{{ change.change }}</strong>
                  </h3>
                  <ActivityProblems :problems="change.problems" />
                  <br />
                </div>
              </div>
            </div>
          </vl-column>
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { orderBy, uniq, uniqBy, camelCase } from "lodash";
import { BackOfficeApi, PublicApi } from "../../../services";
import { featureToggles } from "@/environment";
import ActivityProblems from "../../activity/components/ActivityProblems.vue";
import ActivitySummary from "../../activity/components/ActivitySummary.vue";
import UploadComponent from "../../uploads/views/UploadComponent.vue";

const camelizeKeys: any = (obj: any) => {
  if (Array.isArray(obj)) {
    return obj.map((v) => camelizeKeys(v));
  } else if (obj != null && obj.constructor === Object) {
    return Object.keys(obj).reduce(
      (result, key) => ({
        ...result,
        [camelCase(key)]: camelizeKeys(obj[key]),
      }),
      {}
    );
  }
  return obj;
};

export default defineComponent({
  components: {
    ActivityProblems,
    ActivitySummary,
    UploadComponent,
  },
  data() {
    return {
      trackProgress: true,
      description: "" as string,
      isInformative: false as boolean,
      downloadAvailable: undefined as boolean | undefined,
      downloadError: "" as string,
      extractStatus: undefined as
        | {
            error: boolean;
            message: string;
          }
        | undefined,
      isDownloading: false as boolean,
      archiveId: "" as string,
      ticketId: "" as string,
      ticketStatus: undefined as string | undefined,
      uploadResponseCode: 0 as number,
      uploadAndChangeAccepted: false as boolean,
      fileProblems: [] as Array<any>,
      changes: [] as Array<any>,
      summary: undefined as any | undefined,
    };
  },
  computed: {
    downloadId(): string {
      return this.$route.params.downloadId[0];
    },
    uploadStatus(): {
      success: boolean;
      warning: boolean;
      error: boolean;
      title: string;
      text: string;
    } {
      const status = {
        success: false,
        warning: false,
        error: false,
        title: "",
        text: "",
      };

      switch (this.uploadResponseCode) {
        case 0:
          status.text = "Even geduld a.u.b.";
          break;
        case 200:
        case 202:
          status.success = true;
          status.title = "Gelukt!";
          status.text =
            "Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen.";
          break;
        case 400:
          // show only fileProblems
          break;
        case 404:
          status.error = true;
          status.title = "Fout";
          status.text = "Het extractaanvraag werd niet gevonden.";
          break;
        case 500:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - er is een onbekende fout gebeurd.";
          break;
        case 1001:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Oplading wordt verwerkt, maar er is een probleem bij de opvolging.";
          break;
        case 1100:
        case 1101:
          status.title = "";
          status.text = "Oplading is succesvol ontvangen, bezig met verwerking.";
          break;
        case 1102:
          status.success = true;
          status.title = "Gelukt!";
          status.text = "Oplading is gelukt en de wijzigingen zijn aanvaard.";
          break;
        case 1103:
          status.success = true;
          status.title = "Gelukt!";
          status.text = "Oplading is gelukt maar er zijn geen wijzigingen gevonden.";
          break;
        default:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er is een onbekende fout opgetreden.";
          break;
      }

      return status;
    },
  },
  async mounted() {
    await this.waitUntilExtractDetailsIsAvailable();
    await this.waitForTicketComplete();
  },
  unmounted() {
    this.trackProgress = false;
  },
  methods: {
    async waitUntilExtractDetailsIsAvailable(): Promise<void> {
      while (!this.extractStatus) {
        await this.loadExtractDetails();

        if (!this.extractStatus) {
          await new Promise((resolve) => setTimeout(resolve, 1000));
        }
      }
    },
    async loadExtractDetails(): Promise<void> {
      try {
        let details = await PublicApi.Extracts.getDetails(this.downloadId);

        this.description = details.description;
        this.isInformative = details.isInformative;
        this.archiveId = details.archiveId;
        this.ticketId = details.ticketId;
        this.downloadAvailable = details.downloadAvailable;
        this.extractStatus = details.downloadAvailable
          ? {
              error: false,
              message: "",
            }
          : details.extractDownloadTimeoutOccurred
            ? {
                error: true,
                message: "Er was een probleem bij het aanmaken vah het extract, gelieve een nieuwe aan te vragen.",
              }
            : undefined;
      } catch (err: any) {
        console.error("Error getting extract details", err);

        this.downloadAvailable = false;
        if (err?.response?.status === 400) {
          // retry paar keer voor projectie tijd te geven
        } else if (err?.response?.status === 404) {
          // nog niet beschikbaar
        } else if (err?.response?.status === 410) {
          this.extractStatus = {
            error: true,
            message: "Het extract is niet langer beschikbaar.",
          };
        } else {
          this.extractStatus = {
            error: true,
            message: "Er is een probleem bij het ophalen van de extract details.",
          };
        }
      }
    },
    async waitForTicketComplete(): Promise<void> {
      if (!this.ticketId) {
        return;
      }

      try {
        while (this.trackProgress) {
          try {
            let ticketResult = await PublicApi.Ticketing.get(this.ticketId);
            this.ticketStatus = ticketResult.status;

            switch (ticketResult.status) {
              case "created":
                this.uploadResponseCode = 1100;
                break;
              case "pending":
                this.uploadResponseCode = 1101;
                break;
              case "complete":
                {
                  this.trackProgress = false;
                  let uploadResult = camelizeKeys(JSON.parse(ticketResult.result.json));
                  if (uploadResult.changes.length > 0) {
                    this.uploadResponseCode = 1102;
                    this.changes = uploadResult.changes;
                    this.summary = uploadResult.summary;
                  } else {
                    this.uploadResponseCode = 1103;
                  }
                  this.uploadAndChangeAccepted = true;
                }
                break;
              case "error":
                {
                  this.trackProgress = false;
                  let ticketError = JSON.parse(ticketResult.result.json);
                  let errors = [ticketError, ...(ticketError.Errors ?? [])];
                  errors = uniqBy(errors, (x) => `${x.ErrorCode}_${x.ErrorMessage}`);
                  let problems = errors.map((error) => {
                    let codeParts = (error.ErrorCode ?? "").split("_");
                    let file = codeParts.length > 1 ? codeParts[0] : null;
                    let code = codeParts.length > 1 ? codeParts[1] : codeParts[0];
                    let severity = code.startsWith("Warning") ? "Warning" : "Error";
                    let text = error.ErrorMessage;
                    return { file, code, severity, text };
                  });
                  let fileProblems = uniq(problems.map((x) => x.file)).map((file) => {
                    return {
                      file,
                      problems: orderBy(
                        problems.filter((p) => p.file === file),
                        "severity"
                      ),
                    };
                  });

                  this.uploadResponseCode = 400;
                  this.fileProblems = fileProblems;
                }
                break;
            }
          } catch (err: any) {
            if (!this.handle400Or404Error(err)) {
              console.error("Error getting ticket details", err);
              this.uploadResponseCode = 1001;
            }
            return;
          }

          await new Promise((resolve) => setTimeout(resolve, 3000));
        }
      } catch (err: any) {
        if (!this.handle400Or404Error(err)) {
          this.uploadResponseCode = 500;
          return;
        }
      }
    },
    handle400Or404Error(err: any): boolean {
      if (err?.response?.status === 400) {
        let validationErrors = err?.response?.data?.validationErrors;
        let fileProblems = Object.keys(validationErrors).map((key) => {
          let problems = validationErrors[key].map((validationError: any) => ({
            severity: (validationError.code ?? "").startsWith("Warning") ? "Warning" : "Error",
            text: validationError.reason,
          }));
          problems = orderBy(problems, "severity");

          return {
            file: key,
            problems,
          };
        });

        this.uploadResponseCode = 400;
        this.fileProblems = fileProblems;
        return true;
      }

      if (err?.response?.status === 404) {
        this.uploadResponseCode = 404;
        return true;
      }

      return false;
    },
    async downloadExtract(): Promise<void> {
      this.downloadError = "";
      this.isDownloading = true;
      try {
        if (featureToggles.usePresignedEndpoints) {
          await PublicApi.Extracts.downloadUsingPresignedUrl(this.downloadId);
        } else {
          await BackOfficeApi.Extracts.download(this.downloadId);
        }
      } catch (err: any) {
        console.error("Error downloading extract", err);
        if (err?.response?.status === 404) {
          this.downloadError = "Het extract is nog niet beschikbaar.";
        } else if (err?.response?.status === 410) {
          this.downloadError = "Het extract is niet langer beschikbaar.";
        } else {
          this.downloadError = "Er is een probleem bij het downloaden van het extract.";
        }
      } finally {
        this.isDownloading = false;
      }
    },
    async downloadUpload(): Promise<void> {
      this.isDownloading = true;
      try {
        if (featureToggles.usePresignedEndpoints) {
          await PublicApi.Uploads.downloadUsingPresignedUrl(this.archiveId);
        } else {
          await BackOfficeApi.Uploads.download(this.archiveId);
        }
      } catch (err: any) {
        console.error("Error downloading upload", err);
        if (err?.response?.status === 404) {
          this.downloadError = "De upload is nog niet beschikbaar.";
        } else if (err?.response?.status === 410) {
          this.downloadError = "De upload is niet langer beschikbaar.";
        } else {
          this.downloadError = "Er is een probleem bij het downloaden van de upload.";
        }
      } finally {
        this.isDownloading = false;
      }
    },
    async handleUploadComplete(args: any) {
      this.ticketId = args.ticketId;
      await this.waitForTicketComplete();
      await this.loadExtractDetails();
    },
  },
});
</script>

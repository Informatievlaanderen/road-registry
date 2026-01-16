<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <wr-h2>{{ extractTitle }}</wr-h2>
          </vl-column>

          <vl-column>
            <div v-if="!extract">
              <vl-region>
                <div v-vl-align:center>
                  <vl-loader message="Uw pagina is aan het laden" />
                </div>
              </vl-region>
            </div>

            <div v-if="extract && downloadAvailable === false && downloadStatusMessage">
              <vl-alert :mod-error="downloadStatusMessage.error">
                {{ downloadStatusMessage.message }}
              </vl-alert>
            </div>

            <div v-if="extract">
              <div>Aangevraagd op: {{ formatDate(extract.aangevraagdOp) }}</div>
              <div>Status: {{ status }}</div>

              <div>
                <br />
                <span v-if="downloadAvailable">
                  <vl-button v-if="isDownloading" mod-loading> Download extract... </vl-button>
                  <vl-button v-else @click="downloadExtract()"> Download extract </vl-button>
                </span>
                <span v-if="extract.uploadStatus" style="margin-left: 1rem">
                  <vl-button v-if="isDownloading" mod-loading> Download upload... </vl-button>
                  <vl-button v-else @click="downloadUpload()"> Download upload </vl-button>
                </span>
                <span v-if="userCanClose" style="margin-left: 1rem">
                  <vl-button v-if="trackProgress" mod-loading> Sluit extract... </vl-button>
                  <vl-button v-else @click="closeExtract()"> Sluit extract </vl-button>
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
                v-if="userCanUpload"
                :download-id="downloadId"
                @upload-start="handleUploadStart"
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
import { PublicApi } from "../../../services";
import ActivityProblems from "../../activity/components/ActivityProblems.vue";
import ActivitySummary from "../../activity/components/ActivitySummary.vue";
import UploadComponent from "./UploadComponent.vue";
import DateFormat from "@/core/utils/date-format";
import RoadRegistry from "@/types/road-registry";
import { AuthService } from "@/auth";

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
      extract: undefined as RoadRegistry.ExtractDetailsV2 | undefined,
      downloadAvailable: false as boolean,
      downloadError: "" as string,
      downloadStatusMessage: undefined as
        | {
            error: boolean;
            message: string;
          }
        | undefined,
      isDownloading: false as boolean,
      ticketId: "" as string,
      ticketStatus: undefined as string | undefined,
      ticketResponseCode: 0 as number,
      fileProblems: [] as Array<any>,
      changes: [] as Array<any>,
      summary: undefined as any | undefined,
    };
  },
  computed: {
    extractTitle(): string {
      return `Extract${this.extract?.informatief ? " (informatief)" : ""}${
        this.extract?.beschrijving ? `: ${this.extract?.beschrijving}` : ""
      }`;
    },
    downloadId(): string {
      return this.$route.params.downloadId as string;
    },
    status() {
      if (!this.extract) {
        return "";
      }

      if (this.extract.gesloten) {
        return "Gesloten";
      }

      if (this.extract.uploadStatus) {
        switch (this.extract.uploadStatus) {
          case "Processing":
            return "Verwerken";
          case "Rejected":
            return "Geweigerd";
          case "Accepted":
            return "Aanvaard";
        }
      }

      switch (this.extract.downloadStatus) {
        case "Preparing":
          return "Wordt voorbereid";
        case "Available":
          return "Beschikbaar";
        case "Error":
          return "Fout";
      }

      return "";
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

      switch (this.ticketResponseCode) {
        case 0:
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
        case 999:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Oplading wordt verwerkt, maar er is een probleem bij de opvolging.";
          break;
        case 1000:
        case 1001:
          status.title = "";
          status.text = "Extract wordt aangemaakt.";
          break;
        case 1002: // extract is beschikbaar
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
        case 1200:
          status.title = "";
          status.text = "Extract wordt gesloten.";
          break;
        default:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er is een onbekende fout opgetreden.";
          break;
      }

      return status;
    },
    userCanUpload() {
      return (
        !this.extract?.informatief && this.downloadAvailable && !this.extract?.gesloten && this.extract?.gedownloadOp
      );
    },
    userCanClose() {
      return (
        this.extract &&
        !this.extract.informatief &&
        !this.extract.gesloten &&
        this.downloadStatusMessage &&
        !this.trackProgress &&
        AuthService.userHasAnyContext([RoadRegistry.UserContext.Admin])
      );
    },
  },
  async mounted() {
    await this.waitUntilExtractDetailsIsAvailable();
    await this.waitForTicketComplete();
    await this.loadExtractDetails();
  },
  unmounted() {
    this.trackProgress = false;
  },
  methods: {
    async waitUntilExtractDetailsIsAvailable(): Promise<void> {
      while (!this.downloadStatusMessage && this.trackProgress) {
        await this.loadExtractDetails();

        if (!this.downloadStatusMessage) {
          await new Promise((resolve) => setTimeout(resolve, 1000));
        }
      }
    },
    formatDate(dateString: undefined | string): string {
      if (!dateString) {
        return "";
      }
      return DateFormat.format(dateString);
    },
    async loadExtractDetails(): Promise<void> {
      try {
        let details = await PublicApi.Extracts.V2.getDetails(this.downloadId);
        this.extract = details;
        this.ticketId = details.ticketId;
        this.downloadAvailable = details.downloadStatus == "Available";
        this.downloadStatusMessage =
          details.downloadStatus == "Available"
            ? {
                error: false,
                message: "",
              }
            : details.downloadStatus == "Error"
              ? {
                  error: true,
                  message: "Er was een probleem bij het aanmaken van het extract, gelieve een nieuwe aan te vragen.",
                }
              : undefined;
      } catch (err: any) {
        console.error("Error getting extract details", err);

        this.downloadAvailable = false;
        if (err?.response?.status === 404) {
          // nog niet beschikbaar
        } else if (err?.response?.status === 410) {
          this.downloadStatusMessage = {
            error: true,
            message: "Het extract is niet langer beschikbaar.",
          };
        } else {
          this.downloadStatusMessage = {
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

      this.trackProgress = true;

      try {
        while (this.trackProgress) {
          try {
            let ticketResult = await PublicApi.Ticketing.get(this.ticketId);
            this.ticketStatus = ticketResult.status;

            switch (ticketResult.metadata.action) {
              case "Upload":
                this.handleTicketForUpload(ticketResult);
                break;
              case "CloseExtract":
                this.handleTicketForClose(ticketResult);
                break;
              case "RequestExtract":
                this.handleTicketForDownload(ticketResult);
                break;
            }
          } catch (err: any) {
            if (!this.handle400Or404Error(err)) {
              console.error("Error getting ticket details", err);
              this.ticketResponseCode = 999;
            }
            return;
          }

          await new Promise((resolve) => setTimeout(resolve, 3000));
        }
      } catch (err: any) {
        if (!this.handle400Or404Error(err)) {
          this.ticketResponseCode = 500;
          return;
        }
      }
    },
    handleTicketForDownload(ticketResult: RoadRegistry.TicketDetails): void {
      switch (ticketResult.status) {
        case "created":
          this.ticketResponseCode = 1000;
          break;
        case "pending":
          this.ticketResponseCode = 1001;
          break;
        case "complete":
          this.trackProgress = false;
          this.ticketResponseCode = 1002;
          break;
        case "error":
          this.trackProgress = false;
          this.handleTicketError(ticketResult);
          break;
      }
    },
    handleTicketForUpload(ticketResult: RoadRegistry.TicketDetails): void {
      switch (ticketResult.status) {
        case "created":
          this.ticketResponseCode = 1100;
          break;
        case "pending":
          this.ticketResponseCode = 1101;
          break;
        case "complete":
          {
            this.trackProgress = false;
            let uploadResult = camelizeKeys(JSON.parse(ticketResult.result.json));

            if (uploadResult.changes.length > 0) {
              this.ticketResponseCode = 1102;
              this.changes = uploadResult.changes;
              this.summary = uploadResult.summary;
            } else {
              this.ticketResponseCode = 1103;
            }
          }
          break;
        case "error":
          this.trackProgress = false;
          this.handleTicketError(ticketResult);
          break;
      }
    },
    handleTicketForClose(ticketResult: RoadRegistry.TicketDetails): void {
      switch (ticketResult.status) {
        case "created":
        case "pending":
          this.ticketResponseCode = 1200;
          break;
        case "complete":
          this.trackProgress = false;
          this.ticketResponseCode = 0;
          break;
        case "error":
          this.trackProgress = false;
          this.handleTicketError(ticketResult);
          break;
      }
    },
    handleTicketError(ticketResult: RoadRegistry.TicketDetails) {
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

      this.ticketResponseCode = 400;
      this.fileProblems = fileProblems;
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

        this.ticketResponseCode = 400;
        this.fileProblems = fileProblems;
        return true;
      }

      if (err?.response?.status === 404) {
        this.ticketResponseCode = 404;
        return true;
      }

      return false;
    },
    async downloadExtract(): Promise<void> {
      this.downloadError = "";
      this.isDownloading = true;
      try {
        await PublicApi.Extracts.V2.downloadExtract(this.downloadId);

        if (!this.extract?.gedownloadOp) {
          await this.loadExtractDetails();
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
        await PublicApi.Extracts.V2.downloadUpload(this.downloadId);
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
    async closeExtract(): Promise<void> {
      let response = await PublicApi.Extracts.V2.close(this.downloadId);
      let ticketId = response.ticketUrl.split("/").reverse()[0];
      await this.waitForTicketAndRefreshExtractDetails(ticketId);
    },
    resetUploadFeedback() {
      this.ticketResponseCode = 0;
      this.fileProblems = [];
      this.changes = [];
    },
    handleUploadStart() {
      this.resetUploadFeedback();
    },
    async handleUploadComplete(args: any) {
      await this.waitForTicketAndRefreshExtractDetails(args.ticketId);
    },
    async waitForTicketAndRefreshExtractDetails(ticketId: string) {
      this.ticketId = ticketId;
      await this.waitForTicketComplete();
      await this.loadExtractDetails();
    },
  },
});
</script>

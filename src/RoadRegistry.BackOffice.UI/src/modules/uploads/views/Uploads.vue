<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <wr-h2>Opladen</wr-h2>
          </vl-column>

          <!-- 
          <vl-column>
            <vl-image src="https://picsum.photos/1600/400?image=1048" alt="Bouwen in Brussel" />
          </vl-column>
         -->

          <vl-column>
            <!-- <vl-doormat v-if="isUploading" title="Upload bezig">
              Het door u geselecteerde zip‑bestand wordt geupload.
            </vl-doormat> -->
            <vl-upload
              ref="vlUpload"
              id="upload-component"
              name="upload-component"
              url="#"
              upload-drag-text="Selecteer het zip-bestand met bronbestanden en doelbestanden voor het extract."
              upload-label="Archief opladen"
              :auto-process="false"
              :options="options"
              :mod-success="uploadResult.uploadResponseCode && alertInfo.success"
              :mod-error="uploadResult.uploadResponseCode && (alertInfo.error || alertInfo.warning)"
              :mod-disabled="isUploading || isProcessing"
              @upload-success="isUploading = false"
              @upload-complete="isUploading = false"
              @upload-canceled="isUploading = false"
              @upload-file-added="processing"
              @upload-file-added-manually="processing"
            />

            <div v-if="uploadResult.uploadResponseCode && alertInfo.text">
              <vl-alert
                :title="alertInfo.title"
                :mod-success="alertInfo.success"
                :mod-warning="alertInfo.warning"
                :mod-error="alertInfo.error"
              >
                <p>{{ alertInfo.text }}</p>
                <p v-if="uploadResult.changeRequestId">
                  U kan de vooruitgang volgen via <a :href="redirectToActivityPageUrl">Activiteit</a>. U wordt binnen 5
                  seconden doorverwezen.
                </p>
              </vl-alert>
            </div>

            <div v-if="alertInfo.fileProblems && alertInfo.fileProblems.length > 0">
              <div v-for="fileProblem in alertInfo.fileProblems" :key="fileProblem.file">
                <br />
                <h3 v-if="fileProblem.file">
                  <strong>{{ fileProblem.file.toUpperCase() }}</strong>
                </h3>

                <ActivityProblems :problems="fileProblem.problems" />
              </div>
            </div>

            <div v-if="uploadResult.changes && uploadResult.changes.length > 0">
              <!-- <div class="vl-grid vl-grid--align-center grid-summary">
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.roadNodes.added }}
                      </div>
                      <div class="vl-infotext__text">Toegevoegde wegknopen</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.roadNodes.modified }}
                      </div>
                      <div class="vl-infotext__text">Gewijzigde wegknopen</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.roadNodes.removed }}
                      </div>
                      <div class="vl-infotext__text">Verwijderde wegknopen</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.roadSegments.added }}
                      </div>
                      <div class="vl-infotext__text">Toegevoegde wegsegmenten</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.roadSegments.modified }}
                      </div>
                      <div class="vl-infotext__text">Gewijzigde wegsegmenten</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.roadSegments.removed }}
                      </div>
                      <div class="vl-infotext__text">Verwijderde wegsegmenten</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.added }}
                      </div>
                      <div class="vl-infotext__text">Toegevoegde ongelijkgrondse kruisingen</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.modified }}
                      </div>
                      <div class="vl-infotext__text">Gewijzigde ongelijkgrondse kruisingen</div>
                    </div>
                  </div>
                </div>
                <div class="vl-col--4-12 vl-u-align-center">
                  <div class="vl-infotext-wrapper">
                    <div class="vl-infotext">
                      <div class="vl-infotext__value" data-vl-infotext-value>
                        {{ activity.changeFeedContent.content.summary.gradeSeparatedJunctions.removed }}
                      </div>
                      <div class="vl-infotext__text">Verwijderde ongelijkgrondse kruisingen</div>
                    </div>
                  </div>
                </div>
              </div>
               -->
              <ActivitySummary v-if="uploadResult.summary" :summary="uploadResult.summary" />
              <div v-for="change in uploadResult.changes" :key="change.change">
                <h3>
                  <strong>{{ change.change }}</strong>
                </h3>
                <ActivityProblems :problems="change.problems" />
                <br />
              </div>
            </div>
          </vl-column>
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import Vue from "vue";
import { orderBy, uniq, uniqBy, camelCase } from "lodash";
import { PublicApi } from "../../../services";
import ActivityProblems from "../../activity/components/ActivityProblems.vue";
import ActivitySummary from "../../activity/components/ActivitySummary.vue";
import RoadRegistry from "@/types/road-registry";
import axios from "axios";

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

export default Vue.extend({
  components: {
    ActivityProblems,
    ActivitySummary,
  },
  data() {
    return {
      isUploading: false,
      isProcessing: false,
      uploadResult: {
        uploadResponseCode: undefined,
        fileProblems: [],
        changeRequestId: undefined,
        changes: [],
        summary: undefined,
      } as {
        uploadResponseCode: number | undefined;
        fileProblems: Array<any> | undefined;
        changeRequestId: string | undefined;
        changes: Array<any> | undefined;
        summary: any | undefined;
      },
      redirectToActivityPageTimeout: null as any,
    };
  },
  computed: {
    options() {
      return {
        uploadMultiple: false,
        autoQueue: false,
        autoProcessQueue: false,
        maxFiles: 1,
        maxFilesize: 28672000, //28MB
        acceptedFiles: "application/zip",
        paramName: "archive",
        headers: {},
      };
    },
    alertInfo(): {
      success: boolean;
      warning: boolean;
      error: boolean;
      title: string;
      text: string;
      fileProblems: Array<any> | undefined;
    } {
      const status = {
        success: false,
        warning: false,
        error: false,
        title: "",
        text: "",
        fileProblems: [] as Array<any> | undefined,
      };

      if (this.uploadResult.fileProblems && this.uploadResult.fileProblems.length > 0) {
        status.fileProblems = this.uploadResult.fileProblems;
        return status;
      }

      switch (this.uploadResult.uploadResponseCode) {
        case 1:
          status.error = true;
          status.title = "Opgelet!";
          status.text = "Het max. toegelaten bestand groote is 28MB";
          break;
        case 2:
        case 415:
          status.error = true;
          status.title = "Opgelet!";
          status.text = "Enkel .zip bestanden zijn toegelaten.";
          break;
        case 200:
        case 202:
          status.success = true;
          status.title = "Gelukt!";
          status.text =
            "Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen.";
          break;
        case 400:
          status.warning = true;
          status.title = "Technische storing";
          status.text = "Door een technische storing is dit loket tijdelijk niet beschikbaar.";
          break;
        case 404:
          status.error = true;
          status.title = "Opgelet!";
          status.text = "Het extractaanvraag werd niet gevonden.";
          break;
        case 408:
          status.warning = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - de operatie nam teveel tijd in beslag.";
          break;
        case 500:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - er is een onbekende fout gebeurd.";
          break;
        case 503:
          status.error = true;
          status.title = "Technische storing";
          status.text =
            "Opladen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw.";
          break;
        case 1000:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er is een onbekend probleem bij het opladen gebeurd.";
          break;
        case 1001:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Oplading wordt verwerkt, maar er is een probleem bij de opvolging.";
          break;
        case 1100:
        case 1101:
          status.title = "";
          status.text = "Oplading is succesvol ontvangen, wachten op verwerking.";
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
          status.text = "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website.";
          break;
      }

      return status;
    },
    redirectToActivityPageUrl() {
      return this.$router.resolve({ name: "activiteit", query: { filter: this.uploadResult.changeRequestId } }).href;
    },
  },
  beforeDestroy() {
    if (this.redirectToActivityPageTimeout) {
      clearTimeout(this.redirectToActivityPageTimeout);
    }
    this.endUpload();
  },
  methods: {
    async processing(file: File) {
      this.startUpload();
      try {
        this.uploadResult = {
          uploadResponseCode: undefined,
          fileProblems: undefined,
          changeRequestId: undefined,
          changes: undefined,
          summary: undefined,
        };
        this.uploadResult = await this.uploadFile(file);

        if (this.uploadResult.changeRequestId) {
          this.redirectToActivityPageTimeout = setTimeout(this.redirectToActivityPage, 5000);
        }
      } finally {
        this.endUpload();
      }
    },
    async uploadFile(file: File): Promise<{
      uploadResponseCode: number | undefined;
      fileProblems: Array<any> | undefined;
      changeRequestId: string | undefined;
      changes: Array<any> | undefined;
      summary: any | undefined;
    }> {
      const allowedFileTypes = ["application/zip", "application/x-zip-compressed"];
      if (!allowedFileTypes.includes(file.type)) {
        return {
          uploadResponseCode: 2,
          fileProblems: undefined,
          changeRequestId: undefined,
          changes: undefined,
          summary: undefined,
        };
      }

      if (file.size > this.options.maxFilesize) {
        return {
          uploadResponseCode: 1,
          fileProblems: undefined,
          changeRequestId: undefined,
          changes: undefined,
          summary: undefined,
        };
      }

      try {
        let presignedUploadResponse = await PublicApi.Uploads.uploadUsingPresignedUrl(file, file.name);

        if (!presignedUploadResponse) {
          return {
            uploadResponseCode: 1000,
            fileProblems: undefined,
            changeRequestId: undefined,
            changes: undefined,
            summary: undefined,
          };
        }

        while (this.isProcessing) {
          try {
            var ticketResult = await axios
              .create()
              .get<RoadRegistry.GetTicketResponse>(presignedUploadResponse.ticketUrl, {
                params: { t: new Date().getTime() },
              });

            switch (ticketResult.data.status) {
              case "created":
                this.uploadResult = {
                  uploadResponseCode: 1100,
                  fileProblems: undefined,
                  changeRequestId: undefined,
                  changes: undefined,
                  summary: undefined,
                };
                break;
              case "pending":
                this.uploadResult = {
                  uploadResponseCode: 1101,
                  fileProblems: undefined,
                  changeRequestId: undefined,
                  changes: undefined,
                  summary: undefined,
                };
                break;
              case "complete":
                let uploadResult = camelizeKeys(JSON.parse(ticketResult.data.result.json));

                return {
                  uploadResponseCode: uploadResult.changes.length > 0 ? 1102 : 1103,
                  fileProblems: undefined,
                  changeRequestId: undefined,
                  changes: uploadResult.changes,
                  summary: uploadResult.changes.length > 0 ? uploadResult.summary : undefined,
                };
              case "error":
                let ticketError = JSON.parse(ticketResult.data.result.json);
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
                return {
                  uploadResponseCode: 400,
                  fileProblems,
                  changeRequestId: undefined,
                  changes: undefined,
                  summary: undefined,
                };
            }

            await new Promise((resolve) => setTimeout(resolve, 3000));
          } catch (err) {
            console.error("Error getting ticket details", err);
            return {
              uploadResponseCode: 1001,
              fileProblems: undefined,
              changeRequestId: undefined,
              changes: undefined,
              summary: undefined,
            };
          }
        }
      } catch (err: any) {
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
          return {
            uploadResponseCode: 400,
            fileProblems,
            changeRequestId: undefined,
            changes: undefined,
            summary: undefined,
          };
        }

        if (err?.response?.status === 404) {
          return {
            uploadResponseCode: 404,
            fileProblems: undefined,
            changeRequestId: undefined,
            changes: undefined,
            summary: undefined,
          };
        }

        console.error("Upload failed", err);
        return {
          uploadResponseCode: 500,
          fileProblems: undefined,
          changeRequestId: undefined,
          changes: undefined,
          summary: undefined,
        };
      }

      return {
        uploadResponseCode: undefined,
        fileProblems: undefined,
        changeRequestId: undefined,
        changes: undefined,
        summary: undefined,
      };
    },
    emptyQueue(): void {
      const dropZone = this.$refs.vlUpload as any as { hasFiles: boolean; removeFiles: () => void };
      if (dropZone.hasFiles) {
        dropZone.removeFiles();
      }
    },
    startUpload(): void {
      this.emptyQueue();
      this.isUploading = true;
      this.isProcessing = true;
    },
    endUpload(): void {
      this.emptyQueue();
      this.isUploading = false;
      this.isProcessing = false;
    },
    redirectToActivityPage() {
      this.$router.push({ name: "activiteit", query: { filter: this.uploadResult.changeRequestId } });
    },
  },
});
</script>

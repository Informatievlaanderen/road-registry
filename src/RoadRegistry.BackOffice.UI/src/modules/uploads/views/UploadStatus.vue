<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <wr-h2>Oplading status</wr-h2>
          </vl-column>

          <vl-column>
            <div v-if="alertInfo.text">
              <vl-alert
                :title="alertInfo.title"
                :mod-success="alertInfo.success"
                :mod-warning="alertInfo.warning"
                :mod-error="alertInfo.error"
              >
              {{ alertInfo.text }}
              </vl-alert>
            </div>

            <div v-if="fileProblems && fileProblems.length > 0">
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
          </vl-column>
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import Vue from "vue";
import { orderBy, uniq, uniqBy, camelCase } from "lodash";
import { PublicApi } from "@/services";
import ActivityProblems from "../../activity/components/ActivityProblems.vue";
import ActivitySummary from "../../activity/components/ActivitySummary.vue";

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
      trackProgress: true,
      ticketStatus: undefined as string | undefined,
      uploadResponseCode: 0 as number,
      fileProblems: [] as Array<any>,
      changes: [] as Array<any>,
      summary: undefined as any | undefined,
    };
  },
  computed: {
    alertInfo(): {
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
  mounted() {
    this.waitForTicketComplete();
  },
  destroyed() {
    this.trackProgress = false;
  },
  methods: {
    async waitForTicketComplete(): Promise<void> {
      try {
        while (this.trackProgress) {
          try {
            var ticketResult = await PublicApi.Ticketing.get(this.$route.params.ticketId);
            this.ticketStatus = ticketResult.status;

            switch (ticketResult.status) {
              case "created":
                this.uploadResponseCode = 1100;
                break;
              case "pending":
                this.uploadResponseCode = 1101;
                break;
              case "complete":
                this.trackProgress = false;
                let uploadResult = camelizeKeys(JSON.parse(ticketResult.result.json));
                if (uploadResult.changes.length > 0) {
                  this.uploadResponseCode = 1102;
                  this.changes = uploadResult.changes;
                  this.summary = uploadResult.summary;
                } else {
                  this.uploadResponseCode = 1103;
                }
                break;
              case "error":
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
  },
});
</script>

<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <wr-h2>Extracten</wr-h2>
          </vl-column>
          <vl-column>
            <div v-if="!firstLoadCompleted">
              <vl-region>
                <div v-vl-align:center>
                  <vl-loader message="Uw pagina is aan het laden" />
                </div>
              </vl-region>
            </div>
            <div v-else>
              <label class="vl-checkbox" for="eigen-extracten">
                <input class="vl-checkbox__toggle" type="checkbox" id="eigen-extracten" v-model="eigenExtracten" />
                <span class="vl-checkbox__label">
                  <i class="vl-checkbox__box" aria-hidden="true"></i>
                  Eigen extracten
                </span>
              </label>
              <table class="extracts-table">
                <colgroup>
                  <col class="fit" />
                  <col class="fit" />
                  <col class="fit" />
                  <col class="grow" />
                </colgroup>
                <thead>
                  <tr>
                    <th></th>
                    <th>Aangevraagd op</th>
                    <th>Status</th>
                    <th>Beschrijving</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="extract in extracts"
                    :key="extract.downloadId"
                    :class="{ closed: extract.closed, error: extractIsInError(extract) }"
                  >
                    <td><vl-button @click="openDetails(extract.downloadId)"> Details </vl-button></td>
                    <td>{{ formatDate(extract.requestedOn) }}</td>
                    <td>{{ getStatus(extract) }}</td>
                    <td>{{ getDescription(extract) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </vl-column>
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import Vue from "vue";
import RoadRegistry from "@/types/road-registry";
import { PublicApi } from "../../../services";
import DateFormat from "@/core/utils/date-format";

export default Vue.extend({
  components: {},
  data() {
    return {
      extracts: [] as RoadRegistry.ExtractListItem[],
      eigenExtracten: true,
      firstLoadCompleted: false,
    };
  },
  computed: {},
  async mounted() {
    const eigenExtracten = localStorage.getItem("ExtractList.eigenExtracten");
    if (eigenExtracten !== null) {
      this.eigenExtracten = eigenExtracten === "true";
    }

    await this.refreshExtracten();
    this.firstLoadCompleted = true;

    //TODO-pr add auto-refresh to track those non-informative not completed yet

    //TODO-pr TBD: moeten nieuwe er vanzelf bij komen? ik denk van niet
  },
  destroyed() {
    //TODO-pr clean up any intervals or listeners
  },
  watch: {
    eigenExtracten() {
      localStorage.setItem("ExtractList.eigenExtracten", this.eigenExtracten.toString());
      this.refreshExtracten();
    },
  },
  methods: {
    async refreshExtracten() {
      const response = await PublicApi.Extracts.V2.getList(this.eigenExtracten);
      this.extracts = response.items;
    },
    openDetails(downloadId: string) {
      this.$router.push({ name: "extractDetailsV2", params: { downloadId: downloadId } });
    },
    getStatus(extract: RoadRegistry.ExtractListItem) {
      if (extract.closed) {
        return "Gesloten";
      }

      if (extract.uploadStatus) {
        switch (extract.uploadStatus) {
          case "Processing":
            return "Verwerken";
          case "Rejected":
            return "Geweigerd";
          case "Accepted":
            return "Aanvaard";
        }
      }

      switch (extract.downloadStatus) {
        case "Preparing":
          return "Wordt voorbereid";
        case "Available":
          return "Beschikbaar";
        case "Error":
          return "Fout";
      }

      return "";
    },
    extractIsInError(extract: RoadRegistry.ExtractListItem) {
      return extract.downloadStatus === "Error" || extract.uploadStatus == "Rejected";
    },
    formatDate(iso: string) {
      return DateFormat.format(iso);
    },
    getDescription(extract: RoadRegistry.ExtractListItem) {
      if (extract.isInformative) {
        return `(informatief) ${extract.description}`;
      }

      return `${extract.description}`;
    },
  },
});
</script>

<style lang="scss">
.extracts-table {
  width: 100%;
  table-layout: auto;

  col.fit {
    width: 1%;
  }
  th {
    text-align: left;
    font-weight: bold;
  }
  td:nth-child(-n + 3),
  th:nth-child(-n + 3) {
    white-space: nowrap;
  }
  col.grow {
    width: auto;
  }

  td,
  th {
    padding: 0.25rem 0.5rem;
  }

  tr.closed {
    background-color: #cfd5dd;
  }
  tr.error {
    background-color: #d2373c;
  }
  tr.warning {
    background-color: #ffa10a;
  }
}
</style>

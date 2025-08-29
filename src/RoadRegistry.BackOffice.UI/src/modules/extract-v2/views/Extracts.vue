<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <div>
              <vl-button @click="extractAanvragen()"> Nieuw extract aanvragen </vl-button>
            </div>
          </vl-column>

          <vl-column>
            <div v-if="!initializeCompleted">
              <vl-region>
                <div v-vl-align:center>
                  <vl-loader message="Uw pagina is aan het laden" />
                </div>
              </vl-region>
            </div>
            <div v-else>
              <label class="vl-checkbox" for="eigen-extracten">
                <input
                  class="vl-checkbox__toggle"
                  type="checkbox"
                  id="eigen-extracten"
                  v-model="eigenExtracten"
                  :disabled="isLoading"
                />
                <span class="vl-checkbox__label" :disabled="isLoading ? 'disabled' : undefined">
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
                    :class="{ closed: extract.gesloten, error: extractIsInError(extract) }"
                  >
                    <td><vl-button @click="openDetails(extract.downloadId)"> Details </vl-button></td>
                    <td>{{ formatDate(extract.aangevraagdOp) }}</td>
                    <td>{{ getStatus(extract) }}</td>
                    <td>{{ getDescription(extract) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </vl-column>

          <vl-column>
            <div v-vl-flex v-vl-flex:align-center>
              <vl-button mod-loading v-if="isLoading"></vl-button>
              <vl-button v-else-if="dataBeschikbaar" @click="loadMore()"> Meer ... </vl-button>
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
      isLoading: false,
      initializeCompleted: false,
      nextPage: 0,
      dataBeschikbaar: false,
    };
  },
  computed: {},
  async mounted() {
    const eigenExtracten = localStorage.getItem("ExtractList.eigenExtracten");
    if (eigenExtracten !== null) {
      this.eigenExtracten = eigenExtracten === "true";
    }

    await this.loadInitialExtracts();
    this.initializeCompleted = true;

    //TODO-pr add auto-refresh to track those non-informative not completed yet
    // via de extract list endpoint, first page
    // ook nieuwe extracten vanzelf toevoegen
    // clean up any intervals or listeners in destroyed
  },
  destroyed() {},
  watch: {
    async eigenExtracten() {
      if (this.initializeCompleted) {
        localStorage.setItem("ExtractList.eigenExtracten", this.eigenExtracten.toString());
        this.loadInitialExtracts();
      }
    },
  },
  methods: {
    async loadInitialExtracts() {
      while (this.isLoading) {
        // wait
      }

      this.isLoading = true;
      try {
        this.extracts = [];
        this.nextPage = 0;
        this.dataBeschikbaar = false;
        await this.loadNextPage();
      } finally {
        this.isLoading = false;
      }
    },
    async loadMore() {
      if (this.isLoading) {
        return;
      }

      this.isLoading = true;
      try {
        await this.loadNextPage();
      } finally {
        this.isLoading = false;
      }
    },
    async loadNextPage() {
      const response = await PublicApi.Extracts.V2.getList(this.eigenExtracten, this.nextPage);
      const knownIds = this.extracts.map((x) => x.downloadId);
      this.extracts.push(...response.items.filter((x) => !knownIds.includes(x.downloadId)));
      this.dataBeschikbaar = response.dataBeschikbaar;
      this.nextPage++;
    },
    extractAanvragen() {
      this.$router.push({ name: "requestExtract" });
    },
    openDetails(downloadId: string) {
      this.$router.push({ name: "extractDetailsV2", params: { downloadId: downloadId } });
    },
    getStatus(extract: RoadRegistry.ExtractListItem) {
      if (extract.gesloten) {
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
      if (extract.informatief) {
        return `(informatief) ${extract.beschrijving}`;
      }

      return `${extract.beschrijving}`;
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
    color: white;
  }
  tr.warning {
    background-color: #ffa10a;
  }
}
</style>

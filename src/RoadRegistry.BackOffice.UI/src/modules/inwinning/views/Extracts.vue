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
              <table class="extracts-table" v-if="extracts.length">
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
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import RoadRegistry from "@/types/road-registry";
import { PublicApi } from "../../../services";
import DateFormat from "@/core/utils/date-format";

const waitWhile = async (condition: () => boolean) => {
  while (condition()) {
    await new Promise((resolve) => setTimeout(resolve, 25));
  }
};

export default defineComponent({
  components: {},
  data() {
    return {
      extracts: [] as RoadRegistry.ExtractListItem[],
      isLoading: false,
      isLoadingInBackground: false,
      initializeCompleted: false,
      autoRefreshInterval: null as any,
    };
  },
  async mounted() {
    await this.loadInitialExtracts();
    this.initializeCompleted = true;
    this.startAutoRefresh();
  },
  beforeUnmount() {
    this.stopAutoRefresh();
  },
  methods: {
    startAutoRefresh() {
      this.stopAutoRefresh();
      this.autoRefreshInterval = setInterval(this.loadFirstPageInBackground, 10000);
    },
    stopAutoRefresh() {
      if (this.autoRefreshInterval) {
        clearInterval(this.autoRefreshInterval);
      }
    },
    async loadFirstPageInBackground(): Promise<any> {
      if (this.isLoading || this.isLoadingInBackground) {
        console.warn("Skipping load, loading is still in progress");
        return;
      }

      this.stopAutoRefresh();
      this.isLoadingInBackground = true;
      try {
        const response = await PublicApi.Inwinning.getList();
        if (this.isLoading) {
          return;
        }

        this.extracts = [
          ...response.items,
          ...this.extracts.filter((x) => !response.items.find((y) => y.downloadId === x.downloadId)),
        ];
      } finally {
        this.isLoadingInBackground = false;
        this.startAutoRefresh();
      }
    },
    async loadInitialExtracts() {
      await waitWhile(() => this.isLoading || this.isLoadingInBackground);

      this.isLoading = true;
      try {
        this.extracts = [];
        await this.loadNextPage();
      } finally {
        this.isLoading = false;
      }
    },
    async loadNextPage() {
      const response = await PublicApi.Inwinning.getList();
      const knownIds = this.extracts.map((x) => x.downloadId);
      this.extracts.push(...response.items.filter((x) => !knownIds.includes(x.downloadId)));
    },
    extractAanvragen() {
      this.$router.push({ name: "requestInwinningExtract" });
    },
    openDetails(downloadId: string) {
      this.$router.push({ name: "inwinningExtractDetails", params: { downloadId: downloadId } });
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

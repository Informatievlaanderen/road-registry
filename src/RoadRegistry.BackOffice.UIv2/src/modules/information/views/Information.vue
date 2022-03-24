<template>
  <div>
    <table class="vl-data-table vl-data-table--grid">
      <thead>
        <tr>
          <th>Gegeven</th>

          <th>Waarde</th>
        </tr>
      </thead>
      <tbody>
        
        <tr>
          <td data-title="Organisaties"># organisaties</td>
          <td>
            <div v-if="isLoading">
              <div class="vl-skeleton" style="">
                <div class="vl-skeleton-bone"></div>
              </div>
            </div>
            <div v-else>
              {{ this.information.organizationCount }}
            </div>
          </td>
        </tr>
        
        <tr>
          <td data-title="Wegknopen"># wegknopen</td>
          <td>
            <div v-if="isLoading">
              <div class="vl-skeleton" style="">
                <div class="vl-skeleton-bone"></div>
              </div>
            </div>
            <div v-else>
              {{ this.information.roadNodeCount }}
            </div>
          </td>
        </tr>
        
        <tr>
          <td data-title="Wegsegmenten"># wegsegmenten</td>
          <td>
            <div v-if="isLoading">
              <div class="vl-skeleton" style="">
                <div class="vl-skeleton-bone"></div>
              </div>
            </div>
            <div v-else>
              {{ this.information.roadSegmentCount }}
            </div>
          </td>
        </tr>
        
        <tr>
          <td data-title="Ongelijkgrondse Kruisingen"># ongelijkgrondse kruisingen</td>
          <td>
            <div v-if="isLoading">
              <div class="vl-skeleton" style="">
                <div class="vl-skeleton-bone"></div>
              </div>
            </div>
            <div v-else>
              {{ this.information.gradeSeparatedJunctionCount }}
            </div>
          </td>
        </tr>

      </tbody>
    </table>
  </div>
</template>


<script lang="ts">
import Vue from "vue";
import { PublicApi } from "../../../services";
import RoadRegistry from "../../../types/road-registry";

export default Vue.extend({
  data() {
    return {
      isLoading: true,
      information: {} as RoadRegistry.RoadNetworkInformationResponse,
    };
  },
  async mounted() {
    this.information = await PublicApi.Information.getInformation();
    this.isLoading = false;
  },
});
</script>

<style lang="scss">
</style>
